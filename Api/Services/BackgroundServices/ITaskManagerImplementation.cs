using System.Threading.Tasks;

namespace Api.Services.BackgroundServices;

public interface ITaskManagerImplementation
{
    object GetParsedStatus();
    Task RunTasksFromQueue(object status);
    Task CheckRunningTasks(object status);
}