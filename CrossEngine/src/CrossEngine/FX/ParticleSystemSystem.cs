#define INTERNAL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Profiling;
using CrossEngine.Ecs;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.FX.Particles;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Utils;

namespace CrossEngine.FX.Particles
{
    class ParticleSystemSystem : Ecs.System, IUpdatedSystem
    {
        //public override SystemThreadMode ThreadMode => SystemThreadMode.Async;

        private readonly ParallelOptions _parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };
        private List<ParticleSystemComponent> _filtered = new List<ParticleSystemComponent>();
        private ParticleSystemRenderable renderable = new ParticleSystemRenderable();
        
        public void OnUpdate()
        {
            Profiler.BeginScope();

            Parallel.ForEach(World.Storage.GetArray(typeof(ParticleSystemComponent)), _parallelOptions, (component) => { ((ParticleSystemComponent)component).Update(); });

            Profiler.EndScope();
        }
        
#if INTERNAL
        internal
#endif
        protected override void OnAttach()
        {
            World.GetSystem<RenderSystem>().CommitRenderable(renderable, typeof(IParticleSystemRenderData));
        }

#if INTERNAL
        internal
#endif
        protected override void OnDetach()
        {
            World.GetSystem<RenderSystem>().WithdrawRenderable(renderable);
        }
    }
    
    interface IParticleSystemRenderData : IObjectRenderData
    {
        void Render(Matrix4x4 viewMatrix);
        BlendMode Blend { get; }
    }

    class ParticleSystemRenderable : Renderable<IParticleSystemRenderData>
    {
        ICamera camera;
        Frustum frustum;
        public override void Begin(ICamera camera)
        {
            this.camera = camera;
            this.frustum = camera.GetFrustum();
            var mat = camera.GetViewProjectionMatrix();
            Renderer2D.BeginScene(mat);
            LineRenderer.BeginScene(mat);
        }

        public override void End()
        {
            LineRenderer.EndScene();
            Renderer2D.EndScene();
        }

        public override void Submit(IParticleSystemRenderData data)
        {
            var volume = data.GetVolume();
            CullChecker.Append(volume);
            
            if (volume.IsInFrustum(frustum) == Halfspace.Outside) return;

            Renderer2D.SetBlending(data.Blend);
            data.Render(camera.GetViewMatrix());
            Renderer2D.Flush();
        }
    }
}
