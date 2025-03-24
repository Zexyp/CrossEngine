using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Services;
using CrossEngine.Profiling;
using System.Threading;
using CrossEngine.Events;
using CrossEngine.Logging;

namespace CrossEngine.Core
{
    public abstract class Application : IDisposable
    {
        public readonly ServiceManager Manager = new ServiceManager();
        protected static Logger Log = new Logger("app");

        private bool running = true;
        private EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);

        public void Run()
        {
            Thread.CurrentThread.Name = "main";
            ThreadWrapper(InternalRun);
        }

        public virtual void OnInit()
        {
            Manager.Event += OnEvent;
            
            Manager.InitServices();

            Manager.AttachServices();
        }

        public virtual void OnDestroy()
        {
            Manager.DetachServices();

            Manager.ShutdownServices();

            Manager.Event -= OnEvent;
        }

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
            GC.SuppressFinalize(this);
        }

        internal static void ThreadWrapper(Action action)
        {
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

        private void InternalRun()
        {
            wait.Reset();

            Log.Trace("running");

#if PROFILING
                Profiler.BeginSession("session", "profiling.json");
#endif
            Log.Trace("intializing");

            OnInit();

            Log.Trace("intialized");

            while (running)
            {
                Profiler.BeginScope("Update");

                OnUpdate();

                Profiler.EndScope();
            }

            Log.Trace("destroying (may hang due to scheduled task(s))");

            OnDestroy();

            Log.Trace("destroyed");

#if PROFILING
                Profiler.EndSession();
#endif

            Log.Trace("ended");

            wait.Set();
        }
    }
}
