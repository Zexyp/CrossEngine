using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossEngine.Rendering;

public class CommandQueue : TaskScheduler
{
    TaskFactory _factory;

    public CommandQueue()
    {
        _factory = new TaskFactory(this);
    }
    
    protected override IEnumerable<Task> GetScheduledTasks()
    {
        yield break;
    }

    protected override void QueueTask(Task task)
    {
        TryExecuteTask(task);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return TryExecuteTask(task);
    }

    public Task Submit(Action action)
    {
        return _factory.StartNew(action);
    }
}