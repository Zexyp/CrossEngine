using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CrossEngine.Ecs
{
    internal class World
    {
        readonly List<System> _systems = new List<System>();
        readonly List<IUpdatedSystem> _updatedSystems = new List<IUpdatedSystem>();

        public void RegisterSystem(System system)
        {
            Debug.Assert(!_systems.Contains(system));

            _systems.Add(system);
            if (system is IUpdatedSystem updatedSystem)
                _updatedSystems.Add(updatedSystem);
        }

        public void UnregisterSystem(System system)
        {
            Debug.Assert(_systems.Contains(system));

            _systems.Remove(system);
            if (system is IUpdatedSystem updatedSystem)
                _updatedSystems.Remove(updatedSystem);
        }

        public void AddEntity(Entity entity)
        {
            entity.Attach();
        }

        public void RemoveEntity(Entity entity)
        {
            entity.Detach();
        }

        public void Update()
        {
            for (int i = 0; i < _updatedSystems.Count; i++)
            {
                _updatedSystems[i].Update();
            }
        }
    }
}
