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
            Profiler.BeginSession("session", "profiling.json");

            Manager.InitServices();

            OnInit();

            while (running)
            {
                Profiler.BeginScope("Update");

                Manager.Update();
                
                Profiler.EndScope();
            }

            OnDestroy();

            Manager.ShutdownServices();

            Profiler.EndSession();
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
