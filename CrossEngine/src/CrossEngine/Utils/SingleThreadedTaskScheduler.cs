#define DEBUG_STACK_TRACE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CrossEngine.Utils
{
    internal class SingleThreadedTaskScheduler : TaskScheduler
    {
        struct TaskInfo
        {
            public Task Task;
            public StackTrace Trace;
        }
        
        private bool isExecuting;
        private readonly ConcurrentQueue<TaskInfo> taskQueue = new();

        public Task Schedule(Action action)
        {
            Debug.Assert(action != null);
            
            return
                Task.Factory.StartNew
                    (
                        action,
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        this
                    );
        }
        
        public Task<TResult> Schedule<TResult>(Func<TResult> func)
        {
            Debug.Assert(func != null);
            
            return
                Task.Factory.StartNew
                (
                    func,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    this
                );
        }

        public void RunOnCurrentThread()
        {
            isExecuting = true;

            foreach (var info in taskQueue)
            {
#if DEBUG_STACK_TRACE
                if (!info.Task.IsCompleted)
                {
                    var indent = "  ";
                    Debugger.Log(0, "CrossEngine", $"task stack trace =>\n{indent}" + string.Join($"\n{indent}", info.Trace.GetFrames().Select(x =>
                    {
                        var meth = x.GetMethod();
                        if (meth.DeclaringType.FullName.StartsWith(nameof(System)))
                            return "...";
                        return $"{meth.DeclaringType.FullName}.{meth.Name}";
                    })) + "\n");
                }
#endif
                TryExecuteTask(info.Task);
            }
            
            isExecuting = false;
        }

        protected override IEnumerable<Task> GetScheduledTasks() => null;

        protected override void QueueTask(Task task)
        {
            taskQueue.Enqueue(new TaskInfo
            {
                Task = task,
#if DEBUG_STACK_TRACE
                Trace = new StackTrace(),
#endif
            });
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued) return false;

            return isExecuting && TryExecuteTask(task);
        }
    }
}
