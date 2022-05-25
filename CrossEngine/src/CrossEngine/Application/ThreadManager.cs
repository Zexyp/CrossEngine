using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace CrossEngine
{
    public class ThreadManager
    {
        internal static ConcurrentQueue<Action> RenderThreadActionQueue { get; private set; } = new ConcurrentQueue<Action>();
        internal static ConcurrentQueue<Action> MainThreadActionQueue { get; private set; } = new ConcurrentQueue<Action>();

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

        internal static void Setup(Thread main, Thread render)
        {
            _renderThread = render;
            _mainThread = main;
        }

        internal static void ConfigureCurrentThread()
        {
            var ci = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = ci;
        }
    }
}
