using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace CrossEngine
{
    public static class ThreadManager
    {
        private static readonly Queue<Action> RenderThreadActionQueue = new Queue<Action>();
        private static readonly Queue<Action> MainThreadActionQueue = new Queue<Action>();

        public static bool IsRenderThread => Thread.CurrentThread == _renderThread;
        public static bool IsMainThread => Thread.CurrentThread == _mainThread;

        private static Thread _renderThread;
        private static Thread _mainThread;

        private static readonly Mutex _mainMutex = new Mutex();
        private static readonly Mutex _renderMutex = new Mutex();

        public static void ExecuteOnMianThread(Action action)
        {
            _mainMutex.WaitOne();
            MainThreadActionQueue.Enqueue(action);
            _mainMutex.ReleaseMutex();
        }
        public static void ExecuteOnRenderThread(Action action)
        {
            _renderMutex.WaitOne();
            RenderThreadActionQueue.Enqueue(action);
            _renderMutex.ReleaseMutex();
        }

        internal static void SetMainThread(Thread main)
        {
            _mainThread = main;
        }
        internal static void SetRenderThread(Thread render)
        {
            _renderThread = render;
        }

        internal static void ProcessMainThread()
        {
            _mainMutex.WaitOne();
            while (MainThreadActionQueue.Count > 0)
            {
                MainThreadActionQueue.Dequeue().Invoke();
            }
            _mainMutex.ReleaseMutex();
        }

        internal static void ProcessRenderThread()
        {
            _renderMutex.WaitOne();
            while (RenderThreadActionQueue.Count > 0)
            {
                RenderThreadActionQueue.Dequeue().Invoke();
            }
            _renderMutex.ReleaseMutex();
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
