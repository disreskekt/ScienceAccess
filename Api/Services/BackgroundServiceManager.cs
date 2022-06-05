using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Services.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class BackgroundServiceManager
{
    private static readonly int[] _waitingTimings = new[] {5000, 5000, 10000, 30000};
    
    private readonly TaskManager _taskManager;
    private readonly QueueService _queueService;
    
    public BackgroundServiceManager(IServiceProvider services, QueueService queueService)
    {
        _taskManager = (TaskManager) services.GetServices<IHostedService>().First(service => service is TaskManager);
        _queueService = queueService;
    }

    public async Task TaskManagerRun()
    {
        await _taskManager.RunManually(CancellationToken.None);
    }

    public async Task FastTaskCheck(TicketTask task)
    {
        foreach (int waitingTiming in _waitingTimings)
        {
            await Task.Delay(waitingTiming);
            
            await _taskManager.RunManually(CancellationToken.None);

            TicketTask finishedTask = _queueService.GetFromFinishedList(task);

            if (finishedTask is not null)
            {
                break;
            }
        }
    }
}