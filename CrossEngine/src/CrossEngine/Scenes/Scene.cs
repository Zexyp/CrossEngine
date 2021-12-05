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
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Lines;
using CrossEngine.Assets;
using CrossEngine.Rendering.Passes;

namespace CrossEngine.Scenes
{
    public class Scene
    {
        // serialized
        public AssetPool AssetPool { get; internal set; } = new AssetPool();
        
        public readonly ReadOnlyCollection<Entity> Entities;


        public bool Running { get; private set; } = false;

        internal readonly ComponentRegistry Registry = new ComponentRegistry();

        private readonly List<Entity> _entities = new List<Entity>();
        private Dictionary<int, Entity> _uids = new Dictionary<int, Entity>();

        public readonly TreeNode<Entity> HierarchyRoot = new TreeNode<Entity>();

        public readonly RenderPipeline Pipeline;

        public RigidBodyWorld RigidBodyWorld;

        //float fixedUpdateQueue = 0.0f;
        //int maxFixedUpdateQueue = 3;
        //float fixedUpdateRate = 60.0f;

        public Scene()
        {
            Entities = _entities.AsReadOnly();

            Pipeline = new RenderPipeline();
            Pipeline.RegisterPass(new Renderer2DPass());
            Pipeline.RegisterPass(new LineRenderPass());
        }

        #region Entity Manage
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

            entity.HierarchyNode.Parent = HierarchyRoot;

            if (Running)
            {
                entity.Activate();
            }

            Log.Core.Trace($"created entity with uid {entity.UID}");

            return entity;
        }

        public Entity CreateEntity()
        {
            Entity entity = CreateEmptyEntity();
            entity.AddComponent(new TransformComponent());

            return entity;
        }

        public void RemoveEntity(Entity entity)
        {
            var comps = entity.Components;
            while (comps.Count > 0) entity.RemoveComponent(comps[0]);

            if (Running)
            {
                entity.Deactivate();
            }

            for (int i = 0; i < entity.HierarchyNode.Children.Count; i++)
            {
                entity.HierarchyNode.Children[i].Value.Parent = null;
            }
            entity.Parent = null;
            entity.HierarchyNode.Parent = null;

            _entities.Remove(entity);
            _uids.Remove(entity.UID);

            Log.Core.Trace($"removed entity with uid {entity.UID}");
        }

        public Entity GetEntity(int uid)
        {
            if (!_uids.ContainsKey(uid)) return null;
            return _uids[uid];
        }

        public Entity GetPrimaryCameraEntity()
        {
            // TODO: fix
            if (Registry.ContainsType<CameraComponent>())
            {
                foreach (var cameraComp in Registry.GetComponentsCollection<CameraComponent>())
                {
                    if (cameraComp.Primary) return cameraComp.Entity;
                }
            }
            return null;
        }

        public int GetEntityIndex(Entity entity)
        {
            //if (!_entities.Contains(entity)) throw new InvalidOperationException("Scene does not contain given entity!");
            return _entities.IndexOf(entity);
        }

        public void ShiftEntity(Entity entity, int destinationIndex)
        {
            if (!_entities.Contains(entity)) throw new InvalidOperationException("Scene does not contain given entity!");
            if (destinationIndex < 0 || destinationIndex > _entities.Count - 1) throw new InvalidOperationException("Invalid index!");

            _entities.Remove(entity);
            _entities.Insert(destinationIndex, entity);
        }
        #endregion

        #region Start/End
        public void Start()
        {
            RigidBodyWorld = new RigidBodyWorld();

            Physics.Physics.SetContext(RigidBodyWorld.GetWorld());

            for (int i = 0; i < _entities.Count; i++)
            {
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
            }

            RigidBodyWorld.Cleanup();
            RigidBodyWorld = null;
        }
        #endregion

        #region Resource Manage
        public void Load()
        {
            if (Running == false) AssetPool.Load();
            else throw new InvalidOperationException();
        }

        public void Unload()
        {
            if (Running == false) AssetPool.Dispose();
            else throw new InvalidOperationException();
        }
        #endregion

        public void OnEvent(Event e)
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                if (e.Handled) break;
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

            RigidBodyWorld.Update(timestep);
            OnEvent(new RigidBodyWorldUpdateEvent());

