using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Services;

namespace CrossEngine.Services
{
    public class ServiceManager
    {
        readonly List<Service> _services = new List<Service>();
        readonly Dictionary<Type, Service> _servicesDict = new Dictionary<Type, Service>();
        readonly List<UpdatedService> _updatableServices = new List<UpdatedService>();

        public void Register<T>(T service) where T : Service
        {
            _services.Add(service);
            _servicesDict.Add(service.GetType(), service);
            if (service is UpdatedService)
                _updatableServices.Add((UpdatedService)(Service)service);

            service.Manager = this;
        }

        public void Unregister(Service service)
        {
            foreach (var pair in _servicesDict)
            {
                if (pair.Value == service)
                {
                    _services.Remove(service);
                    _servicesDict.Remove(pair.Key);
                    if (service.GetType() == typeof(UpdatedService))
                        _updatableServices.Remove((UpdatedService)service);
                    service.Manager = null;
                }
            }
        }

        public T GetService<T>() where T : Service
        {
            Type type = typeof(T);
            if (_servicesDict.ContainsKey(type))
                return (T)_servicesDict[type];
            return null;
        }

        public Service GetService(Type type)
        {
            if (_servicesDict.ContainsKey(type))
                return _servicesDict[type];
            return null;
        }

        public void InitServices()
        {
            for (int i = 0; i < _services.Count; i++)
            {
                _services[i].OnStart();
            }
        }

        public void ShutdownServices()
        {
            for (int i = _services.Count - 1; i >= 0; i--)
            {
                _services[i].OnDestroy();
            }
        }

        public void Update()
        {
            for (int i = 0; i < _updatableServices.Count; i++)
            {
                _updatableServices[i].OnUpdate();
            }
        }
    }
}
