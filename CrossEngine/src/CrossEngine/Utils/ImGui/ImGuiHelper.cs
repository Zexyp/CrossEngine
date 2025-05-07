using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Utils.ImGui
{
    public static class ImGuiHelper
    {
        public static IntPtr ImDrawCallback_ResetRenderState = -8;

        public unsafe delegate void ImDrawCallback(ImDrawList* parent_list, ImDrawCmd* cmd);
    }
}
