using System.Numerics;
using System.Reflection;
using CrossEngine.Components;
using CrossEngine.ECS;
using CrossEngine.Scenes;
using CrossEngineEditor.UndoRedo;

/*
HierarchyPanel - 
InspectorPanel - done
ViewportPanel - done
PropertyDrawer - 
*/

namespace CrossEngineEditor.Operations
{
    class EntitySelectOpertion : IOperationShard
    {
        public Entity PreviousSelection, NextSelection;
        public EditorContext Context;

        public EntitySelectOpertion(EditorContext context, Entity previousSelection, Entity nextSelection)
        {
            Context = context;
            PreviousSelection = previousSelection;
            NextSelection = nextSelection;
        }

        public void Redo()
        {
            Context.ActiveEntity = NextSelection;
        }

        public void Undo()
        {
            Context.ActiveEntity = PreviousSelection;
        }
    }

    class EntityTransformChangeOperation : IOperationShard
    {
        public TransformComponent Target;
        public Matrix4x4 PreviousTransform, NextTransform;

        public EntityTransformChangeOperation(TransformComponent target, Matrix4x4 previousTransform)
        {
            Target = target;
            PreviousTransform = previousTransform;
        }

        public void Redo()
        {
            Target.SetWorldTransform(NextTransform);
        }

        public void Undo()
        {
            Target.SetWorldTransform(PreviousTransform);
        }
    }

    class EntityShiftChildOperation : IOperationShard
    {
        public Entity Parent;
        public Entity Child;
        public int PreviousIndex, NextIndex;

        public EntityShiftChildOperation(Entity parent, Entity child, int previousIndex, int nextIndex)
        {
            Parent = parent;
            Child = child;
            PreviousIndex = previousIndex;
            NextIndex = nextIndex;
        }

        public void Redo()
        {
            Parent.ShiftChild(Child, NextIndex);
        }

        public void Undo()
        {
            Parent.ShiftChild(Child, PreviousIndex);
        }
    }

    class EntityParentChangeOperation : IOperationShard
    {
        public Entity Target;
        public Entity PreviousParent, NextParent;

        public EntityParentChangeOperation(Entity target, Entity previousParent, Entity nextParent)
        {
            Target = target;
            PreviousParent = previousParent;
            NextParent = nextParent;
        }

        public void Redo()
        {
            Target.Parent = NextParent;
        }

        public void Undo()
        {
            Target.Parent = PreviousParent;
        }
    }

    class EntityRemoveOperation : IOperationShard
    {
        public Entity Target;
        public Entity Parent;
        public Scene Scene;
        public Entity[] Children;
        public int SceneIndex;
        public int ChildIndex;

        public EntityRemoveOperation(Entity target, Scene scene, Entity paren, Entity[] children, int sceneIndex, int indexAsChild)
        {
            Target = target;
            Scene = scene;
            Parent = paren;
            Children = children;
            SceneIndex = sceneIndex;
            ChildIndex = indexAsChild;
        }

        public void Redo()
        {
            Scene.RemoveEntity(Target);
        }

        public void Undo()
        {
            Scene.AddEntity(Target);
            Scene.ShiftEntity(Target, SceneIndex);
            Target.Parent = Parent;

            if (Target.Parent == null)
                Scene.ShiftRootEntity(Target, ChildIndex);
            else
                Target.Parent.ShiftChild(Target, ChildIndex);

            for (int i = 0; i < Children.Length; i++)
            {
                Children[i].Parent = Target;
            }
        }
    }

    class EntityAddOperation : IOperationShard
    {
        public Entity Target;
        public Scene Scene;

        public EntityAddOperation(Entity target, Scene scene)
        {
            Target = target;
            Scene = scene;
        }

        public void Redo()
        {
            Scene.AddEntity(Target);
        }

        public void Undo()
        {
            Scene.RemoveEntity(Target);
        }
    }

    class SceneShiftRootEntityOperation : IOperationShard
    {
        public Scene Target;
        public Entity Child;
        public int PreviousIndex, NextIndex;

        public SceneShiftRootEntityOperation(Scene target, Entity child, int previousIndex, int nextIndex)
        {
            Target = target;
            Child = child;
            PreviousIndex = previousIndex;
            NextIndex = nextIndex;
        }

        public void Redo()
        {
            Target.ShiftRootEntity(Child, NextIndex);
        }

        public void Undo()
        {
            Target.ShiftRootEntity(Child, PreviousIndex);
        }
    }

    class SceneShiftEntityOperation : IOperationShard
    {
        public Scene Target;
        public Entity Entity;
        public int PreviousIndex, NextIndex;

        public SceneShiftEntityOperation(Scene target, Entity entity, int previousIndex, int nextIndex)
        {
            Target = target;
            Entity = entity;
            PreviousIndex = previousIndex;
            NextIndex = nextIndex;
        }

        public void Redo()
        {
            Target.ShiftEntity(Entity, NextIndex);
        }

        public void Undo()
        {
            Target.ShiftEntity(Entity, PreviousIndex);
        }
    }

    class ComponentAddOperation : IOperationShard
    {
        public Entity Target;
        public Component Component;

        public ComponentAddOperation(Entity target, Component component)
        {
            Target = target;
            Component = component;
        }

        public void Redo()
        {
            Target.AddComponent(Component);
        }

        public void Undo()
        {
            Target.RemoveComponent(Component);
        }
    }

    class ComponentRemoveOperation : IOperationShard
    {
        public Entity Target;
        public Component Component;
        public int Index;

        public ComponentRemoveOperation(Entity target, Component component, int index)
        {
            this.Target = target;
            this.Component = component;
            this.Index = index;
        }

        public void Redo()
        {
            Target.RemoveComponent(Component);
        }

        public void Undo()
        {
            Target.AddComponent(Component);
            Target.ShiftComponent(Component, Index);
        }
    }

    class ComponentShiftOperation : IOperationShard
    {
        public Entity Target;
        public Component Component;
        public int PreviousIndex, NextIndex;

        public ComponentShiftOperation(Entity target, Component component, int previousIndex, int nextIndex)
        {
            Target = target;
            Component = component;
            PreviousIndex = previousIndex;
            NextIndex = nextIndex;
        }

        public void Redo()
        {
            Target.ShiftComponent(Component, NextIndex);
        }

        public void Undo()
        {
            Target.ShiftComponent(Component, PreviousIndex);
        }
    }

    class ComponentEnabledChangeOperation : IOperationShard
    {
        public Component Target;
        public bool PreviousValue, NextValue;

        public ComponentEnabledChangeOperation(Component target, bool previousValue, bool nextValue)
        {
            Target = target;
            PreviousValue = previousValue;
            NextValue = nextValue;
        }

        public void Redo()
        {
            Target.Enabled = NextValue;
        }

        public void Undo()
        {
            Target.Enabled = PreviousValue;
        }
    }

    class MemberValueChangeOperation : IOperationShard
    {
        public MemberInfo Member;
        public object Target;

        public object PreviousValue;
        public object NextValue;

        public void Redo()
        {
            CrossEngineEditor.Utils.PropertyDrawerUtilExtensions.SetFieldOrPropertyValue(Member, Target, NextValue);
        }

        public void Undo()
        {
            CrossEngineEditor.Utils.PropertyDrawerUtilExtensions.SetFieldOrPropertyValue(Member, Target, PreviousValue);
        }

        public override string ToString()
        {
            return $"Value of {Member.Name} on {Target} ({PreviousValue} -> {NextValue})";
        }
    }
}
