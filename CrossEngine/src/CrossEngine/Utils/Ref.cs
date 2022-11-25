using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine
{
    public static class Ref
    {
        public static bool IsNull<T>(Ref<T> value) where T : class => value == null || value.Value == null;
    }

    public class Ref<T> where T : class
    {
        private WeakReference<T> _value;

        public T? Value
        {
            get
            {
                bool succ = _value.TryGetTarget(out var v);
                return succ ? v : null;
            }
            set
            {
                _value.SetTarget(value);
            }
        }

        public Ref(T? value)
        {
            _value = new (value);
        }

        public static explicit operator Ref<T>(T? value) => new Ref<T>(value);
        public static explicit operator T?(Ref<T> value) => (value != null) ? value.Value : null;
    }
}
