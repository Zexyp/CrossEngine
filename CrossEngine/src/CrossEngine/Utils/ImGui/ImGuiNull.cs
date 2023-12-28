using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CrossEngine.Utils.ImGui
{
    public static class ImGuiNull
    {
        public unsafe static bool Begin(string name, ref bool? open, ImGuiWindowFlags flags)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(name);
            byte result;
            byte openval = (open ?? false) ? (byte)1 : (byte)0;
            fixed (byte* p = buffer)
                result = ImGuiNative.igBegin(p, open.HasValue ? &openval : null, flags);
            if (open != null)
                open = openval != 0;
            return result != 0;
        }
    }
}
