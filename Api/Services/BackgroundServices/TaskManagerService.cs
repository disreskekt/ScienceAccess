using System.Threading;
using System.Threading.Tasks;
using Api.Models.NvidiaSmiModels;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices;

public class TaskManagerService : BaseBackgroundService
{
    private readonly ITaskManagerService<NvidiaSmiModel> _taskManagerServiceImplementation; //todo more abstract!

    public TaskManagerService(ITaskManagerService<NvidiaSmiModel> taskManagerServiceImplementation)
    {
        _taskManagerServiceImplementation = taskManagerServiceImplementation;
    }

    protected override int ExecutionInterval => 60000;
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        NvidiaSmiModel status = _taskManagerServiceImplementation.GetParsedStatus();
        
        await _taskManagerServiceImplementation.CheckRunningTasks(status);
        
        await _taskManagerServiceImplementation.RunTasksFromQueue(status);
    }
}