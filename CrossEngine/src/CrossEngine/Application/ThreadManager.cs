using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace CrossEngine
{
    public static class ThreadManager
    {
        internal static readonly ConcurrentQueue<Action> RenderThreadActionQueue = new ConcurrentQueue<Action>();
        internal static readonly ConcurrentQueue<Action> MainThreadActionQueue = new ConcurrentQueue<Action>();

        public static bool IsRenderThread => Thread.CurrentThread == _renderThread;
        public static bool IsMainThread => Thread.CurrentThread == _mainThread;

        private static Thread _renderThread;
        private static Thread _mainThread;

        public static void ExecuteOnRenderThread(Action action)
        {
            RenderThreadActionQueue.Enqueue(action);
        }
        public static void ExecuteOnMianThread(Action action)
        {
            MainThreadActionQueue.Enqueue(action);
        }

        internal static void SetMainThread(Thread main)
        {
            _mainThread = main;
        }

        internal static void SetRenderThread(Thread render)
        {
            _renderThread = render;
        }

        // it's not possible to clone a culture info of a given thread
        internal static void ConfigureCurrentThread()
        {
            var ci = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = ci;
        }
    }
}
