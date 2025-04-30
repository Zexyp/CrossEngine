using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Events;
using CrossEngine.Core.Services;

namespace CrossEngine.Core.Services
{
    public class ServiceManager
    {
        readonly List<Service> _services = new List<Service>();
        readonly Dictionary<Type, Service> _servicesDict = new Dictionary<Type, Service>();
        readonly List<IUpdatedService> _updatedServices = new List<IUpdatedService>();
        public event OnEventFunction Event;

        public void Register(Service service)
        {
            _services.Add(service);
            _servicesDict.Add(service.GetType(), service);
            
            if (service is IUpdatedService us)
                _updatedServices.Add(us);

            service.Manager = this;
        }

        public void Unregister(Service service)
        {
            service.Manager = null;
                    
            if (service is IUpdatedService us)
                _updatedServices.Remove(us);
            
            _servicesDict.Remove(service.GetType(), out var dictRemove);
            Debug.Assert(dictRemove == service);
            _services.Remove(service);
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
                _services[i].OnInit();
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

        public void SendEvent(Event e)
        {
            Debug.Assert(e != null);
            Event?.Invoke(e);
        }
    }
}
