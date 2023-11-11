using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Logging;

namespace CrossEngine.Ecs
{
    public abstract class Component : ICloneable
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

                if (_enabled) Enable();
                else Disable();

                EnabledChanged?.Invoke(this);
            }
        }

        private bool _enabled = true;

        protected internal virtual void Enable() { }
        protected internal virtual void Disable() { }

        protected internal virtual void Attach() { }
        protected internal virtual void Detach() { }


        //protected internal virtual void Serialize(SerializationInfo info) { }
        //protected internal virtual void Deserialize(SerializationInfo info) { }

        protected virtual Component CreateClone()
        {
            Log.Default.Info("using default constructor for cloning component");
            return (Component)Activator.CreateInstance(this.GetType());
        }

        public object Clone()
        {
            var comp = CreateClone();
            comp.Enabled = this.Enabled;
            return comp;
        }

        //public void GetObjectData(SerializationInfo info)
        //{
        //    info.AddValue(nameof(Enabled), Enabled);
        //    Serialize(info);
        //}
        //
        //public void SetObjectData(SerializationInfo info)
        //{
        //    Enabled = info.GetValue<bool>(nameof(Enabled));
        //    Deserialize(info);
        //}
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