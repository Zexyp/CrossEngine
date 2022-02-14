using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Components;
using CrossEngine.Rendering;

namespace CrossEngine.Scenes
{
    using ECSWorld = World;
    using ECSEntity = Entity;

    public class Scene
    {
        List<ECSEntity> _entities = new List<ECSEntity>();
        ECSWorld _ecsWorld = new ECSWorld();

        public SceneRenderData renderData = new SceneRenderData();

        public void Init()
        {
            var lrd = new SceneLayerRenderData();
            _ecsWorld.RegisterSystem(new TransformSystem());
            var srs = new SpriteRendererSystem();
            _ecsWorld.RegisterSystem(srs);
            lrd.Objects.Add(new CrossEngine.Rendering.Renderables.SpriteRenderable(), srs.Sus);
            renderData.Layers.Add(lrd);
        }

        public void Update()
        {
            _ecsWorld.Update();
        }

        public ECSEntity CreateEmptyEntity()
        {
            ECSEntity entity = new ECSEntity();
            _entities.Add(entity);
            return entity;
        }

        public ECSEntity CreateEntity()
        {
            ECSEntity entity = CreateEmptyEntity();
            entity.AddComponent<TransformComponent>();
            return entity;
        }
    }
}