            //// fixed update mechanism
            //fixedUpdateQueue += timestep * fixedUpdateRate;
            //if (fixedUpdateQueue > maxFixedUpdateQueue)
            //{
            //    fixedUpdateQueue = 0.0f;
            //    Log.Core.Trace("droped scene fixed update queue");
            //}
            //while (fixedUpdateQueue > 1.0f)
            //{
            //    fixedUpdateQueue--;
            //}
            //OnFixedUpdateRuntime();
        }

        //public void OnFixedUpdateRuntime()
        //{
        //
        //}

        public void OnRenderRuntime(Framebuffer framebuffer)
        {
            // TODO: fix no camera state

            var camEnt = GetPrimaryCameraEntity();
            if (camEnt != null)
            {
                RenderPipeline(camEnt.GetComponent<CameraComponent>().ViewProjectionMatrix, framebuffer);
            }
        }

        public void OnRenderEditor(EditorCamera camera, Framebuffer framebuffer = null)
        {
            RenderPipeline(camera.ViewProjectionMatrix, framebuffer);
        }

        private void RenderPipeline(Matrix4x4 viewProjectionMatrix, Framebuffer framebuffer = null)
        {
            Pipeline.Render(new SceneData(this, viewProjectionMatrix), framebuffer);

            //if (framebuffer != null) framebuffer.Bind();
            //
            //// sprite render pass
            //Renderer2D.BeginScene(viewProjectionMatrix);
            //{
            //    var spriteEnts = Registry.GetComponentsGroup<TransformComponent, SpriteRendererComponent>(Registry.GetComponentsCollection<SpriteRendererComponent>());
            //    foreach (var spEnt in spriteEnts)
            //    {
            //        SpriteRendererComponent src = spEnt.Item2;
            //        // check if enabled
            //        if (!src.Enabled) continue;
            //        TransformComponent trans = spEnt.Item1;
            //
            //        // forced z index
            //        //if (!ForceZIndex)
            //        //else
            //
            //        Matrix4x4 matrix = Matrix4x4.CreateScale(new Vector3(src.Size, 1.0f)) * Matrix4x4.CreateTranslation(new Vector3(src.DrawOffset, 1.0f)) * trans.WorldTransformMatrix;
            //
            //        //                          small check can be implemented later
            //        if (src.TextureAsset == null/* || !src.TextureAsset.IsLoaded*/) Renderer2D.DrawQuad(matrix, src.Color, spEnt.CommonEntity.UID);
            //        else Renderer2D.DrawQuad(matrix, src.TextureAsset.Texture, src.Color, src.TextureOffsets, spEnt.CommonEntity.UID);
            //    }
            //    OnRender(new SpriteRenderEvent());
            //}
            //Renderer2D.EndScene();
            //
            ////Renderer2D.EnableDiscardingTransparency(true);
            ////
            ////Renderer2D.BeginScene(viewProjectionMatrix);
            ////{
            ////    SpriteRenderEvent re = new SpriteRenderEvent(SpriteRendererComponent.TransparencyMode.Discarding);
            ////    OnRender(re);
            ////}
            ////Renderer2D.EndScene();
            ////
            ////Renderer2D.EnableDiscardingTransparency(false);
            ////
            ////Renderer.EnableBlending(true, BlendFunc.OneMinusSrcAlpha);
            ////
            ////Renderer2D.BeginScene(viewProjectionMatrix);
            ////{
            ////    SpriteRenderEvent re = new SpriteRenderEvent(SpriteRendererComponent.TransparencyMode.Blending);
            ////    OnRender(re);
            ////}
            ////Renderer2D.EndScene();
            ////
            ////Renderer.EnableBlending(false);
            //
            //if (framebuffer != null) framebuffer.EnableColorDrawBuffer(1, false);
            //
            //// line render pass
            //Renderer.SetDepthFunc(DepthFunc.LessEqual);
            //
            //LineRenderer.BeginScene(viewProjectionMatrix);
            //{
            //    OnRender(new LineRenderEvent());
            //}
            //LineRenderer.EndScene();
            //
            //Renderer.SetDepthFunc(DepthFunc.Default);
            //
            //if (framebuffer != null) framebuffer.EnableAllColorDrawBuffers(true);
        }
    }
}
