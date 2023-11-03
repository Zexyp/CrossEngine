using System;
using OpenGL;
using OpenGL.Extensions;
using static OpenGL.Extensions.Debug;

using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using CrossEngine.Logging;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Rendering;
using static CrossEngine.Platform.OpenGL.GLContext;
using Silk.NET.OpenGL;

namespace OpenGL.Extensions
{
    //using GLenum = Int32;
    //using GLuint = UInt32;
    //using GLsizei = Int32;

    static unsafe class Debug
    {
        const string ExtensionName = "GL_KHR_debug";

        public static void glDebugMessageCallback(GLDEBUGPROC callback, /*const*/ void* userParam) => _glDebugMessageCallback(DebugMessageCallbackHolder = callback, userParam);

        public const int GL_DEBUG_OUTPUT = 0x92E0;
        public const int GL_DEBUG_OUTPUT_SYNCHRONOUS = 0x8242;

        public const int GL_DEBUG_SEVERITY_HIGH = 0x9146;
        public const int GL_DEBUG_SEVERITY_MEDIUM = 0x9147;
        public const int GL_DEBUG_SEVERITY_LOW = 0x9148;
        public const int GL_DEBUG_SEVERITY_NOTIFICATION = 0x826B;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PFNGLDEBUGMESSAGECALLBACKPROC(GLDEBUGPROC callback, /*const*/ void* userParam);

        public delegate void GLDEBUGPROC(int source, int type, uint id, int severity, int length, IntPtr message, IntPtr userParam);

        private static PFNGLDEBUGMESSAGECALLBACKPROC _glDebugMessageCallback;

        static GLDEBUGPROC DebugMessageCallbackHolder;

        public static void Import(Func<string, IntPtr> loader)
        {
            GLExtensions.CheckExtension(ExtensionName);
            _glDebugMessageCallback = Marshal.GetDelegateForFunctionPointer<PFNGLDEBUGMESSAGECALLBACKPROC>(loader.Invoke("glDebugMessageCallback"));
        }
    }
}

namespace CrossEngine.Platform.OpenGL.Debugging
{
    internal static class GLDebugging
    {
        static Logger GLLog;

        static GLDebugging()
        {
            GLLog = new Logger("OpenGL");
            GLLog.Pattern = "[%t][%n]";
        }

        static public void GLError(string text)
        {
            GLLog.Error(text);
        }

        static private void GLMessage(int source, int type, uint id, int severity, int length, IntPtr message, IntPtr userParam)
        {
            byte[] textArray = new byte[length];
            Marshal.Copy(message, textArray, 0, length);
            string text = Encoding.ASCII.GetString(textArray);

            switch (severity)
            {
                case GL_DEBUG_SEVERITY_HIGH:
                    {
                        GLLog.Error("[high]: " + text);
                    }
                    break;
                case GL_DEBUG_SEVERITY_MEDIUM:
                    {
                        GLLog.Warn("[medium]: " + text);
                    }
                    break;
                case GL_DEBUG_SEVERITY_LOW:
                    {
                        GLLog.Info("[low]: " + text);
                    }
                    break;
                case GL_DEBUG_SEVERITY_NOTIFICATION:
                    {
                        GLLog.Trace("[notification]: " + text);
                    }
                    break;
                default:
                    {
                        GLLog.Info(": " + text);
                    }
                    break;
            }
        }

        public static unsafe void Enable(GLContext context, LogLevel level = LogLevel.Trace)
        {
            Debug.Import(GLFW.Glfw.GetProcAddress);

            GLLog.LogLevel = level;
            glDebugMessageCallback(GLMessage, null);
            gl.Enable(GLEnum.DebugOutput);
            gl.Enable(GLEnum.DebugOutputSynchronous);

            GLLog.Trace(": gl debuging enabled");
        }
    }
}
