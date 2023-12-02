using CrossEngine.Components;
using CrossEngine.Systems;
using CrossEngine.Ecs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering;
using CrossEngine.Events;

namespace CrossEngine.Scenes
{
    public class Scene
    {
        internal readonly EcsWorld World = new EcsWorld();
        readonly List<Entity> _entities = new List<Entity>();
        bool _started = false;
        public readonly SceneRenderData RenderData = new SceneRenderData();
        public readonly SceneLayerRenderData _sceneLayer = new SceneLayerRenderData();

        public Scene()
        {

            _sceneLayer = new SceneLayerRenderData();
            RenderData.Layers.Add(_sceneLayer);
            
            var rs = new RenderSystem();
            rs.PrimaryCameraChanged += (rsys) => { _sceneLayer.Camera = rsys.PrimaryCamera; };
            
            World.RegisterSystem(new TransformSystem());
            World.RegisterSystem(rs);
            World.RegisterSystem(new SpriteRendererSystem(_sceneLayer));
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
            World.Start();
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
