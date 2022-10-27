using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Profiling;

namespace CrossEngine.ComponentSystems
{
    class TransformSystem : System<TransformComponent>
    {
        private readonly List<TransformComponent> _roots = new List<TransformComponent>();
        private readonly ParallelOptions _parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };

        private void ParentCheck(TransformComponent component)
        {
            Debug.Assert(Components.Contains(component));

            if (component.Parent == null) _roots.Add(component);
            else _roots.Remove(component);
        }

        public override void Register(TransformComponent component)
        {
            base.Register(component);

            ParentCheck(component);
            component.OnParentChanged += ParentCheck;
        }

        public override void Unregister(TransformComponent component)
        {
            component.OnParentChanged -= ParentCheck;
            _roots.Remove(component);

            base.Unregister(component);
        }

        public override void Update()
        {
            Profiler.BeginScope($"{nameof(TransformSystem)}.{nameof(TransformSystem.Update)}");

            void RecursiveUpdate(TransformComponent c)
            {
                for (int i = 0; i < c.Children.Count; i++)
                {
                    if (c.Children[i] != null) RecursiveUpdate(c.Children[i]);
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

        public override void Render()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Rendering.LineRenderer.DrawAxies(Components[i].WorldTransformMatrix);
            }
        }
    }
}
