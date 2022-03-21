using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Events;

namespace CrossEngine.Scenes
{
    using ECSWorld = World;
    using ECSEntity = Entity;

    public class Scene
    {
        List<ECSEntity> _entities = new List<ECSEntity>();
        ECSWorld _ecsWorld = new ECSWorld();
        int lastId;

        SceneRenderData _renderData;
        SceneLayerRenderData _worldLayer;
        public SceneRenderData GetRenderData()
        {
            var camComp = RendererSystem.Instance.Primary;
            if (camComp?.Camera == null) return null;

            var viewMat = camComp.ViewMatrix;
            if (viewMat.HasValue)
            {
                _worldLayer.ProjectionViewMatrix = viewMat.Value * camComp.Camera.ProjectionMatrix;
            }
            else
            {
                _worldLayer.ProjectionViewMatrix = camComp.Camera.ProjectionMatrix;
            }
            return _renderData;
        }

        public void Init()
        {
            _worldLayer = new SceneLayerRenderData();

            _ecsWorld.RegisterSystem(new ScriptableSystem());
            _ecsWorld.RegisterSystem(new SpriteRendererSystem(_worldLayer));
            _ecsWorld.RegisterSystem(new PhysicsSysten(_worldLayer));
            _ecsWorld.RegisterSystem(new TransformSystem());
            _ecsWorld.RegisterSystem(new RendererSystem());

            _renderData = new SceneRenderData();
            _renderData.Layers.Add(_worldLayer);
        }

        public void Start()
        {
            _ecsWorld.Init();
        }

        public void Stop()
        {
            _ecsWorld.Shutdown();
        }

        public void Update()
        {
            _ecsWorld.Update();
        }

        public void OnEvent(Event e)
        {
            
        }

        public ECSEntity CreateEmptyEntity()
        {
            ECSEntity entity = new ECSEntity();

            entity.Id = lastId++;
            
            _entities.Add(entity);
            return entity;
        }

        public ECSEntity CreateEntity()
        {
            ECSEntity entity = CreateEmptyEntity();
            entity.AddComponent<TransformComponent>();
            return entity;
        }

        public void DestroyEntity(ECSEntity entity)
        {
            while (entity.Components.Count > 0) entity.RemoveComponent(entity.Components[0]);
            entity.Id = 0;
            
            _entities.Remove(entity);
        }
    }
}
