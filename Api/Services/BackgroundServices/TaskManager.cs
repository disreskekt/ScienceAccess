using System.Threading;
using System.Threading.Tasks;
using Api.Models.NvidiaSmiModels;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices;

public class TaskManager : BaseBackgroundService
{
    private readonly ITaskManagerImplementation<NvidiaSmiModel> _taskManagerImplementation; //todo more abstract!

    public TaskManager(ITaskManagerImplementation<NvidiaSmiModel> taskManagerImplementation)
    {
        _taskManagerImplementation = taskManagerImplementation;
    }

    protected override int ExecutionInterval => 60000;
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        NvidiaSmiModel status = _taskManagerImplementation.GetParsedStatus();
        
        await _taskManagerImplementation.CheckRunningTasks(status);
        
        await _taskManagerImplementation.RunTasksFromQueue(status);
    }
}