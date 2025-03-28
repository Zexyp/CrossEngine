using CrossEngine.Display;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Services;
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
        static internal Logger Log = new Logger("platform");

#if WASM
        private static HttpClient httpClient = new HttpClient();
#endif

        internal static Window CreateWindow()
        {
#if WINDOWS || LINUX
            return new CrossEngine.Platform.Glfw.GlfwWindow();
#elif WASM
            return new CrossEngine.Platform.Wasm.CanvasWindow();
#else
#error
#endif
        }

        internal static GraphicsApi GetGraphicsApi()
        {
#if WINDOWS || LINUX
            return GraphicsApi.OpenGL;
#elif WASM
            return GraphicsApi.OpenGLES;
#else
#error
#endif
        }

        public static Task<Stream> FileRead(string path)
        {
            Log.Trace($"file read '{path}'");
#if WINDOWS || LINUX
            return Task.FromResult((Stream)File.OpenRead(path));
#elif WASM
            // .Result creates deadlock
            return httpClient.GetStreamAsync(Path.Join(CrossEngine.Platform.Wasm.Interop.RootUri.ToString(), path));
#else
#error
#endif
        }

        public static Stream FileCreate(string path)
        {
            Log.Trace($"file write '{path}'");
#if WINDOWS || LINUX
            var stream = File.Create(path);
            stream.SetLength(0);
            return stream;
#elif WASM
            throw new NotSupportedException();
#else
#error
#endif
        }

        //public void 
    }
}
