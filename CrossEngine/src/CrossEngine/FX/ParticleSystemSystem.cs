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
    class ParticleSystemSystem : SimpleSystem<ParticleSystemComponent>, IRenderableSystem
    {
        public override SystemThreadMode ThreadMode => SystemThreadMode.Async;

        public (IRenderable Renderable, IList Objects) RenderData { get; private set; }

        private readonly ParallelOptions _parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };
        private List<ParticleSystemComponent> _filtered = new List<ParticleSystemComponent>();

        public ParticleSystemSystem() : base()
        {
            RenderData = (new ParticleSystemRenderable(), _filtered);
        }

        public override void Update()
        {
            Profiler.BeginScope($"{nameof(ParticleSystemSystem)}.{nameof(ParticleSystemSystem.Update)}(Prlllsm:{_parallelOptions.MaxDegreeOfParallelism})");

            Parallel.ForEach(Components, _parallelOptions, (component) => { if (component.Enabled) component.Update(); });

            Profiler.EndScope();
        }

        public override void Register(ParticleSystemComponent component)
        {
            base.Register(component);
            if (component.Enabled)
                _filtered.Add(component);
        }

        public override void Unregister(ParticleSystemComponent component)
        {
            component.OnEnabledChanged += Component_OnEnabledChanged;
            base.Unregister(component);
            _filtered.Remove(component);
        }

        private void Component_OnEnabledChanged(Component obj)
        {
            if (obj.Enabled)
                _filtered.Add((ParticleSystemComponent)obj);
            else
                _filtered.Remove((ParticleSystemComponent)obj);
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
