using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    public static class WeakReferenceExtension
    {
        public static T GetValue<T>(this WeakReference<T> value) where T : class
        {
            if (value.TryGetTarget(out T result))
                return result;
            return null;
        }

        public static void Dispose<T>(this WeakReference<T> value) where T : class, IDisposable
        {
            value.GetValue().Dispose();
            //value.SetTarget(null); // not sure
        }
    }
}
