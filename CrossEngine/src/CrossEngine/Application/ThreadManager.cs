using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine
{
    public class ThreadManager
    {
        internal static ConcurrentQueue<Action> RenderThreadActionQueue { get; private set; } = new ConcurrentQueue<Action>();
        internal static ConcurrentQueue<Action> MainThreadActionQueue { get; private set; } = new ConcurrentQueue<Action>();

        public static void ExecuteOnRenderThread(Action action)
        {
            RenderThreadActionQueue.Enqueue(action);
        }
        public static void ExecuteOnMianThread(Action action)
        {
            throw new NotImplementedException();
        }
    }
}
