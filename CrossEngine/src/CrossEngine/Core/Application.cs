using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Services;
using CrossEngine.Profiling;
using System.Threading;
using CrossEngine.Logging;

namespace CrossEngine.Core
{
    public abstract class Application : IDisposable
    {
        public ServiceManager Manager = new ServiceManager();
        private bool running = true;
        private EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static Logger Log = new Logger("app");

        public void Run()
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

            Log.Trace("destroying");

            OnDestroy();

            Log.Trace("destroyed");

#if PROFILING
            Profiler.EndSession();
#endif

            Log.Trace("ended");

            wait.Set();
        }

        public virtual void OnInit()
        {
            Manager.InitServices();

            Manager.AttachServices();
        }

        public virtual void OnDestroy()
        {
            Manager.DetachServices();

            Manager.ShutdownServices();
        }

        public virtual void OnUpdate()
        {
            Manager.Update();
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
