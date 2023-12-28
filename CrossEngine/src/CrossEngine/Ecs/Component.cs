using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CrossEngine.Serialization;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Logging;

namespace CrossEngine.Ecs
{
    public abstract class Component : ICloneable, ISerializable
    {
        public Entity Entity { get; internal set; }
        public event Action<Component> EnabledChanged;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled) return;

                _enabled = value;

                if (!Attached) return;

                if (_enabled) OnEnable();
                else OnDisable();

                EnabledChanged?.Invoke(this);
            }
        }

        private bool _enabled = true;
        internal bool Attached = false; // idfk rn, ecs world refernce would be as good as this weird bool

        public Component()
        {
            
        }

        public object Clone()
        {
            var comp = CreateClone();
            comp.Enabled = this.Enabled;
            return comp;
        }

        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue("Enabled", Enabled);
            OnSerialize(info);
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            Enabled = info.GetValue<bool>("Enabled");
            OnDeserialize(info);
        }

        protected virtual Component CreateClone()
        {
            Log.Default.Debug("using default constructor for cloning component");
            return (Component)Activator.CreateInstance(this.GetType());
        }

        protected internal virtual void OnEnable() { }
        protected internal virtual void OnDisable() { }

        protected internal virtual void OnAttach() { }
        protected internal virtual void OnDetach() { }

        protected internal virtual void OnSerialize(SerializationInfo info) { }
        protected internal virtual void OnDeserialize(SerializationInfo info) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class AllowSinglePerEntityAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponentAttribute<T> : Attribute where T : Component
    {
        public RequireComponentAttribute() => throw new NotImplementedException();
    }
}