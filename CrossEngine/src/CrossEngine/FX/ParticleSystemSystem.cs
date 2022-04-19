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

namespace CrossEngine.ComponentSystems
{
    class ParticleSystemSystem : System<ParticleSystemComponent>
    {
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

            Parallel.ForEach(Components, _parallelOptions, (component) => component.Update());

            Profiler.EndScope();
        }
    }
}

namespace CrossEngine.FX.Particles
{
    interface IParticleSystemRenderData : IObjectRenderData
    {
        ParticleSystem ParticleSystem { get; }
    }

    class ParticleSystemRenderable : Renderable<IParticleSystemRenderData>
    {
        public override void Begin(Matrix4x4 viewProjectionMatrix)
        {
            Renderer2D.BeginScene(viewProjectionMatrix);
        }

        public override void End()
        {
            Renderer2D.EndScene();
        }

        public override void Submit(IParticleSystemRenderData data)
        {
            data.ParticleSystem.Render();
        }
    }
}
