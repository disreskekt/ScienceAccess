using System.Threading.Tasks;

namespace Api.Services.BackgroundServices;

public interface ITaskManagerService<TModel>
where TModel : class
{
    TModel GetParsedStatus();
    Task RunTasksFromQueue(TModel status);
    Task CheckRunningTasks(TModel status);
}