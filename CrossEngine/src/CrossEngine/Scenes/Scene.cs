﻿using CrossEngine.Components;
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

namespace CrossEngine.Scenes
{
    public class Scene
    {
        public readonly EcsWorld World = new EcsWorld();
        public readonly SceneRenderData RenderData = new SceneRenderData();
        public readonly ReadOnlyCollection<Entity> Entities;
        readonly SceneLayerRenderData _sceneLayer = new SceneLayerRenderData();
        bool _loaded = false;
        readonly List<Entity> _entities = new List<Entity>();
        public AssetPool Assets { get; internal set; } = new AssetPool();

        public Scene()
        {
            Entities = _entities.AsReadOnly();

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
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            _entities.Add(entity);

            if (_loaded)
                World.AddEntity(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (_loaded)
                World.RemoveEntity(entity);

            _entities.Remove(entity);
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
            Assets.Load();

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

            Assets.Unload();
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Update()
        {
            World.Update();
        }

        public void FixedUpdate()
        {
            World.FixedUpdate();
        }
    }
}
