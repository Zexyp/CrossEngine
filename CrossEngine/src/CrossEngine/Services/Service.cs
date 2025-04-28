using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Events;
using CrossEngine.Services;

namespace CrossEngine.Services
{
    public abstract class Service
    {
        protected internal ServiceManager Manager { get; internal set; }

        public abstract void OnStart();
        public abstract void OnDestroy();

        public abstract void OnAttach();
        public abstract void OnDetach();
    }

    public interface IUpdatedService
    {
        void OnUpdate();
    }

    public interface IScheduledService
    {
        Task Execute(Action action);
        TaskScheduler GetScheduler();
    }
}
