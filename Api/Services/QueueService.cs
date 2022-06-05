using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.NvidiaSmiModels;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class QueueService
{
    private static readonly ConcurrentQueue<TicketTask> _queue;
    private static readonly ConcurrentDictionary<Process, TicketTask> _runningTasks;
    private static readonly List<TicketTask> _finishedList;
    private static readonly List<TicketTask> _killList;
    
    static QueueService()
    {
        _queue = new ConcurrentQueue<TicketTask>();
        _runningTasks = new ConcurrentDictionary<Process, TicketTask>();
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

    public void AddToRunningTasks(Process process, TicketTask task)
    {
        if (!_runningTasks.TryAdd(process, task))
        {
            throw new Exception("Can't add to _runningTasks dictionary");
        }
    }

    public Dictionary<Process, TicketTask> GetRunningTasks()
    {
        return _runningTasks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public TicketTask RemoveRunningTask(Process process)
    {
        if (!_runningTasks.TryRemove(process, out TicketTask task))
        {
            throw new Exception("Can't remove value from _runningTasks dictionary");
        }
        
        return task;
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