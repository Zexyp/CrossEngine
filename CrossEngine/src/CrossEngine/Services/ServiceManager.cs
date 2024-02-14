using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Events;
using CrossEngine.Services;

namespace CrossEngine.Services
{
    public class ServiceManager
    {
        public event Action<ServiceManager> ServicesDestroyed;

        readonly List<Service> _services = new List<Service>();
        readonly Dictionary<Type, Service> _servicesDict = new Dictionary<Type, Service>();
        readonly List<IUpdatedService> _updatedServices = new List<IUpdatedService>();
        readonly List<IEventedService> _eventedServices = new List<IEventedService>();

        public void Register<T>(T service) where T : Service
        {
            _services.Add(service);
            _servicesDict.Add(service.GetType(), service);
            
            if (service is IUpdatedService us)
                _updatedServices.Add(us);
            if (service is IEventedService es)
                _eventedServices.Add(es);

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
                    
                    if (service is IUpdatedService us)
                        _updatedServices.Remove(us);
                    if (service is IEventedService es)
                        _eventedServices.Remove(es);

                    service.Manager = null;
                }
            }
        }

        public T GetService<T>() where T : Service
        {
            return (T)GetService(typeof(T));
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

        public void AttachServices()
        {
            for (int i = 0; i < _services.Count; i++)
            {
                _services[i].OnAttach();
            }
        }

        public void ShutdownServices()
        {
            for (int i = _services.Count - 1; i >= 0; i--)
            {
                _services[i].OnDestroy();
            }
            ServicesDestroyed?.Invoke(this);
        }

        public void DetachServices()
        {
            for (int i = _services.Count - 1; i >= 0; i--)
            {
                _services[i].OnDetach();
            }
        }

        public void Update()
        {
            for (int i = 0; i < _updatedServices.Count; i++)
            {
                _updatedServices[i].OnUpdate();
            }
        }

        public void Event(Event e)
        {
            for (int i = 0; i < _eventedServices.Count; i++)
            {
                _eventedServices[i].OnEvent(e);
            }
        }
    }
}
