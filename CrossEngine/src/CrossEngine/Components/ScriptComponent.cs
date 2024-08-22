using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components
{
    public class ScriptComponent : Component
    {
        internal const string FuncNameOnEnable = "OnEnable";
        internal const string FuncNameOnDisable = "OnDisable";
        internal const string FuncNameOnAttach = "OnAttach";
        internal const string FuncNameOnDetach = "OnDetach";
        internal const string FuncNameOnUpdate = "OnUpdate";
        internal const string FuncNameOnFixedUpdate = "OnFixedUpdate";

        public override object Clone()
        {
            var clone = (ScriptComponent)Activator.CreateInstance(this.GetType());
            
            clone.Enabled = this.Enabled;

            EnumerateFields(mi =>
            {
                mi.SetValue(clone, mi.GetValue(this));
            });
            return clone;
        }

        protected internal override void OnSerialize(SerializationInfo info)
        {
            base.OnSerialize(info);

            EnumerateFields(mi =>
            {
                info.AddValue(mi.Name, mi.GetValue(this));
            });
        }

        protected internal override void OnDeserialize(SerializationInfo info)
        {
            base.OnDeserialize(info);

            EnumerateFields(mi =>
            {
                var defaultValue = mi.GetValue(this);
                mi.SetValue(this, info.GetValue(mi.Name, mi.FieldType, defaultValue));
            });
        }

        delegate void FieldEnumerationHandler(FieldInfo info);
        private void EnumerateFields(FieldEnumerationHandler callback)
        {
            var type = this.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                if (!f.FieldType.IsPrimitive)
                    ScriptSystem.Log.Debug("non primitive field");
                ScriptSystem.Log.Trace($"enumerating field '{f.Name}'");
                callback.Invoke(f);
            }
        }
    }
}
