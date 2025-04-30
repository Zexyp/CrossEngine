using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Core.Services;
using CrossEngine.Profiling;
using System.Threading;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Platform;

namespace CrossEngine.Core
{
    public abstract class Application : IDisposable
    {
        public readonly ServiceManager Manager = new ServiceManager();
        protected static Logger Log = new Logger("app");

        private bool running;
        private EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);

        public void Run()
        {
            running = true;
            Thread.CurrentThread.Name = "main";
            ThreadWrapper(InternalRun);
        }

        public virtual void OnInit() { }
        public virtual void OnDestroy() { }
        public virtual void OnStart() { }
        public virtual void OnEnd() { }

        public virtual void OnUpdate()
        {
            Manager.Update();
        }

        public virtual void OnEvent(Event e)
        {
            if (e is WindowCloseEvent wce && !wce.Handled)
            {
                Close();
                wce.Handled = true;
            }
        }

        public void Close()
        {
            running = false;
        }

        public void CloseWait()
        {
            Close();
            wait.WaitOne();
        }

        // what the
        public void Dispose()
        {
            GC.Collect(); // xd
        }

        internal static void ThreadWrapper(Action action)
        {
            Log.Trace($"wrapping thread '{Thread.CurrentThread.Name}'...");

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = culture;
            
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Log.Fatal($"a wild unhandled exception has appeared in thread '{Thread.CurrentThread.Name}':\n{ex}");
                throw;
            }
        }

        private void InternalInit()
        {
            OnInit();

            PlatformHelper.Init();

            Manager.InitServices();

            Manager.AttachServices();

            Manager.Event += OnEvent;
            
            OnStart();
        }

        private void InternalDestroy()
        {
            OnEnd();

            Manager.Event -= OnEvent;

            Manager.DetachServices();

            Manager.ShutdownServices();

            PlatformHelper.Terminate();
            
            OnDestroy();
        }

        private void InternalRun()
        {
            wait.Reset();

            Log.Trace("running");

#if PROFILING
                Profiler.BeginSession("session", "profiling.json");
#endif
            Log.Trace("intializing");

            InternalInit();

            Log.Trace("intialized");

            while (running)
            {
                Profiler.BeginScope("Update");

                OnUpdate();

                Profiler.EndScope();
            }

            Log.Trace("destroying (may hang due to scheduled task(s))");

            InternalDestroy();

            Log.Trace("destroyed");

#if PROFILING
                Profiler.EndSession();
#endif

            Log.Trace("ended");

            wait.Set();
        }
    }
}
