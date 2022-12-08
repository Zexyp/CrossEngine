using System;
using System.Collections.ObjectModel;
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
using CrossEngine.Utils;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Assets;
using CrossEngine.FX.Particles;

namespace CrossEngine.Scenes
{
    using ECSWorld = World;

    public class Scene
    {
        public readonly ReadOnlyCollection<Entity> Entities;
        public readonly ReadOnlyCollection<Entity> HierarchyRoot;
        public SceneRenderData RenderData { get; private set; }
        public AssetRegistry AssetRegistry;
        
        private readonly ECSWorld _ecsWorld = new ECSWorld();
        private readonly List<Entity> _roots = new List<Entity>();
        private readonly Dictionary<Guid, Entity> _entityIds = new Dictionary<Guid, Entity>();
        private readonly List<Entity> _entities = new List<Entity>();
        private SceneLayerRenderData _defaultRenderLayer;


        public Scene()
        {
            Entities = _entities.AsReadOnly();
            HierarchyRoot = _roots.AsReadOnly();

            _defaultRenderLayer = new SceneLayerRenderData();

            AddSystem(new ScriptableSystem());
            //_ecsWorld.RegisterSystem(new UISystem(_renderData));
            AddSystem(new SpriteRendererSystem());
            AddSystem(new TextRendererSystem());
            AddSystem(new ParticleSystemSystem());
            AddSystem(new PhysicsSystem());
            AddSystem(new TransformSystem());
            AddSystem(new RendererSystem());
            AddSystem(new TagSystem());

            RenderData = new SceneRenderData();
            RenderData.Layers.Add(_defaultRenderLayer);

            AssetRegistry = new AssetRegistry("./");
        }

        public SceneRenderData UpdateRenderData()
        {
            var camComp = _ecsWorld.GetSystem<RendererSystem>().PrimaryCamera;

            _defaultRenderLayer.Camera = camComp?.Camera;

            return RenderData;
        }

        public void Load()
        {
            AssetRegistry.Load();
        }

        public void Unload()
        {
            AssetRegistry.Unload();
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
            //_ecsWorld.Event(e);
        }

        public Entity CreateEmptyEntity()
        {
            Entity entity = new Entity(_ecsWorld);

            entity.Id = Guid.NewGuid();
            AddEntity(entity);
            
            return entity;
        }

        public Entity CreateEntity()
        {
            Entity entity = CreateEmptyEntity();
            entity.AddComponent<TransformComponent>();
            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            while (entity.Components.Count > 0) entity.RemoveComponent(entity.Components[0]);
            entity.Id = Guid.Empty;

            RemoveEntity(entity);
        }

        public Entity? GetEntityById(Guid id)
        {
            if (_entityIds.ContainsKey(id))
                return _entityIds[id];
            return null;
        }

        public Entity? GetEntityById(int idhash)
        {
            foreach (var item in _entityIds)
            {
                if (item.Key.GetHashCode() == idhash)
                    return item.Value;
            }
            return null;
        }

        public int GetEntityIndex(Entity entity) => _entities.IndexOf(entity);

        public void ShiftEntity(Entity child, int destinationIndex)
        {
            if (!_entities.Contains(child)) throw new InvalidOperationException("Scene does not contain entity!");
            if (destinationIndex < 0 || destinationIndex > _entities.Count - 1) throw new IndexOutOfRangeException("Invalid index!");

            _entities.Remove(child);
            _entities.Insert(destinationIndex, child);
        }

        public int GetRootEntityIndex(Entity entity) => _roots.IndexOf(entity);

        public void ShiftRootEntity(Entity child, int destinationIndex)
        {
            if (!_roots.Contains(child)) throw new InvalidOperationException("Scene does not contain root entity!");
            if (destinationIndex < 0 || destinationIndex > _roots.Count - 1) throw new IndexOutOfRangeException("Invalid index!");

            _roots.Remove(child);
            _roots.Insert(destinationIndex, child);
        }

        public void AddSystem(IComponentSystem system)
        {
            if (system is IRenderableComponentSystem)
                _defaultRenderLayer.Data.Add(((IRenderableComponentSystem)system).RenderData);
            _ecsWorld.RegisterSystem(system);
        }

        public void RemoveSystem(IComponentSystem system)
        {
            if (system is IRenderableComponentSystem)
                _defaultRenderLayer.Data.Remove(((IRenderableComponentSystem)system).RenderData);
            _ecsWorld.UnregisterSystem(system);
        }

        public T GetSystem<T>() where T : IComponentSystem => _ecsWorld.GetSystem<T>();

        internal void SetEntityId(Entity entity, Guid id)
        {
            if (!_entities.Contains(entity))
                throw new InvalidOperationException("Entity is not a part of this scene.");

            RemoveEntity(entity);
            entity.Id = id;
            AddEntity(entity);
        }

        private void Entity_OnParentChanged(Entity sender)
        {
            if (sender.Parent == null) _roots.Add(sender);
            else _roots.Remove(sender);
        }

        private void AddEntity(Entity entity)
        {
            _entityIds.Add(entity.Id, entity);

            _entities.Add(entity);

            if (entity.Parent == null)
                _roots.Add(entity);
            entity.OnParentChanged += Entity_OnParentChanged;
        }

        private void RemoveEntity(Entity entity)
        {
            entity.OnParentChanged += Entity_OnParentChanged;
            _roots.Remove(entity);
            _entityIds.Remove(entity.Id);
            _entities.Remove(entity);
        }
    }
}
