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
using CrossEngine.Utils;
using CrossEngine.Serialization;
using System.Collections.ObjectModel;
using CrossEngine.Assets;
using System.Drawing;
using CrossEngine.Services;
using System.Diagnostics;
using CrossEngine.Profiling;

namespace CrossEngine.Scenes
{
    public class Scene : ICloneable
    {
        public readonly EcsWorld World = new EcsWorld();
        public readonly SceneRenderData RenderData = new SceneRenderData();
        public readonly ReadOnlyCollection<Entity> Entities;
        readonly SceneLayerRenderData _defaultLayer = new SceneLayerRenderData();
        bool _loaded = false;
        readonly List<Entity> _roots = new();
        public bool Started { get; private set; } = false;
        readonly List<Entity> _entities = new List<Entity>();

        public Scene()
        {
            Entities = _entities.AsReadOnly();

            _defaultLayer = new SceneLayerRenderData();
            RenderData.Layers.Add(_defaultLayer);

            var rs = new RenderSystem();
            rs.PrimaryCameraChanged += (rsys) => { _defaultLayer.Camera = rsys.PrimaryCamera; };
            RenderData.Resize += (s, w, h) => rs.Resize(w, h);

            World.RegisterSystem(new TransformSystem());
            World.RegisterSystem(rs);
            World.RegisterSystem(new SpriteRendererSystem(_defaultLayer));
            World.RegisterSystem(new UISystem(RenderData));
            World.RegisterSystem(new ScriptSystem());
        }

        #region Entity Methods
        public Entity GetEntity(int hashCode)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                var ent = _entities[i];
                if (ent.Id.GetHashCode() == hashCode)
                    return ent;
            }

            return null;
        }

        public void AddEntity(Entity entity)
        {
            Profiler.BeginScope();

            Debug.Assert(entity.Parent == null);

            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            _entities.Add(entity);

            if (entity.Parent == null) _roots.Add(entity);

            if (_loaded)
                World.AddEntity(entity);

            for (int i = 0; i < entity.Children.Count; i++)
            {
                AddEntity(entity.Children[i]);
            }

            entity.ParentChanged += OnEntityParentChanged;

            SceneService.Log.Trace($"added entity '{entity.Id}'");

            Profiler.EndScope();
        }

        private void OnEntityParentChanged(Entity obj)
        {
            if (obj.Parent == null) _roots.Add(obj);
            else _roots.Remove(obj);
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

            if (_loaded)
                World.RemoveEntity(entity);

            _entities.Remove(entity);

            _roots.Remove(entity);

            SceneService.Log.Trace($"removed entity '{entity.Id}'");

            Profiler.EndScope();
        }

        public void ShifEntity(Entity entity, int destinationIndex)
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
        #endregion

        public void Load()
        {
            _loaded = true;
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
            _loaded = false;
        }

        public void Start()
        {
            World.Start();
            Started = true;
        }

        public void Stop()
        {
            Started = false;
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
            Scene scene = new Scene();
            void AddChildren(Entity clone, Entity original)
            {
                for (int i = 0; i < original.Children.Count; i++)
                {
                    var ch = original.Children[i];
                    var cch = (Entity)ch.Clone();
                    scene.AddEntity(cch);
                    AddChildren(cch, ch);
                    cch.Parent = clone;
                }
            }

            for (int i = 0; i < this._roots.Count; i++)
            {
                var entity = this._roots[i];
                var centity = (Entity)entity.Clone();
                scene.AddEntity(centity);
                AddChildren(centity, entity);
            }
            return scene;
        }
    }
}
