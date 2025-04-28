using System;

using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using CrossEngine.Logging;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Rendering;

#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

namespace CrossEngine.Platform.OpenGL.Debugging
{
    internal static class GLDebugging
    {
        static readonly Logger GLLog;

        static GLDebugging()
        {
            GLLog = new Logger("OpenGL");
            GLLog.Pattern = "[%t][%n]%m";
        }

        static public void GLError(string text)
        {
            GLLog.Error(text);
        }

        static private void GLMessage(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
        {
            byte[] textArray = new byte[length];
            Marshal.Copy(message, textArray, 0, length);
            string text = Encoding.ASCII.GetString(textArray);

            switch (severity)
            {
                case GLEnum.DebugSeverityHigh:
                    {
                        GLLog.Error("[high]:\n" + text);
                    }
                    break;
                case GLEnum.DebugSeverityMedium:
                    {
                        GLLog.Warn("[medium]:\n" + text);
                    }
                    break;
                case GLEnum.DebugSeverityLow:
                    {
                        GLLog.Info("[low]:\n" + text);
                    }
                    break;
                case GLEnum.DebugSeverityNotification:
                    {
                        GLLog.Trace("[notification]:\n" + text);
                    }
                    break;
                default:
                    {
                        GLLog.Info(": " + text);
                    }
                    break;
            }
        }

        public static unsafe void Enable(LogLevel level = LogLevel.Trace)
        {
            GLLog.Level = level;
            gl.DebugMessageCallback(GLMessage, null);
            gl.Enable(GLEnum.DebugOutput);
            gl.Enable(GLEnum.DebugOutputSynchronous);

            GLLog.Trace(": gl debuging enabled");
        }
    }
}
