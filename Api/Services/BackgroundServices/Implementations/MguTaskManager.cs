﻿using System;
using System.Collections.Generic;
using System.Linq;
using Api.Data;
using Api.Helpers;
using Api.Models;
using Api.Models.Enums;
using Api.Models.NvidiaSmiModels;
using Api.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices.Implementations;

public class MguTaskManager : ITaskManagerImplementation
{
    private static Dictionary<Process, TicketTask> _processToTaskDictionary = new Dictionary<Process, TicketTask>();

    private readonly IServiceProvider _services;
    private readonly QueueService _queueService;
    private readonly SshService _sshService;
    private readonly SftpService _sftpService;
    private readonly GlobalParametersService _globalParametersService;
    private readonly string _programVersionsFolder;

    public MguTaskManager(
        IServiceProvider services,
        QueueService queueService,
        SshService sshService,
        SftpService sftpService,
        GlobalParametersService globalParametersService,
        IOptions<ProgramVersionsFolder> programVersionsFolder)
    {
        _services = services;
        _queueService = queueService;
        _sshService = sshService;
        _sftpService = sftpService;
        _globalParametersService = globalParametersService;
        _programVersionsFolder = programVersionsFolder.Value.Path;
    }
    
    public object GetParsedStatus()
    {
        string status = _sshService.RunCustomCommand("nvidia-smi").GetAwaiter().GetResult();

        return NvidiaSmiParser.ParseNvidiaSmiResult(status);
    }

    public async Task RunTasksFromQueue(object status)
    {
        NvidiaSmiModel nvidiaSmiResult = (NvidiaSmiModel) status;
        
        while (nvidiaSmiResult.HasFreeGpu)
        {
            TicketTask taskFromQueue = _queueService.GetFromQueue();

            if (taskFromQueue is null)
            {
                break;
            }

            string programPath = _programVersionsFolder + '/' + taskFromQueue.ProgramVersion;

            string jobFile = taskFromQueue.FileNames.Select(filename => filename.Name)
                .First(name => name.Contains(".job"));

            int freeGpu = nvidiaSmiResult.Gpus
                .First(gpu => !nvidiaSmiResult.Processes.Select(proc => proc.Gpu).Contains(gpu.Id)).Id;

            if (!_globalParametersService
                .GetGlobalParameters()
                .GlobalParametersDictionary
                .TryGetValue(freeGpu.ToString(), out string streams))
            {
                streams = "1";
            }
            
            string cdCommand = $"cd {taskFromQueue.DirectoryPath}";
            string mainCommand = $"{programPath} -cfg {jobFile} -gpu {freeGpu} -streams {streams} >out.txt 2>&1 \\&";
            string disownCommand = "disown -r";

            _sshService.RunCustomCommand($"{cdCommand} && {mainCommand} && {disownCommand}");

            Process startedProcess = ((NvidiaSmiModel) GetParsedStatus()).Processes.FirstOrDefault(proc => proc.Gpu == freeGpu);

            if (startedProcess is null)
            {
                _queueService.AddToFinishedList(taskFromQueue);
                
                using (IServiceScope serviceScope = _services.CreateScope())
                {
                    Context db = (Context) serviceScope.ServiceProvider.GetService(typeof(Context)) ??
                                 throw new InvalidOperationException();
                    
                    TicketTask trackingTask = db.Find<TicketTask>(taskFromQueue.Id) ?? throw new InvalidOperationException();
                    
                    trackingTask.Status = TaskStatuses.Done;
                    
                    string[] outputFiles = _sftpService.ListOfFiles(trackingTask.DirectoryPath, trackingTask.FileNames.Select(filename => filename.Name).ToArray());
                    
                    foreach (string outputFile in outputFiles)
                    {
                        trackingTask.FileNames.Add(new Filename() {Name = outputFile, Inputed = false});
                    }
                    
                    await db.SaveChangesAsync();
                }
            }
            else
            {
                _queueService.AddToRunningTasks(taskFromQueue);
                
                _processToTaskDictionary.Add(startedProcess, taskFromQueue);

                using (IServiceScope serviceScope = _services.CreateScope())
                {
                    Context db = (Context) serviceScope.ServiceProvider.GetService(typeof(Context)) ??
                                 throw new InvalidOperationException();
                    
                    TicketTask trackingTask = db.Find<TicketTask>(taskFromQueue.Id) ?? throw new InvalidOperationException();
                    
                    trackingTask.Status = TaskStatuses.InProgress;

                    await db.SaveChangesAsync();
                }
            }
        }
    }

    public async Task CheckRunningTasks(object status)
    {
        NvidiaSmiModel nvidiaSmiResult = (NvidiaSmiModel) status;
        
        List<TicketTask> runningTasksToUpdate = _queueService.GetRunningTasks();

        Dictionary<Process, TicketTask> runningProcessesToUpdate = _processToTaskDictionary.Where(kvp => runningTasksToUpdate.Contains(kvp.Value))
            .ToDictionary(x => x.Key, y => y.Value);
        
        TicketTask[] tasksToKill = _queueService.GetKillList();
        
        using (IServiceScope serviceScope = _services.CreateScope())
        {
            Context db = (Context) serviceScope.ServiceProvider.GetService(typeof(Context)) ??
                         throw new InvalidOperationException();
            
            foreach (KeyValuePair<Process, TicketTask> kvp in runningProcessesToUpdate)
            {
                if (!nvidiaSmiResult.Processes.Contains(kvp.Key))
                {
                    _queueService.RemoveRunningTask(kvp.Value);
                    
                    _queueService.AddToFinishedList(kvp.Value);
                    
                    _processToTaskDictionary.Remove(_processToTaskDictionary.First(kvpair => kvpair.Key.Equals(kvp.Key)).Key);
                    
                    TicketTask trackingTask = db.Find<TicketTask>(kvp.Value.Id) ?? throw new InvalidOperationException();
                    
                    trackingTask.Status = TaskStatuses.Done;
                    
                    string[] outputFiles = _sftpService.ListOfFiles(trackingTask.DirectoryPath,
                        trackingTask.FileNames.Select(filename => filename.Name).ToArray());
                    
                    foreach (string outputFile in outputFiles)
                    {
                        trackingTask.FileNames.Add(new Filename() {Name = outputFile, Inputed = false});
                    }
                }
                else
                {
                    foreach (TicketTask task in tasksToKill)
                    {
                        if (runningProcessesToUpdate.ContainsValue(task))
                        {
                            _sshService.RunCustomCommand(
                                $"kill {runningProcessesToUpdate.Where(kvpair => kvpair.Value.Equals(task)).Select(kvpair => kvpair.Key).First()}");
                            
                            _queueService.RemoveRunningTask(kvp.Value);
                    
                            _queueService.AddToFinishedList(task);
                    
                            _processToTaskDictionary.Remove(_processToTaskDictionary.First(kvpair => kvpair.Key.Equals(kvp.Key)).Key);
                    
                            TicketTask trackingTask = db.Find<TicketTask>(task.Id) ?? throw new InvalidOperationException();
                    
                            trackingTask.Status = TaskStatuses.Failed;
                    
                            string[] outputFiles = _sftpService.ListOfFiles(trackingTask.DirectoryPath,
                                trackingTask.FileNames.Select(filename => filename.Name).ToArray());
                    
                            foreach (string outputFile in outputFiles)
                            {
                                trackingTask.FileNames.Add(new Filename() {Name = outputFile, Inputed = false});
                            }
                        }
                    }
                }
            }
            
            await db.SaveChangesAsync();
        }
    }
}