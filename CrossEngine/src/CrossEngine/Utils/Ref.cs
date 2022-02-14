using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    public class Ref<T>
    {
        public T Value;

        public Ref(T value)
        {
            Value = value;
        }

        public static explicit operator Ref<T>(T value) => new Ref<T>(value);
        public static explicit operator T(Ref<T> value) => value.Value;

        [Obsolete("Consider comparing actual values")]
        public static bool operator ==(Ref<T> left, Ref<T> right) => left == right;
        [Obsolete("Consider comparing actual values")]
        public static bool operator !=(Ref<T> left, Ref<T> right) => left != right;
    }
}
