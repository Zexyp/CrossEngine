using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using CrossEngine.Logging;
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
    static class GLError
    {
        public static bool activeAssert;

        public static void ClearError()
        {
            while (gl.GetError() != GLEnum.NoError) ;
        }

        public static bool CheckError()
        {
            GLEnum error;
            while ((error = gl.GetError()) != GLEnum.NoError)
            {
                RendererApi.Log.Error("(" + error + ")");
                return false;
            }
            return true;
        }

        public static void LogError()
        {
            GLEnum error;
            while ((error = gl.GetError()) != GLEnum.NoError)
            {
                RendererApi.Log.Error("(" + error + ")");
                //error.ToString("X")
            }
        }

        public static void Call(Action action, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filepath = null)
        {
            ClearError();
            action.Invoke();
            if (!CheckError())
            {
                string errorString = "at " + caller + " in " + filepath + ":line " + line;
                RendererApi.Log.Error(errorString);
                if (activeAssert) Debug.Assert(false, errorString);
            }
        }
    }
}
