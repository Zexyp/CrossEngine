using CrossEngine.Components;
using CrossEngine.Systems;
using CrossEngine.Ecs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering;

namespace CrossEngine.Scenes
{
    public class Scene
    {
        internal readonly EcsWorld World = new EcsWorld();
        readonly List<Entity> _entities = new List<Entity>();
        bool _started = false;
        public readonly SceneRenderData RenderData = new SceneRenderData();

        public Scene()
        {
            World.RegisterSystem(new TransformSystem());

            World.RegisterSystem(new RenderSystem());
            var layer = new SceneLayerRenderData();
            RenderData.Layers.Add(layer);
            World.RegisterSystem(new SpriteRendererSystem(layer));
        }

        public void AddEntity(Entity entity)
        {
            entity.Id = Guid.NewGuid();
            _entities.Add(entity);
            if (_started)
                World.AddEntity(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (_started)
                World.RemoveEntity(entity);
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

        public void Load()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                World.AddEntity(_entities[i]);
            }
        }

        public void Unload()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                World.RemoveEntity(_entities[i]);
            }
        }

        public void Start()
        {
            _started = true;
        }

        public void Stop()
        {
            _started = false;
        }

        public void Update()
        {
            World.Update();
        }
    }
}
