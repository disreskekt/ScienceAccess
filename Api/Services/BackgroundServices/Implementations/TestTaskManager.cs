using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Models.Enums;
using Api.Models.TestTaskMangerModels;
using Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices.Implementations;

public class TestTaskManager : ITaskManagerImplementation
{
    private readonly IServiceProvider _services;
    private readonly QueueService _queueService;
    private readonly SshService _sshService;
    private readonly SftpService _sftpService;
    private readonly string _programVersionsFolder;

    public TestTaskManager(
        IServiceProvider services,
        QueueService queueService,
        SshService sshService,
        SftpService sftpService,
        IOptions<ProgramVersionsFolder> programVersionsFolder)
    {
        _services = services;
        _queueService = queueService;
        _sshService = sshService;
        _sftpService = sftpService;
        _programVersionsFolder = programVersionsFolder.Value.Path;
    }
    
    public object GetParsedStatus()
    {
        byte[] statusBytes = _sftpService.GetFile("/home/sshuser/lc/Web", "Status.json");
        string statusString = Encoding.UTF8.GetString(statusBytes);

        TestTaskStatus testTaskStatus = new TestTaskStatus()
            {Dictionary = JsonConvert.DeserializeObject<Dictionary<string, string?>>(statusString)};

        return testTaskStatus;
    }

    public async Task RunTasksFromQueue(object status)
    {
        TestTaskStatus testTaskStatus = (TestTaskStatus) status;
        
        while (testTaskStatus.HasFreeGpu)
        {
            TicketTask taskFromQueue = _queueService.GetFromQueue();

            if (taskFromQueue is null)
            {
                break;
            }

            string programPath = _programVersionsFolder + '/' + taskFromQueue.ProgramVersion;

            string freeGpu = testTaskStatus.Dictionary.First(kvp => kvp.Value is null).Key;

            string cdCommand = $"cd {taskFromQueue.DirectoryPath}";
            string mainCommand = $"{programPath} {freeGpu} {taskFromQueue.Id} \\&";
            string disownCommand = "disown -r";

            _sshService.RunCustomCommand($"{cdCommand} && {mainCommand} && {disownCommand}");

            _queueService.AddToRunningTasks(taskFromQueue);

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

    public async Task CheckRunningTasks(object status)
    {
        TestTaskStatus testTaskStatus = (TestTaskStatus) status;
        
        List<TicketTask> runningTasksToUpdate = _queueService.GetRunningTasks();
        
        TicketTask[] tasksToKill = _queueService.GetKillList();
        
        using (IServiceScope serviceScope = _services.CreateScope())
        {
            Context db = (Context) serviceScope.ServiceProvider.GetService(typeof(Context)) ??
                         throw new InvalidOperationException();
            
            foreach (TicketTask taskToUpdate in runningTasksToUpdate)
            {
                if (!testTaskStatus.Dictionary.ContainsValue(taskToUpdate.Id.ToString()))
                {
                    _queueService.RemoveRunningTask(taskToUpdate);
                    
                    _queueService.AddToFinishedList(taskToUpdate);

                    TicketTask trackingTask = await db.Tasks.Where(task => task.Id == taskToUpdate.Id)
                        .Include(task => task.FileNames)
                        .FirstAsync();
                    
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
                        if (runningTasksToUpdate.Contains(task))
                        {
                            _queueService.RemoveRunningTask(taskToUpdate);
                    
                            _queueService.AddToFinishedList(taskToUpdate);
                    
                            TicketTask trackingTask = await db.Tasks.Where(t => t.Id == taskToUpdate.Id)
                                .Include(t => t.FileNames)
                                .FirstAsync();
                    
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