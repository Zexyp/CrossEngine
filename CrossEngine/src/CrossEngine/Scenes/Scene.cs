using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq;

using CrossEngine.Entities;
using CrossEngine.Entities.Components;
using CrossEngine.Events;
using CrossEngine.Utils;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Physics;
using CrossEngine.Logging;

namespace CrossEngine.Scenes
{
    public class Scene
    {
        public bool Running { get; private set; } = false;

        internal readonly ComponentRegistry Registry = new ComponentRegistry();

        private readonly List<Entity> _entities = new List<Entity>();
        public ReadOnlyCollection<Entity> Entities { get => _entities.AsReadOnly(); }

        Dictionary<int, Entity> _uids = new Dictionary<int, Entity>();

        public readonly TreeNode<Entity> HierarchyRoot = new TreeNode<Entity>();

        public RigidBodyWorld RigidBodyWorld;

        float fixedUpdateQueue = 0.0f;
        int maxFixedUpdateQueue = 3;
        float fixedUpdateRate = 60.0f;

        public Scene()
        {
            RigidBodyWorld = new RigidBodyWorld();
        }

        public Entity CreateEntity()
        {
            Entity entity = CreateEmptyEntity();
            entity.AddComponent(new TransformComponent());
            return entity;
        }

        public Entity CreateEmptyEntity()
        {
            int why;

            if (_uids.Keys.Count > 0)
                why = Enumerable.Range(1, _uids.Keys.Max() + 2).Except(_uids.Keys).First();
            else
                why = 1;

            Entity entity = new Entity(this, why);

            _uids.Add(why, entity);
            _entities.Add(entity);

            entity.HierarchyNode.SetParent(HierarchyRoot);

            //entity.OnComonentAdded += Entity_OnComonentAdded;
            //entity.OnComonentRemoved += Entity_OnComonentRemoved;

            if (Running)
            {
                entity.OnAwake();
                entity.Activate();
            }

            return entity;
        }

        //private void Entity_OnComonentRemoved(Entity sender, Component component)
        //{
        //    Registry.RemoveComponent(component);
        //}
        //
        //private void Entity_OnComonentAdded(Entity sender, Component component)
        //{
        //    Registry.AddComponent(component);
        //}

        public void RemoveEntity(Entity entity)
        {
            if (Running)
            {
                entity.Deactivate();
                entity.OnDie();
            }

            //entity.OnComonentAdded -= Entity_OnComonentAdded;
            //entity.OnComonentRemoved -= Entity_OnComonentRemoved;

            entity.HierarchyNode.SetParent(null);

            _entities.Remove(entity);
            _uids.Remove(entity.UID);
        }

        public void Start()
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i].OnAwake();
                _entities[i].Activate();
            }

            Running = true;
        }

        public void End()
        {
            Running = false;

            for (int i = 0; i < _entities.Count; i++)
            {
                _entities[i].Deactivate();
                _entities[i].OnDie();
            }
        }

        public void OnEvent(Event e)
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                if (_entities[i].Enabled)
                    _entities[i].OnEvent(e);
            }
        }

        public void OnRender(RenderEvent re)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].Enabled)
                    _entities[i].OnRender(re);
            }
        }

        public void OnUpdateRuntime(float timestep)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (_entities[i].Enabled)
                    _entities[i].OnUpdate(timestep);
            }

            // fixed update mechanism
            fixedUpdateQueue += timestep * fixedUpdateRate;
            if (fixedUpdateQueue > maxFixedUpdateQueue)
            {
                fixedUpdateQueue = 0.0f;
                Log.Core.Info("droped scene fixed update queue");
            }
            while (fixedUpdateQueue > 1.0f)
            {
                OnFixedUpdateRuntime();
                fixedUpdateQueue--;
            }
        }

        public void OnFixedUpdateRuntime()
        {
            RigidBodyWorld.Update(1.0f / 60);

            OnEvent(new RigidBodyWorldUpdateEvent());
        }

        public void OnRednerRuntime()
        {
            // TODO: fix no camera state

            var camEnt = GetPrimaryCameraEntity();
            if (camEnt != null)
            {
                Matrix4x4 projectionMatrix = camEnt.GetComponent<CameraComponent>().Camera.ProjectionMatrix;
                TransformComponent trans = camEnt.GetComponent<TransformComponent>();
                Renderer.Render(this, projectionMatrix * Matrix4x4.CreateTranslation(-trans.WorldPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(trans.WorldRotation)));
            }
        }

        public void OnRenderEditor(EditorCamera camera)
        {
            Renderer.Render(this, camera.ViewProjectionMatrix);
            //Matrix4x4.Lerp(Matrix4x4.Identity, camera.GetViewProjection(), (MathF.Sin((float)Time.TotalElapsedSeconds) + 1) / 2));
            //projectionMatrix* Matrix4x4.CreateTranslation(-trans.WorldPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(trans.WorldRotation))
        }

        public Entity GetPrimaryCameraEntity()
        {
            // TODO: fix
            if (Registry.ContainsType<CameraComponent>())
                return Registry.GetComponents<CameraComponent>()[0].Entity;
            return null;
        }
    }
}
