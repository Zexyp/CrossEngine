using System;
using static OpenGL.GL;

using System.Diagnostics;
using System.Runtime.CompilerServices;

using CrossEngine.Logging;

namespace CrossEngine.Rendering.Errors
{
    public static class GLError
    {
        public static bool activeAssert;

        public static void ClearError()
        {
            while (glGetError() != GL_NO_ERROR) ;
        }

        public static bool CheckError()
        {
            int error;
            while ((error = glGetError()) != GL_NO_ERROR)
            {
                Log.GLError("(" + error + ")");
                return false;
            }
            return true;
        }

        public static void LogError()
        {
            int error;
            while ((error = glGetError()) != GL_NO_ERROR)
            {
                Log.GLError("(" + error + ")");
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
                Log.Core.Error(errorString);
                if (activeAssert) Debug.Assert(false, errorString);
            }
        }
    }
}
