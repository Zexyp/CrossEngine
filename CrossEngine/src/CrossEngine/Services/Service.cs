using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Services;

namespace CrossEngine.Services
{
    public abstract class Service
    {
        public ServiceManager Manager { get; internal set; }

        public abstract void OnStart();
        public abstract void OnDestroy();
    }

    public interface IUpdatedService
    {
        void OnUpdate();
    }

    public interface IMessagableService
    {
        void Execute(Action action);
    }
}
