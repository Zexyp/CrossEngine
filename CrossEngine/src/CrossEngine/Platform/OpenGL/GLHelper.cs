using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Platform.OpenGL
{
    static class GLHelper
    {
        public static string PtrToStringUtf8(IntPtr ptr)
        {
            var length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
                length++;
            var buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        public static string PtrToStringUtf8(IntPtr ptr, int length)
        {
            var buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
