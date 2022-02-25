using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Profiling;

namespace CrossEngine.ComponentSystems
{
    class SpriteRendererSystem : System<SpriteRendererComponent>
    {
        List<ISpriteRenderData> _filtered = new List<ISpriteRenderData>();
        (IRenderable Renderable, IList Objects) Data;

        private readonly ParallelOptions _parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };

        public SpriteRendererSystem(SceneLayerRenderData renderData) : base()
        {
            Data = (new SpriteRenderable(), _filtered);
            renderData.Data.Add(Data);
        }

        public override void Register(SpriteRendererComponent component)
        {
            base.Register(component);

            EnabledChange(component);
            component.OnEnabledChanged += EnabledChange;
        }

        public override void Unregister(SpriteRendererComponent component)
        {
            base.Unregister(component);

            component.OnEnabledChanged -= EnabledChange;
            _filtered.Remove(component);
        }

        private void EnabledChange(Component sender)
        {
            if (sender.Enabled)
                _filtered.Add((SpriteRendererComponent)sender);
            else
                _filtered.Remove((SpriteRendererComponent)sender);
        }

        public override void Update()
        {
            Profiler.BeginScope($"{nameof(SpriteRendererSystem)}.{nameof(SpriteRendererSystem.Update)}");

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update();
            }
            // (Prlllsm:{_parallelOptions.MaxDegreeOfParallelism})
            //Parallel.ForEach(Components, _parallelOptions, (component) => component.Update());

            Profiler.EndScope();
        }
    }
}
