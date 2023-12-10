using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    public static class SerializableExtensions
    {
        public static T? GetValue<T>(this SerializationInfo info, string name)
        {
            var result = info.GetValue(name, typeof(T));
            return result == null ? default : (T)result;
        }
    }
}
