using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Profiling;
using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.FX.Particles;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;

namespace CrossEngine.FX.Particles
{
    class ParticleSystemSystem : System<ParticleSystemComponent>
    {
        // TODO: implement filtered list of enabled

        public override SystemThreadMode ThreadMode => SystemThreadMode.Async;

        (IRenderable Renderable, IList Objects) Data;

        private readonly ParallelOptions _parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };

        public ParticleSystemSystem(SceneLayerRenderData renderData) : base()
        {
            Data = (new ParticleSystemRenderable(), Components);
            renderData.Data.Add(Data);
        }

        public override void Update()
        {
            Profiler.BeginScope($"{nameof(ParticleSystemSystem)}.{nameof(ParticleSystemSystem.Update)}(Prlllsm:{_parallelOptions.MaxDegreeOfParallelism})");

            Parallel.ForEach(Components, _parallelOptions, (component) => { if (component.Enabled) component.Update(); });

            Profiler.EndScope();
        }
    }
}

namespace CrossEngine.FX.Particles
{
    interface IParticleSystemRenderData : IObjectRenderData
    {
        void Render(Matrix4x4 viewMatrix);
        BlendFunc BlendMode { get; }
        AABox Bounds { get; }
    }

    class ParticleSystemRenderable : Renderable<IParticleSystemRenderData>
    {
        Camera camera;
        public override void Begin(Camera camera)
        {
            this.camera = camera;
        }

        public override void Submit(IParticleSystemRenderData data)
        {
            if (!((ParticleSystemComponent)data).Enabled) return;

            if (camera.Frustum.IsAABoxIn(data.Bounds) == Halfspace.Outside) return;

            Application.Instance.RendererAPI.SetBlendFunc(data.BlendMode);
            data.Render(camera.ViewMatrix);
            Renderer2D.Flush();
        }
    }
}
