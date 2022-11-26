using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Logging;
using CrossEngine.Events;

namespace CrossEngine.ECS
{
    public class World
    {
        List<IComponentSystem> _systems = new List<IComponentSystem>();

        public void Init()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Init();
            }
        }

        public void Shutdown()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Shutdown();
            }
        }

        public void Update()
        {
            var tasks = _systems
                .Where(s => s.ThreadMode == SystemThreadMode.Async)
                .Select(s => Task.Run(s.Update))
                .ToArray();

            for (int i = 0; i < _systems.Count; i++)
            {
                if (_systems[i].ThreadMode == SystemThreadMode.Sync)
                    _systems[i].Update();
            }

            Task.WaitAll(tasks);
        }

        public void RegisterSystem(IComponentSystem system)
        {
            _systems.Add(system);
        }

        public void UnregisterSystem(IComponentSystem system)
        {
            _systems.Remove(system);
        }

        public T GetSystem<T>() where T : IComponentSystem
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                if (_systems[i] is T)
                    return (T)_systems[i];
            }
            return default;
        }
    }
}
