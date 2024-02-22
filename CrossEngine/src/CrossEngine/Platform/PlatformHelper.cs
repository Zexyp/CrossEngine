using CrossEngine.Display;
using CrossEngine.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Platform
{
    public static class PlatformHelper
    {
#if WASM
        private static HttpClient httpClient = new HttpClient();
#endif

        internal static Window CreateWindow()
        {
#if WINDOWS
            return new CrossEngine.Platform.Windows.GlfwWindow();
#elif WASM
            return new CrossEngine.Platform.Wasm.CanvasWindow();
#else
#error
#endif
        }

        internal static GraphicsApi GetGraphicsApi()
        {
#if WINDOWS
            return GraphicsApi.OpenGL;
#elif WASM
            return GraphicsApi.OpenGLES;
#else
#error
#endif
        }

        public static Stream FileRead(string path)
        {
#if WINDOWS
            return File.OpenRead(path);
#elif WASM
            // .Result creates deadlock
            return httpClient.GetStreamAsync(Path.Join(CrossEngine.Platform.Wasm.Interop.RootUri.ToString(), path)).GetAwaiter().GetResult();
#else
#error
#endif
        }

        public static Stream FileWrite(string path)
        {
#if WINDOWS
            return File.OpenWrite(path);
#elif WASM
            throw new NotSupportedException();
#else
#error
#endif
        }
    }
}
