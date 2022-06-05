using System.Threading;
using System.Threading.Tasks;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices;

public class TaskManager : BaseBackgroundService
{
    private readonly ITaskManagerImplementation _taskManagerImplementation; //todo maybe generics

    public TaskManager(ITaskManagerImplementation taskManagerImplementation)
    {
        _taskManagerImplementation = taskManagerImplementation;
    }

    protected override int ExecutionInterval => 60000;
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        object status = _taskManagerImplementation.GetParsedStatus();
        
        await _taskManagerImplementation.CheckRunningTasks(status);
        
        await _taskManagerImplementation.RunTasksFromQueue(status);
    }
}