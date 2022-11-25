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

        public static void ExecuteOnMianThread(Action action)
        {
            if (IsMainThread)
            {
                action.Invoke();
                return;
            }

            lock (MainThreadActionQueue)
                MainThreadActionQueue.Enqueue(action);
        }
        public static void ExecuteOnRenderThread(Action action)
        {
            if (IsRenderThread)
            {
                action.Invoke();
                return;
            }

            lock (RenderThreadActionQueue)
                RenderThreadActionQueue.Enqueue(action);
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
            lock (MainThreadActionQueue)
                while (MainThreadActionQueue.Count > 0)
                    MainThreadActionQueue.Dequeue().Invoke();
        }

        internal static void ProcessRenderThread()
        {
            lock (RenderThreadActionQueue)
                while (RenderThreadActionQueue.Count > 0)
                    RenderThreadActionQueue.Dequeue().Invoke();
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
