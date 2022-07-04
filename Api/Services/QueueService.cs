using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class QueueService
{
    private static readonly ConcurrentQueue<TicketTask> _queue;
    private static readonly List<TicketTask> _runningTasks;
    private static readonly object _locker = new object();
    private static readonly List<TicketTask> _finishedList;
    private static readonly List<TicketTask> _killList;
    
    static QueueService()
    {
        _queue = new ConcurrentQueue<TicketTask>();
        _runningTasks = new List<TicketTask>();
        _finishedList = new List<TicketTask>();
        _killList = new List<TicketTask>();
    }
    
    public async Task AddToQueue(TicketTask task)
    {
        _queue.Enqueue(task);
    }

    public TicketTask GetFromQueue()
    {
        if (_queue.TryDequeue(out TicketTask result))
        {
            return result;
        }

        return null;
    }

    public void AddToFinishedList(TicketTask task)
    {
        _finishedList.Add(task);
        
        //todo maybe remove this
    }

    public TicketTask GetFromFinishedList(TicketTask task)
    {
        return _finishedList.Find(el => el.Equals(task));
    }

    public void AddToRunningTasks(TicketTask task)
    {
        lock (_locker)
        {
            _runningTasks.Add(task);
        }
    }

    public List<TicketTask> GetRunningTasks()
    {
        List<TicketTask> newList = new List<TicketTask>(_runningTasks.Count);
        
        lock (_locker)
        {
            foreach (TicketTask task in _runningTasks)
            {
                newList.Add(new TicketTask() {Id = task.Id, Comment = task.Comment, Status = task.Status, DirectoryPath = task.DirectoryPath, FileNames = task.FileNames, ProgramVersion = task.ProgramVersion});
            }
            
            return newList;
        }
    }

    public void RemoveRunningTask(TicketTask task)
    {
        lock (_locker)
        {
            TicketTask taskToRemove = _runningTasks.First(t => t.Equals(task));

            _runningTasks.Remove(taskToRemove);
        }
    }

    public void AddToKillList(TicketTask task)
    {
        _killList.Add(task);
    }

    public TicketTask[] GetKillList()
    {
        return _killList.ToArray();
    }
}