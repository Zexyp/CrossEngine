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

        public bool Enabled;

        public Component()
        {
            
        }

        public virtual object Clone()
        {
            // mby use OnSerialize and OnDeserialize to copy data (would be cool 😎)
            Log.Default.Trace("using default ctor for cloning of component");

            var comp = (Component)Activator.CreateInstance(this.GetType());
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

        protected internal virtual void OnSerialize(SerializationInfo info) => Serializer.UseAttributesWrite(this, info);
        protected internal virtual void OnDeserialize(SerializationInfo info) => Serializer.UseAttributesRead(this, info);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class AllowSinglePerEntityAttribute : Attribute
    {

    }

    [Obsolete("not implemented")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponentAttribute<T> : Attribute where T : Component
    {
        public bool Inherit;
        public RequireComponentAttribute(bool inherit = true)
        {
            Inherit = inherit;
        }
    }
}