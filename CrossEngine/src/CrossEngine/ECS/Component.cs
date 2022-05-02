using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Serialization;

namespace CrossEngine.ECS
{
    public abstract class Component : ICloneable, ISerializable
    {
        public Entity Entity { get; internal set; }
        private bool _enabled = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled) return;

                _enabled = value;

                if (_enabled) Enable();
                else Disable();

                OnEnabledChanged?.Invoke(this);
            }
        }
        public event Action<Component> OnEnabledChanged;

        protected internal virtual void Enable() { }
        protected internal virtual void Disable() { }

        protected internal virtual void Attach() { }
        protected internal virtual void Detach() { }

        protected internal virtual void Update() { }

        protected internal virtual void Serialize(SerializationInfo info) { }
        protected internal virtual void Deserialize(SerializationInfo info) { }

        protected virtual Component CreateClone()
        {
            Logging.Log.Core.Info("using default constructor for cloning component");
            return (Component)Activator.CreateInstance(this.GetType());
        }

        public object Clone()
        {
            var comp = CreateClone();
            comp.Enabled = this.Enabled;
            return comp;
        }

        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(Enabled), Enabled);
            Serialize(info);
        }

        public void SetObjectData(SerializationInfo info)
        {
            Enabled = info.GetValue<bool>(nameof(Enabled));
            Deserialize(info);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class AllowSinglePerEntityAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponentAttribute : Attribute
    {
        public readonly Type RequiredComponentType;
        public RequireComponentAttribute(Type componentType)
        {
            if (!componentType.IsSubclassOf(typeof(Component))) throw new InvalidOperationException($"Given type '{componentType.Name}' is not a subclass of '{nameof(Component)}'.");
            
            RequiredComponentType = componentType;

            throw new NotImplementedException("sry");
        }
    }
}
