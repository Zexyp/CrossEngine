using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine
{
    public static class Ref
    {
        public static bool IsNull<T>(Ref<T> value) => value == null || value.Value == null;
    }

    public class Ref<T>
    {
        public T? Value;

        public Ref(T? value)
        {
            Value = value;
        }

        public static explicit operator Ref<T>(T? value) => new Ref<T>(value);
        public static explicit operator T?(Ref<T> value) => (value != null) ? value.Value : default(T);
    }
}
