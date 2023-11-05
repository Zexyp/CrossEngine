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

            Manager.InitServices();

            OnInit();

            while (running)
            {
                Profiler.BeginScope("Update");

                Manager.Update();

                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("hello");

                Profiler.EndScope();
            }

            OnDestroy();

            Manager.ShutdownServices();

#if PROFILING
            Profiler.EndSession();
#endif
        }

        protected abstract void OnInit();
        protected abstract void OnDestroy();

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
