using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Utils
{
    // https://stackoverflow.com/questions/57124243/winforms-dark-title-bar-on-windows-10
    // however i don't care so rip weird windows versions
    internal class Theming
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            int useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }
    }

    /*
    public static class LibDecor
    {
        const string LIBRARY = "libdecor-0.so.0";
        
        // libdecor_context_new
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_new(IntPtr display, IntPtr iface);

        // libdecor_context_free
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern void libdecor_context_free(IntPtr ctx);

        // libdecor_context_get_version
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint libdecor_context_get_version(IntPtr ctx);

        // libdecor_context_get_backend
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_backend(IntPtr ctx);

        // libdecor_context_get_display
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_display(IntPtr ctx);

        // libdecor_context_dispatch
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libdecor_context_dispatch(IntPtr ctx, int timeout);

        // libdecor_context_get_fd
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libdecor_context_get_fd(IntPtr ctx);

        // libdecor_context_get_registry
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_registry(IntPtr ctx);

        // libdecor_context_get_compositor
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_compositor(IntPtr ctx);

        // libdecor_context_get_seat
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_seat(IntPtr ctx);

        // libdecor_context_get_shm
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_shm(IntPtr ctx);

        // libdecor_context_get_drm
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_drm(IntPtr ctx);

        // libdecor_context_get_cursor
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_cursor(IntPtr ctx);

        // libdecor_context_get_output
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_output(IntPtr ctx);

        // libdecor_context_get_input
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_input(IntPtr ctx);

        // libdecor_context_get_data_device
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_data_device(IntPtr ctx);

        // libdecor_context_get_subcompositor
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_subcompositor(IntPtr ctx);

        // libdecor_context_get_xdg_shell
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_xdg_shell(IntPtr ctx);

        // libdecor_context_get_xdg_decoration_manager
        [DllImport(LIBRARY, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libdecor_context_get_xdg_decoration_manager(IntPtr ctx);
    }
    */
}
