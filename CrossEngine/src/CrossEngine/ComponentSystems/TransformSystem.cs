using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using System.Collections;

namespace CrossEngine.ComponentSystems
{
    class TransformRenderable : IRenderable
    {
        void IRenderable.Begin(Rendering.Cameras.ICamera camera)
        {
            LineRenderer.BeginScene(camera.ViewProjectionMatrix);
        }

        void IRenderable.End()
        {
            LineRenderer.EndScene();
        }

        public void Submit(object data)
        {
            LineRenderer.DrawAxies(((TransformComponent)data).WorldTransformMatrix);
        }
    }

    class TransformSystem : SimpleComponentSystem<TransformComponent>, IRenderableComponentSystem
    {
        public (IRenderable Renderable, IList Objects) RenderData { get; private set; }

        private readonly List<TransformComponent> _roots = new List<TransformComponent>();
        private readonly ParallelOptions _parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };

        public TransformSystem()
        {
            RenderData = (new TransformRenderable(), Components);
        }

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
    }
}
