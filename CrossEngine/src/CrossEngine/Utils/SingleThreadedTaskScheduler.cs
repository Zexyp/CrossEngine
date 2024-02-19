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
        private bool isExecuting;
        private readonly ConcurrentQueue<Task> taskQueue = new ConcurrentQueue<Task>();

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

        public void RunOnCurrentThread()
        {
            isExecuting = true;

            foreach (var task in taskQueue)
            {
                TryExecuteTask(task);
            }
            
            isExecuting = false;
        }

        protected override IEnumerable<Task> GetScheduledTasks() => null;

        protected override void QueueTask(Task task)
        {
            taskQueue.Enqueue(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued) return false;

            return isExecuting && TryExecuteTask(task);
        }
    }
}
