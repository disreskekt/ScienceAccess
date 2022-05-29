using System;
using System.Collections.Concurrent;
using System.Linq;
using TicketTask = Api.Models.Task;

namespace Api.Services;

public class QueueService
{
    private static readonly ConcurrentQueue<TicketTask> _queue;
    private static readonly ConcurrentDictionary<int, TicketTask> _runningTasks;
    private static readonly ConcurrentQueue<TicketTask> _finishedQueue;

    static QueueService()
    {
        _queue = new ConcurrentQueue<TicketTask>();
        _runningTasks = new ConcurrentDictionary<int, TicketTask>();
        _finishedQueue = new ConcurrentQueue<TicketTask>();
    }

    public void AddToQueue(TicketTask task)
    {
        _queue.Enqueue(task);
        
        //todo call some job to unscheduled run
    }

    public TicketTask GetFromQueue()
    {
        if (_queue.TryDequeue(out TicketTask result))
        {
            return result;
        }

        return null;
    }

    public void AddToFinishedQueue(TicketTask task)
    {
        _finishedQueue.Enqueue(task);
        
        
    }

    public void AddToRunningTasks(int gpu, TicketTask task)
    {
        if (!_runningTasks.TryAdd(gpu, task))
        {
            throw new Exception("Can't add to _runningTasks dictionary");
        }
    }

    public int[] GetRunningGpus()
    {
        return _runningTasks.Keys.ToArray();
    }

    public TicketTask RemoveRunningTask(int gpu)
    {
        if (!_runningTasks.TryRemove(gpu, out TicketTask task))
        {
            throw new Exception("Can't remove value from _runningTasks dictionary");
        }
        
        return task;
    }
}