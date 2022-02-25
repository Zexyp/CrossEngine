using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Logging;
using CrossEngine.Events;

namespace CrossEngine.ECS
{
    class World
    {
        List<ISystem> _systems = new List<ISystem>();

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
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].Update();
            }
        }

        public void RegisterSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void UnregisterSystem(ISystem system)
        {
            _systems.Remove(system);
        }
    }
}
