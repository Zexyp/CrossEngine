using CrossEngine.Components;
using CrossEngine.Systems;
using CrossEngine.Ecs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Scenes
{
    public class Scene
    {
        readonly World _world = new World();
        public readonly List<Entity> _entities = new List<Entity>();
        bool _started = false;

        public Scene()
        {
            _world.RegisterSystem(new TransformSystem());
        }

        public void AddEntity(Entity entity)
        {
            entity.Id = Guid.NewGuid();
            _entities.Add(entity);
            if (_started)
                _world.AddEntity(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (_started)
                _world.RemoveEntity(entity);
            _entities.Remove(entity);
            entity.Id = Guid.Empty;
        }

        public Entity CreateEntity()
        {
            var entity = new Entity();
            entity.AddComponent<TransformComponent>();
            AddEntity(entity);
            return entity;
        }

        public void Start()
        {
            _started = true;
            for (int i = 0; i < _entities.Count; i++)
            {
                _world.AddEntity(_entities[i]);
            }
        }

        public void Stop()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                _world.RemoveEntity(_entities[i]);
            }
            _started = false;
        }

        public void Update()
        {
            _world.Update();
        }
    }
}
