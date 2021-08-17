using System;

using System.Runtime.InteropServices;
using System.Security;

namespace OpenGL.Extensions
{
    //using GLenum = Int32;
    //using GLuint = UInt32;
    //using GLsizei = Int32;

    static unsafe class Debug
    {

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

        public static void Import(GetProcAddressHandler loader)
        {
            _glDebugMessageCallback = Marshal.GetDelegateForFunctionPointer<PFNGLDEBUGMESSAGECALLBACKPROC>(loader.Invoke("glDebugMessageCallback"));
        }
    }
}
