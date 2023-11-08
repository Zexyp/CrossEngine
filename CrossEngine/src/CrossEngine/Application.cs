using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Services;
using CrossEngine.Profiling;

namespace CrossEngine
{
    internal abstract class Application : IDisposable
    {
        public ServiceManager Manager = new ServiceManager();
        bool running = true;

        public void Run()
        {
#if PROFILING
            Profiler.BeginSession("session", "profiling.json");
#endif

            OnInit();

            while (running)
            {
                Profiler.BeginScope("Update");

                OnUpdate();

                Profiler.EndScope();
            }

            OnDestroy();

#if PROFILING
            Profiler.EndSession();
#endif
        }

        public virtual void OnInit()
        {
            Manager.InitServices();
        }

        public virtual void OnDestroy()
        {
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
