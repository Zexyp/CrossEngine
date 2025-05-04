using CrossEngine.Display;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Core.Services;
using Silk.NET.GLFW;
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

        internal static void Init()
        {
#if WINDOWS || LINUX
            static void GlfwErrorCallback(ErrorCode code, string message)
            {
                Log.Error(((int)code) + " (" + code.ToString() + "): " + message);
            }

            Glfw.GlfwWindow.glfw = Silk.NET.GLFW.Glfw.GetApi();

            Glfw.GlfwWindow.glfw.Init();

            Glfw.GlfwWindow.glfw.SetErrorCallback(GlfwErrorCallback);

            Glfw.GlfwWindow.glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
            Glfw.GlfwWindow.glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            Glfw.GlfwWindow.glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            Glfw.GlfwWindow.glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            //Glfw.GlfwWindow.glfw.WindowHint(WindowHintBool.DoubleBuffer, true);
            //glfw.WindowHint(WindowHintBool.Decorated, true);
            //glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
#endif
        }

        internal static void Terminate()
        {
#if WINDOWS || LINUX
            CrossEngine.Platform.Glfw.GlfwWindow.glfw.Terminate();
#endif
        }

        internal static Window CreateWindow()
        {
#if WINDOWS || LINUX
            return new CrossEngine.Platform.Glfw.GlfwWindow();
#elif WASM
            return new CrossEngine.Platform.Wasm.CanvasWindow();
#else
#error No platform window creation
#endif
        }

        internal static GraphicsApi GetGraphicsApi()
        {
#if OPENGL_ES
            return GraphicsApi.OpenGLES;
#elif OPENGL
            return GraphicsApi.OpenGL;
#elif GDI
            return GraphicsApi.GDI;
#else
#error Unknown platform graphics backend
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
#error No platform file reading
#endif
        }

        public static Stream FileCreate(string path)
        {
            Log.Trace($"file create '{path}'");
#if WINDOWS || LINUX
            var stream = File.Create(path);
            stream.SetLength(0);
            return stream;
#elif WASM
            throw new NotSupportedException();
#else
#error No platform file creation
#endif
        }

        //public void 
    }
}
