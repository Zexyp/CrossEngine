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

namespace CrossEngine.Scenes
{
    using ECSWorld = World;

    public class Scene
    {
        readonly List<Entity> _entities = new List<Entity>();
        public readonly ReadOnlyCollection<Entity> Entities;
        Dictionary<int, Entity> _entityIds = new Dictionary<int, Entity>();
        ECSWorld _ecsWorld = new ECSWorld();
        int lastId;
        public readonly ReadOnlyCollection<Entity> HierarchyRoot;
        private readonly List<Entity> _roots = new List<Entity>();
        public EditorCamera _overrideEditorCamera = null;

        public SceneRenderData _renderData;
        SceneLayerRenderData _worldLayer;

        public AssetRegistry AssetRegistry;

        public Scene()
        {
            HierarchyRoot = _roots.AsReadOnly();
            Entities = _entities.AsReadOnly();

            _worldLayer = new SceneLayerRenderData();

            _ecsWorld.RegisterSystem(new ScriptableSystem());
            _ecsWorld.RegisterSystem(new SpriteRendererSystem(_worldLayer));
            _ecsWorld.RegisterSystem(new ParticleSystemSystem(_worldLayer));
            _ecsWorld.RegisterSystem(new PhysicsSystem(_worldLayer));
            _ecsWorld.RegisterSystem(new TransformSystem());
            _ecsWorld.RegisterSystem(new RendererSystem());

            _renderData = new SceneRenderData();
            _renderData.Layers.Add(_worldLayer);

            AssetRegistry = new AssetRegistry("./assets");
            AssetManager._ctx = AssetRegistry;
        }

        public SceneRenderData GetRenderData()
        {
            var camComp = _ecsWorld.GetSystem<RendererSystem>().Primary;

            if ((_overrideEditorCamera ?? camComp?.Camera) == null) return null;

            if (_overrideEditorCamera == null)
            {
                Matrix4x4? viewMat = camComp.ViewMatrix;
                _worldLayer.Camera = camComp.Camera;
                if (viewMat.HasValue)
                {
                    _worldLayer.Camera.ViewMatrix = viewMat.Value;
                }
                else
                {
                    _worldLayer.Camera.ViewMatrix = Matrix4x4.Identity;
                }
            }
            else
            {
                _worldLayer.Camera = _overrideEditorCamera;
            }

            return _renderData;
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

        public void Render()
        {
            _ecsWorld.Render();
        }

        public void OnEvent(Event e)
        {

        }

        public Entity CreateEmptyEntity()
        {
            Entity entity = new Entity(_ecsWorld);

            entity.Id = ++lastId;
            _entityIds.Add(entity.Id, entity);

            _entities.Add(entity);

            if (entity.Parent == null) _roots.Add(entity);
            entity.OnParentChanged += Entity_OnParentChanged;
            
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
            entity.OnParentChanged += Entity_OnParentChanged;
            _roots.Remove(entity);

            while (entity.Components.Count > 0) entity.RemoveComponent(entity.Components[0]);
            _entityIds.Remove(entity.Id);
            entity.Id = 0;
            
            _entities.Remove(entity);
        }

        public Entity GetEntityById(int id)
        {
            if (_entityIds.ContainsKey(id))
                return _entityIds[id];
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

        private void Entity_OnParentChanged(Entity sender)
        {
            if (sender.Parent == null) _roots.Add(sender);
            else _roots.Remove(sender);
        }
    }
}
