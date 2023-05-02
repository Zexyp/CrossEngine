using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    internal class ServiceManager
    {
        readonly Dictionary<Type, Service> _services = new Dictionary<Type, Service>();
        readonly List<Service> _updatableServices = new List<Service>();

        public void Register<T>(Service service) where T : Service
        {
            Register(typeof(T), service);
        }

        public void Register(Type type, Service service)
        {
            if (!type.IsSubclassOf(typeof(Service)) || !service.GetType().IsSubclassOf(type))
                throw new InvalidOperationException();

            _services.Add(type, service);
            if (service is UpdatableService)
                _updatableServices.Add(service);
        }

        public void Unregister(Service service)
        {
            foreach (var pair in _services)
            {
                if (pair.Value == service)
                {
                    _services.Remove(pair.Key);
                    _updatableServices.Remove(service);
                }
            }
        }
    }
}
