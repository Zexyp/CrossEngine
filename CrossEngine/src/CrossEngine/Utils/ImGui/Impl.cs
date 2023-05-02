using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CrossEngine.Utils.ImGui
{
    static class Impl
    {
        public static unsafe void* New<T>() where T : struct
        {
            int l = Marshal.SizeOf<T>();
            var p = (void*)Marshal.AllocHGlobal(l);
            Unsafe.InitBlockUnaligned(p, 0, (uint)l);
            return p;
        }

        public static unsafe void Delete<T>(void* p) where T : struct
        {
            Marshal.FreeHGlobal((IntPtr)p);
        }
    }
}
