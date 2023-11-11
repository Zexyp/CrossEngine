#define DEBUG_TRANSFORMS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Components;
using CrossEngine.Profiling;
using CrossEngine.Ecs;

namespace CrossEngine.ComponentSystems
{
    class TransformSystem : System<TransformComponent>, IUpdatedSystem
    {
        private readonly List<TransformComponent> _roots = new List<TransformComponent>();
#if DEBUG_TRANSFORMS
        private readonly List<TransformComponent> _components = new List<TransformComponent>();
#endif

        private void ParentCheck(TransformComponent component)
        {
            if (component.Parent == null) _roots.Add(component);
            else _roots.Remove(component);
        }

        public override void Register(TransformComponent component)
        {
            ParentCheck(component);
            component.ParentChanged += ParentCheck;
#if DEBUG_TRANSFORMS
            _components.Add(component);
#endif
        }

        public override void Unregister(TransformComponent component)
        {
#if DEBUG_TRANSFORMS
            _components.Remove(component);
#endif
            component.ParentChanged -= ParentCheck;
            _roots.Remove(component);
        }

        public void Update()
        {
            Profiler.BeginScope($"{nameof(TransformSystem)}.{nameof(TransformSystem.Update)}");

            void RecursiveUpdate(TransformComponent c)
            {
                c.Update();
                for (int i = 0; i < c.Children.Count; i++)
                {
                    RecursiveUpdate(c.Children[i]);
                }
            }

            for (int i = 0; i < _roots.Count; i++)
            {
                RecursiveUpdate(_roots[i]);
            }

            // (Prlllsm:{_parallelOptions.MaxDegreeOfParallelism})
            //Parallel.ForEach(_roots, _parallelOptions, (component) =>
            //{
            //    RecursiveUpdate(component);
            //});

            Profiler.EndScope();
        }
#if DEBUG_TRANSFORMS
        public void Render()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Rendering.LineRenderer.DrawAxies(_components[i].WorldTransformMatrix);
            }
        }
#endif
    }
}