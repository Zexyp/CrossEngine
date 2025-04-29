using CrossEngine.Ecs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering;
using CrossEngine.Events;
using CrossEngine.Utils;
using CrossEngine.Serialization;
using System.Collections.ObjectModel;
using System.Drawing;
using CrossEngine.Services;
using System.Diagnostics;
using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngine.Components;

namespace CrossEngine.Scenes
{
    public class Scene : ICloneable
    {
        public readonly World World = new World();
        public readonly ReadOnlyCollection<Entity> Entities;
        public bool IsStarted { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;

        readonly List<Entity> _roots = new();
        readonly List<Entity> _entities = new List<Entity>();
        readonly Dictionary<int, Entity> _ids = new Dictionary<int, Entity>();

        public Scene()
        {
            Entities = _entities.AsReadOnly();

            World.RegisterSystem(new TransformSystem());
            World.RegisterSystem(new RenderSystem());
            World.RegisterSystem(new CrossEngine.FX.Particles.ParticleSystemSystem());
        }

        public Entity GetEntityById(int id)
        {
            if (_ids.TryGetValue(id, out Entity entity))
                return entity;
            return null;
        }

        public void AddEntity(Entity entity)
        {
            Profiler.BeginScope();

            Debug.Assert(entity.Parent == null);

            if (entity.Id == 0)
                entity.Id = entity.GetHashCode();

            _entities.Add(entity);
            _ids[entity.Id] = entity;

            if (entity.Parent == null)
                _roots.Add(entity);

            if (IsInitialized)
                World.AddEntity(entity);

            Debug.Assert(entity.Children.Count == 0);

            entity.ParentChanged += OnEntityParentChanged;

            Log.Default.Trace($"added entity '{entity.Id}'");

            Profiler.EndScope();
        }

        public void RemoveEntity(Entity entity)
        {
            Profiler.BeginScope();

            entity.ParentChanged -= OnEntityParentChanged;

            // collapse gap
            for (int i = 0; i < entity.Children.Count; i++)
            {
                entity.Children[i].Parent = entity.Parent;
            }

            // remove parenting
            entity.Parent = null;

            if (IsInitialized)
                World.RemoveEntity(entity);

            _entities.Remove(entity);
            _ids.Remove(entity.Id);

            _roots.Remove(entity);

            Log.Default.Trace($"removed entity '{entity.Id}'");

            entity.Id = 0;

            Profiler.EndScope();
        }

        public Entity GetEntity(int id)
        {
            return _ids.TryGetValue(id, out Entity entity) ? entity : null;
        }
        
        private void OnEntityParentChanged(Entity obj)
        {
            if (obj.Parent == null) _roots.Add(obj);
            else _roots.Remove(obj);
        }

        public void MoveEntity(Entity entity, int destinationIndex)
        {
            if (!_entities.Contains(entity)) throw new InvalidOperationException("Scene does not contain entity!");
            if (destinationIndex < 0 || destinationIndex > _entities.Count - 1) throw new IndexOutOfRangeException("Invalid index!");

            _entities.Remove(entity);
            _entities.Insert(destinationIndex, entity);
        }

        public Entity CreateEntity()
        {
            var entity = new Entity();
            entity.AddComponent<TransformComponent>();
            AddEntity(entity);
            return entity;
        }

        public Entity CreateEmptyEntity()
        {
            var entity = new Entity();
            AddEntity(entity);
            return entity;
        }

        public void Init()
        {
            Debug.Assert(!IsInitialized);
            
            IsInitialized = true;
            World.Init();
            for (int i = 0; i < _entities.Count; i++)
            {
                World.AddEntity(_entities[i]);
            }
        }

        public void Deinit()
        {
            Debug.Assert(IsInitialized);

            for (int i = 0; i < _entities.Count; i++)
            {
                World.RemoveEntity(_entities[i]);
            }
            World.Deinit();
            IsInitialized = false;
        }

        public void Start()
        {
            World.Start();
            IsStarted = true;
        }

        public void Stop()
        {
            IsStarted = false;
            World.Stop();
        }

        public void Update()
        {
            World.Update();
        }

        public void FixedUpdate()
        {
            World.FixedUpdate();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
