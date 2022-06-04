using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Data;
using Api.Helpers;
using Api.Models.Enums;
using Api.Models.NvidiaSmiModels;
using Api.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices;

public class TaskManagerService : BaseBackgroundService
{
    private readonly IServiceProvider _services;
    private readonly QueueService _queueService;
    private readonly SshService _sshService;
    private readonly GlobalParametersService _globalParametersService;
    private readonly string _programVersionsFolder;

    public TaskManagerService(
        IServiceProvider services,
        QueueService queueService,
        SshService sshService,
        GlobalParametersService globalParametersService,
        IOptions<ProgramVersionsFolder> programVersionsFolder)
    {
        _services = services;
        _queueService = queueService;
        _sshService = sshService;
        _globalParametersService = globalParametersService;
        _programVersionsFolder = programVersionsFolder.Value.Path;
    }

    protected override int ExecutionInterval => 60000;
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        NvidiaSmiModel nvidiaSmiResult = GetParsedStatus();
        
        await CheckRunningTasks(nvidiaSmiResult);
        
        await RunTasksFromQueue(nvidiaSmiResult);
    }

    private NvidiaSmiModel GetParsedStatus()
    {
        string status = _sshService.GetStatus();

        return NvidiaSmiParser.ParseNvidiaSmiResult(status);
    }

    private async Task RunTasksFromQueue(NvidiaSmiModel nvidiaSmiResult)
    {
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
                streams = "qwe";
            }

            _sshService.RunTask(taskFromQueue.DirectoryPath, programPath, jobFile, freeGpu, streams);

            Process startedProcess = GetParsedStatus().Processes.FirstOrDefault(proc => proc.Gpu == freeGpu);

            if (startedProcess is null)
            {
                _queueService.AddToFinishedQueue(taskFromQueue);
                
                using (IServiceScope serviceScope = _services.CreateScope())
                {
                    Context db = (Context) serviceScope.ServiceProvider.GetService(typeof(Context)) ??
                                 throw new InvalidOperationException();
                    
                    TicketTask trackingTask = db.Find<TicketTask>(taskFromQueue.Id) ?? throw new InvalidOperationException();
                    
                    trackingTask.Status = TaskStatuses.Done;
                    
                    //todo add output filenames
                    
                    await db.SaveChangesAsync();
                }
            }
            else
            {
                _queueService.AddToRunningTasks(startedProcess, taskFromQueue);

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

    private async Task CheckRunningTasks(NvidiaSmiModel nvidiaSmiResult)
    {
        Process[] runningProcessesToUpdate = _queueService.GetRunningProcesses();

        foreach (Process process in runningProcessesToUpdate)
        {
            if (!nvidiaSmiResult.Processes.Contains(process))
            {
                TicketTask task = _queueService.RemoveRunningTask(process);
                
                _queueService.AddToFinishedQueue(task);

                using (IServiceScope serviceScope = _services.CreateScope())
                {
                    Context db = (Context) serviceScope.ServiceProvider.GetService(typeof(Context)) ??
                                 throw new InvalidOperationException();
                    
                    TicketTask trackingTask = db.Find<TicketTask>(task.Id) ?? throw new InvalidOperationException();
                    
                    trackingTask.Status = TaskStatuses.Done;
                
                    //todo add output filenames
                    
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}