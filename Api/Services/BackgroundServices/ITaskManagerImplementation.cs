using System.Threading.Tasks;

namespace Api.Services.BackgroundServices;

public interface ITaskManagerImplementation<TModel>
where TModel : class
{
    TModel GetParsedStatus();
    Task RunTasksFromQueue(TModel status);
    Task CheckRunningTasks(TModel status);
}