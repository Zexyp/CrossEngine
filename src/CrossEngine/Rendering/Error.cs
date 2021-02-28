using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrossEngine.Rendering.Errors
{
    public static class Error
    {
        public static bool activeAssert = true;

        public static void GLClearError()
        {
            while (glGetError() != GL_NO_ERROR) ;
        }

        public static bool GLCheckError()
        {
            int error;
            while ((error = glGetError()) != GL_NO_ERROR)
            {
                Log.GLError("(" + error + ")");
                return false;
            }
            return true;
        }

        public static void GLLogError()
        {
            int error;
            while ((error = glGetError()) != GL_NO_ERROR)
            {
                Log.GLError("(" + error + ")");
                //error.ToString("X")
            }
        }

        public static void GLCall(Action action, [CallerLineNumber] int line = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filepath = null)
        {
            GLClearError();
            action.Invoke();
            if (!GLCheckError())
            {
                string errorString = "at " + caller + " in " + filepath + ":line " + line;
                Log.Error(errorString);
                if (activeAssert) Debug.Assert(false, errorString);
            }
        }
    }
}
