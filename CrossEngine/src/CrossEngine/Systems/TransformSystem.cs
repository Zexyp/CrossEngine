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
using System.ComponentModel;

namespace CrossEngine.Systems
{
    public class TransformSystem : UnicastSystem<TransformComponent>, IUpdatedSystem
    {
        private readonly List<TransformComponent> _roots = new List<TransformComponent>();
#if DEBUG_TRANSFORMS
        private readonly List<TransformComponent> _components = new List<TransformComponent>();
#endif

        public override void Register(TransformComponent component)
        {
            ParentCheck(component);
            component.ParentChanged += ParentCheck;

#if DEBUG_TRANSFORMS
            _components.Add(component);
#endif

            component.Entity.ParentChanged += OnEntityParentChanged;

            AttachTransform(component);
        }

        private void OnEntityParentChanged(Entity sender)
        {
            sender.Transform.Parent = GetTopTransformComponent(sender);
        }

        public override void Unregister(TransformComponent component)
        {
            DetachTransform(component);

            component.Entity.ParentChanged -= OnEntityParentChanged;

#if DEBUG_TRANSFORMS
            _components.Remove(component);
#endif

            component.ParentChanged -= ParentCheck;
            _roots.Remove(component);
        }

        public void OnUpdate()
        {
            Profiler.BeginScope($"{nameof(TransformSystem)}.{nameof(TransformSystem.OnUpdate)}");

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
        public void DebugRender()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Rendering.LineRenderer.DrawAxies(_components[i].WorldTransformMatrix);
            }
        }
#endif

        // i do not care about memory
        private TransformComponent[] GetBottomTransformComponents(Entity root)
        {
            List<TransformComponent> trans = new List<TransformComponent>();
            Queue<Entity> q = new Queue<Entity>();
            for (int i = 0; i < root.Children.Count; i++)
                q.Enqueue(root.Children[i]);
            while (q.TryDequeue(out var ent))
            {
                if (ent.TryGetComponent<TransformComponent>(out var c))
                {
                    trans.Add(c);
                    continue;
                }
                for (int i = 0; i < ent.Children.Count; i++)
                    q.Enqueue(ent.Children[i]);
            }
            return trans.ToArray();
        }

        private TransformComponent GetTopTransformComponent(Entity ent)
        {
            if (ent.Parent == null)
                return null;
            if (ent.Parent.TryGetComponent<TransformComponent>(out var comp))
                return comp;
            return GetTopTransformComponent(ent.Parent);
        }

        private void ParentCheck(TransformComponent component)
        {
            if (component.Parent == null) _roots.Add(component);
            else _roots.Remove(component);
        }

        private void AttachTransform(TransformComponent component)
        {
            // basically insert yourself
            component.Parent = GetTopTransformComponent(component.Entity);
            // set all children's parent to this
            var children = GetBottomTransformComponents(component.Entity);
            for (int i = 0; i < children.Length; i++)
            {
                children[i].Parent = component;
            }
        }

        private void DetachTransform(TransformComponent component)
        {
            // bridge the connection
            while (component.Children.Count > 0)
                component.Children[0].Parent = component.Parent;

            component.Parent = null;
        }
    }
}