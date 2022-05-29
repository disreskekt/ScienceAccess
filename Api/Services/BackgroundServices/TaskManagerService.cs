using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Helpers;
using Api.Models.NvidiaSmiModels;
using Api.Options;
using Microsoft.Extensions.Options;
using TicketTask = Api.Models.Task;

namespace Api.Services.BackgroundServices;

public class TaskManagerService : BaseBackgroundService
{
    private readonly QueueService _queueService;
    private readonly SshService _sshService;
    private readonly GlobalParametersService _globalParametersService;
    private readonly string _programVersionsFolder;

    public TaskManagerService(
        QueueService queueService,
        SshService sshService,
        GlobalParametersService globalParametersService,
        IOptions<ProgramVersionsFolder> programVersionsFolder)
    {
        _queueService = queueService;
        _sshService = sshService;
        _globalParametersService = globalParametersService;
        _programVersionsFolder = programVersionsFolder.Value.Path;
    }

    protected override int ExecutionInterval => 30000;
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        NvidiaSmiModel nvidiaSmiResult = GetParsedStatus();
        
        CheckRunningTasks(nvidiaSmiResult);
        
        RunTasksFromQueue(nvidiaSmiResult);
    }

    private NvidiaSmiModel GetParsedStatus()
    {
        string status = _sshService.GetStatus();

        return NvidiaSmiParser.ParseNvidiaSmiResult(status);
    }

    private void RunTasksFromQueue(NvidiaSmiModel nvidiaSmiResult)
    {
        while (nvidiaSmiResult.HasFreeGpu)
        {
            TicketTask taskFromQueue = _queueService.GetFromQueue();

            if (taskFromQueue is null)
            {
                break;
            }

            string directory = taskFromQueue.DirectoryPath + '/' + taskFromQueue.Id;

            string programPath = _programVersionsFolder + '/' + taskFromQueue.ProgramVersion;

            string jobFile = taskFromQueue.FileNames.Select(filename => filename.Name)
                .First(name => name.Contains(".job"));

            int freeGpu = nvidiaSmiResult.Gpus
                .First(gpu => !nvidiaSmiResult.Processes.Select(proc => proc.Gpu).Contains(gpu.Id)).Id;

            int streams = _globalParametersService.GetGlobalParameters().VideocardStreams[freeGpu.ToString()];

            _sshService.RunTask(directory, programPath, jobFile, freeGpu, streams);

            _queueService.AddToRunningTasks(freeGpu, taskFromQueue);
        }
    }

    private void CheckRunningTasks(NvidiaSmiModel nvidiaSmiResult)
    {
        int[] runningGpusFromStatus = nvidiaSmiResult.Processes.Select(proc => proc.Gpu).ToArray();

        int[] runningGpusToUpdate = _queueService.GetRunningGpus();

        foreach (int gpu in runningGpusToUpdate)
        {
            if (!runningGpusFromStatus.Contains(gpu))
            {
                _queueService.AddToFinishedQueue(_queueService.RemoveRunningTask(gpu));
            }
        }
    }
}