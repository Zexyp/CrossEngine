using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Utils.Reflection
{
    static class MemberInfoExtensions
    {
        public static void SetFieldOrPropertyValue(this MemberInfo info, object? obj, object? value)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)info).SetValue(obj, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)info).SetValue(obj, value);
                    break;
                default: throw new InvalidOperationException();
            }
        }

        public static object? GetFieldOrPropertyValue(this MemberInfo info, object? obj)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)info).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo)info).GetValue(obj);
                default: throw new InvalidOperationException();
            }
        }
    }
}
