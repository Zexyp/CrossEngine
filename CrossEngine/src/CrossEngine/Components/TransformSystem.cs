#define DEBUG_TRANSFORMS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Ecs;

namespace CrossEngine.Components
{
    // handles sparse transform hierarchy connections
    public class TransformSystem : CrossEngine.Ecs.System
    {
        protected internal override void OnInit()
        {
            World.Storage.AddNotifyRegister(typeof(TransformComponent), Register);
            World.Storage.AddNotifyUnregister(typeof(TransformComponent), Unregister);
        }

        protected internal override void OnShutdown()
        {
            World.Storage.RemoveNotifyRegister(typeof(TransformComponent), Register);
            World.Storage.RemoveNotifyUnregister(typeof(TransformComponent), Unregister);
        }

        private void Register(Component component)
        {
            var trans = (TransformComponent)component;

            component.Entity.ParentChanged += OnEntityParentChanged;

            AttachTransform(trans);
        }

        public void Unregister(Component component)
        {
            var trans = (TransformComponent)component;
            
            DetachTransform(trans);

            component.Entity.ParentChanged -= OnEntityParentChanged;
        }

        private void OnEntityParentChanged(Entity sender)
        {
            sender.GetComponent<TransformComponent>().Parent = GetTopTransformComponent(sender);
        }

        void OnUpdate()
        {
            Profiler.BeginScope($"{nameof(TransformSystem)}.{nameof(TransformSystem.OnUpdate)}");

            /*
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
            */

            Profiler.EndScope();
        }

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
            // bridge the connections
            var parent = GetTopTransformComponent(component.Entity);
            var children = GetBottomTransformComponents(component.Entity);
            for (int i = 0; i < children.Length; i++)
            {
                children[i].Parent = parent;
            }
            component.Parent = null;
        }
    }
}