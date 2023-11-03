// dear imgui: Renderer Backend for modern OpenGL with shaders / programmatic pipeline
// - Desktop GL: 2.x 3.x 4.x
// - Embedded GL: ES 2.0 (WebGL 1.0), ES 3.0 (WebGL 2.0)
// This needs to be used along with a Platform Backend (e.g. GLFW, SDL, Win32, custom..)

// Implemented features:
//  [X] Renderer: User texture binding. Use 'GLuint' OpenGL texture identifier as void*/ImTextureID. Read the FAQ about ImTextureID!
//  [x] Renderer: Large meshes support (64k+ vertices) with 16-bit indices (Desktop OpenGL only).

// You can use unmodified imgui_impl_* files in your project. See examples/ folder for examples of using this.
// Prefer including the entire imgui/ repository into your project (either as a copy or as a submodule), and only build the backends you need.
// If you are new to Dear ImGui, read documentation from the docs/ folder + read the top of imgui.cpp.
// Read online: https://github.com/ocornut/imgui/tree/master/docs

// CHANGELOG
// (minor and older changes stripped away, please see git history for details)
//  2023-04-18: OpenGL: Restore front and back polygon mode separately when supported by context. (#6333)
//  2023-03-23: OpenGL: Properly restoring "no shader program bound" if it was the case prior to running the rendering function. (#6267, #6220, #6224)
//  2023-03-15: OpenGL: Fixed GL loader crash when GL_VERSION returns NULL. (#6154, #4445, #3530)
//  2023-03-06: OpenGL: Fixed restoration of a potentially deleted OpenGL program, by calling glIsProgram(). (#6220, #6224)
//  2022-11-09: OpenGL: Reverted use of glBufferSubData(), too many corruptions issues + old issues seemingly can't be reproed with Intel drivers nowadays (revert 2021-12-15 and 2022-05-23 changes).
//  2022-10-11: Using 'nullptr' instead of 'NULL' as per our switch to C++11.
//  2022-09-27: OpenGL: Added ability to '#define IMGUI_IMPL_OPENGL_DEBUG'.
//  2022-05-23: OpenGL: Reworking 2021-12-15 "Using buffer orphaning" so it only happens on Intel GPU, seems to cause problems otherwise. (#4468, #4825, #4832, #5127).
//  2022-05-13: OpenGL: Fixed state corruption on OpenGL ES 2.0 due to not preserving GL_ELEMENT_ARRAY_BUFFER_BINDING and vertex attribute states.
//  2021-12-15: OpenGL: Using buffer orphaning + glBufferSubData(), seems to fix leaks with multi-viewports with some Intel HD drivers.
//  2021-08-23: OpenGL: Fixed ES 3.0 shader ("#version 300 es") use normal precision floats to avoid wobbly rendering at HD resolutions.
//  2021-08-19: OpenGL: Embed and use our own minimal GL loader (imgui_impl_opengl3_loader.h), removing requirement and support for third-party loader.
//  2021-06-29: Reorganized backend to pull data from a single structure to facilitate usage with multiple-contexts (all g_XXXX access changed to bd->XXXX).
//  2021-06-25: OpenGL: Use OES_vertex_array extension on Emscripten + backup/restore current state.
//  2021-06-21: OpenGL: Destroy individual vertex/fragment shader objects right after they are linked into the main shader.
//  2021-05-24: OpenGL: Access GL_CLIP_ORIGIN when "GL_ARB_clip_control" extension is detected, inside of just OpenGL 4.5 version.
//  2021-05-19: OpenGL: Replaced direct access to ImDrawCmd::TextureId with a call to ImDrawCmd::GetTexID(). (will become a requirement)
//  2021-04-06: OpenGL: Don't try to read GL_CLIP_ORIGIN unless we're OpenGL 4.5 or greater.
//  2021-02-18: OpenGL: Change blending equation to preserve alpha in output buffer.
//  2021-01-03: OpenGL: Backup, setup and restore GL_STENCIL_TEST state.
//  2020-10-23: OpenGL: Backup, setup and restore GL_PRIMITIVE_RESTART state.
//  2020-10-15: OpenGL: Use glGetString(GL_VERSION) instead of glGetIntegerv(GL_MAJOR_VERSION, ...) when the later returns zero (e.g. Desktop GL 2.x)
//  2020-09-17: OpenGL: Fix to avoid compiling/calling glBindSampler() on ES or pre 3.3 context which have the defines set by a loader.
//  2020-07-10: OpenGL: Added support for glad2 OpenGL loader.
//  2020-05-08: OpenGL: Made default GLSL version 150 (instead of 130) on OSX.
//  2020-04-21: OpenGL: Fixed handling of glClipControl(GL_UPPER_LEFT) by inverting projection matrix.
//  2020-04-12: OpenGL: Fixed context version check mistakenly testing for 4.0+ instead of 3.2+ to enable ImGuiBackendFlags_RendererHasVtxOffset.
//  2020-03-24: OpenGL: Added support for glbinding 2.x OpenGL loader.
//  2020-01-07: OpenGL: Added support for glbinding 3.x OpenGL loader.
//  2019-10-25: OpenGL: Using a combination of GL define and runtime GL version to decide whether to use glDrawElementsBaseVertex(). Fix building with pre-3.2 GL loaders.
//  2019-09-22: OpenGL: Detect default GL loader using __has_include compiler facility.
//  2019-09-16: OpenGL: Tweak initialization code to allow application calling ImGui_ImplOpenGL3_CreateFontsTexture() before the first NewFrame() call.
//  2019-05-29: OpenGL: Desktop GL only: Added support for large mesh (64K+ vertices), enable ImGuiBackendFlags_RendererHasVtxOffset flag.
//  2019-04-30: OpenGL: Added support for special ImDrawCallback_ResetRenderState callback to reset render state.
//  2019-03-29: OpenGL: Not calling glBindBuffer more than necessary in the render loop.
//  2019-03-15: OpenGL: Added a GL call + comments in ImGui_ImplOpenGL3_Init() to detect uninitialized GL function loaders early.
//  2019-03-03: OpenGL: Fix support for ES 2.0 (WebGL 1.0).
//  2019-02-20: OpenGL: Fix for OSX not supporting OpenGL 4.5, we don't try to read GL_CLIP_ORIGIN even if defined by the headers/loader.
//  2019-02-11: OpenGL: Projecting clipping rectangles correctly using draw_data->FramebufferScale to allow multi-viewports for retina display.
//  2019-02-01: OpenGL: Using GLSL 410 shaders for any version over 410 (e.g. 430, 450).
//  2018-11-30: Misc: Setting up io.BackendRendererName so it can be displayed in the About Window.
//  2018-11-13: OpenGL: Support for GL 4.5's glClipControl(GL_UPPER_LEFT) / GL_CLIP_ORIGIN.
//  2018-08-29: OpenGL: Added support for more OpenGL loaders: glew and glad, with comments indicative that any loader can be used.
//  2018-08-09: OpenGL: Default to OpenGL ES 3 on iOS and Android. GLSL version default to "#version 300 ES".
//  2018-07-30: OpenGL: Support for GLSL 300 ES and 410 core. Fixes for Emscripten compilation.
//  2018-07-10: OpenGL: Support for more GLSL versions (based on the GLSL version string). Added error output when shaders fail to compile/link.
//  2018-06-08: Misc: Extracted imgui_impl_opengl3.cpp/.h away from the old combined GLFW/SDL+OpenGL3 examples.
//  2018-06-08: OpenGL: Use draw_data->DisplayPos and draw_data->DisplaySize to setup projection matrix and clipping rectangle.
//  2018-05-25: OpenGL: Removed unnecessary backup/restore of GL_ELEMENT_ARRAY_BUFFER_BINDING since this is part of the VAO state.
//  2018-05-14: OpenGL: Making the call to glBindSampler() optional so 3.2 context won't fail if the function is a nullptr pointer.
//  2018-03-06: OpenGL: Added const char* glsl_version parameter to ImGui_ImplOpenGL3_Init() so user can override the GLSL version e.g. "#version 150".
//  2018-02-23: OpenGL: Create the VAO in the render function so the setup can more easily be used with multiple shared GL context.
//  2018-02-16: Misc: Obsoleted the io.RenderDrawListsFn callback and exposed ImGui_ImplSdlGL3_RenderDrawData() in the .h file so you can call it yourself.
//  2018-01-07: OpenGL: Changed GLSL shader version from 330 to 150.
//  2017-09-01: OpenGL: Save and restore current bound sampler. Save and restore current polygon mode.
//  2017-05-01: OpenGL: Fixed save and restore of current blend func state.
//  2017-05-01: OpenGL: Fixed save and restore of current GL_ACTIVE_TEXTURE.
//  2016-09-05: OpenGL: Fixed save and restore of current scissor rectangle.
//  2016-07-29: OpenGL: Explicitly setting GL_UNPACK_ROW_LENGTH to reduce issues because SDL changes it. (#752)

//----------------------------------------
// OpenGL    GLSL      GLSL
// version   version   string
//----------------------------------------
//  2.0       110       "#version 110"
//  2.1       120       "#version 120"
//  3.0       130       "#version 130"
//  3.1       140       "#version 140"
//  3.2       150       "#version 150"
//  3.3       330       "#version 330 core"
//  4.0       400       "#version 400 core"
//  4.1       410       "#version 410 core"
//  4.2       420       "#version 410 core"
//  4.3       430       "#version 430 core"
//  ES 2.0    100       "#version 100"      = WebGL 1.0
//  ES 3.0    300       "#version 300 es"   = WebGL 2.0
//----------------------------------------

//#if defined(_MSC_VER) && !defined(_CRT_SECURE_NO_WARNINGS)
//#define _CRT_SECURE_NO_WARNINGS
//#endif
//
//#include "imgui.h"
//#include "imgui_impl_opengl3.h"
//#include <stdio.h>
//#if defined(_MSC_VER) && _MSC_VER <= 1500 // MSVC 2008 or earlier
//#include <stddef.h>     // intptr_t
//#else
//#include <stdint.h>     // intptr_t
//#endif
//#if defined(__APPLE__)
//#include <TargetConditionals.h>
//#endif
//
//// Clang/GCC warnings with -Weverything
//#if defined(__clang__)
//#pragma clang diagnostic push
//#pragma clang diagnostic ignored "-Wold-style-cast"         // warning: use of old-style cast
//#pragma clang diagnostic ignored "-Wsign-conversion"        // warning: implicit conversion changes signedness
//#pragma clang diagnostic ignored "-Wunused-macros"          // warning: macro is not used
//#pragma clang diagnostic ignored "-Wnonportable-system-include-path"
//#pragma clang diagnostic ignored "-Wcast-function-type"     // warning: cast between incompatible function types (for loader)
//#endif
//#if defined(__GNUC__)
//#pragma GCC diagnostic push
//#pragma GCC diagnostic ignored "-Wpragmas"                  // warning: unknown option after '#pragma GCC diagnostic' kind
//#pragma GCC diagnostic ignored "-Wunknown-warning-option"   // warning: unknown warning group 'xxx'
//#pragma GCC diagnostic ignored "-Wcast-function-type"       // warning: cast between incompatible function types (for loader)
//#endif
//
//// GL includes
//#if defined(IMGUI_IMPL_OPENGL_ES2)
//#if (defined(__APPLE__) && (TARGET_OS_IOS || TARGET_OS_TV))
//#include <OpenGLES/ES2/gl.h>    // Use GL ES 2
//#else
//#include <GLES2/gl2.h>          // Use GL ES 2
//#endif
//#if defined(__EMSCRIPTEN__)
//#ifndef GL_GLEXT_PROTOTYPES
//#define GL_GLEXT_PROTOTYPES
//#endif
//#include <GLES2/gl2ext.h>
//#endif
//#elif defined(IMGUI_IMPL_OPENGL_ES3)
//#if (defined(__APPLE__) && (TARGET_OS_IOS || TARGET_OS_TV))
//#include <OpenGLES/ES3/gl.h>    // Use GL ES 3
//#else
//#include <GLES3/gl3.h>          // Use GL ES 3
//#endif
//#elif !defined(IMGUI_IMPL_OPENGL_LOADER_CUSTOM)
//// Modern desktop OpenGL doesn't have a standard portable header file to load OpenGL function pointers.
//// Helper libraries are often used for this purpose! Here we are using our own minimal custom loader based on gl3w.
//// In the rest of your app/engine, you can use another loader of your choice (gl3w, glew, glad, glbinding, glext, glLoadGen, etc.).
//// If you happen to be developing a new feature for this backend (imgui_impl_opengl3.cpp):
//// - You may need to regenerate imgui_impl_opengl3_loader.h to add new symbols. See https://github.com/dearimgui/gl3w_stripped
//// - You can temporarily use an unstripped version. See https://github.com/dearimgui/gl3w_stripped/releases
//// Changes to this backend using new APIs should be accompanied by a regenerated stripped loader version.
//#define IMGL3W_IMPL
//#include "imgui_impl_opengl3_loader.h"
//#endif
//
//// Vertex arrays are not supported on ES2/WebGL1 unless Emscripten which uses an extension
//#if !IMGUI_IMPL_OPENGL_ES2
//#define IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
//#elif __EMSCRIPTEN__
//#define IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
//#define glBindVertexArray       glBindVertexArrayOES
//#define glGenVertexArrays       glGenVertexArraysOES
//#define glDeleteVertexArrays    glDeleteVertexArraysOES
//#define GL_VERTEX_ARRAY_BINDING GL_VERTEX_ARRAY_BINDING_OES
//#endif
//
//// Desktop GL 2.0+ has glPolygonMode() which GL ES and WebGL don't have.
//#ifdef GL_POLYGON_MODE
//#define IMGUI_IMPL_HAS_POLYGON_MODE
//#endif
//
//// Desktop GL 3.2+ has glDrawElementsBaseVertex() which GL ES and WebGL don't have.
//#if !defined(IMGUI_IMPL_OPENGL_ES2) && !defined(IMGUI_IMPL_OPENGL_ES3) && defined(GL_VERSION_3_2)
//#define IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET
//#endif
//
//// Desktop GL 3.3+ has glBindSampler()
//#if !defined(IMGUI_IMPL_OPENGL_ES2) && !defined(IMGUI_IMPL_OPENGL_ES3) && defined(GL_VERSION_3_3)
//#define IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
//#endif
//
//// Desktop GL 3.1+ has GL_PRIMITIVE_RESTART state
//#if !defined(IMGUI_IMPL_OPENGL_ES2) && !defined(IMGUI_IMPL_OPENGL_ES3) && defined(GL_VERSION_3_1)
//#define IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
//#endif
//
//// Desktop GL use extension detection
//#if !defined(IMGUI_IMPL_OPENGL_ES2) && !defined(IMGUI_IMPL_OPENGL_ES3)
//#define IMGUI_IMPL_OPENGL_MAY_HAVE_EXTENSIONS
//#endif
//
//// [Debugging]
////#define IMGUI_IMPL_OPENGL_DEBUG
//#ifdef IMGUI_IMPL_OPENGL_DEBUG
//#include <stdio.h>
//#define GL_CALL(_CALL)      do { _CALL; GLenum gl_err = glGetError(); if (gl_err != 0) fprintf(stderr, "GL error 0x%x returned from '%s'.\n", gl_err, #_CALL); } while (0)  // Call with error check
//#else
//#define GL_CALL(_CALL)      _CALL   // Call without error check
//#endif

#define IMGUI_IMPL_OPENGL_LOADER_CUSTOM
#define IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET
#define IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
#define IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
#define IMGUI_IMPL_OPENGL_MAY_HAVE_EXTENSIONS
#define IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
#define IMGUI_IMPL_HAS_POLYGON_MODE

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Evergine.Bindings.Imgui;
using static Evergine.Bindings.Imgui.ImguiNative;
using static CrossEngine.Platform.OpenGL.GLContext;
using GLEnum = Silk.NET.OpenGL.GLEnum;
using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Utils.ImGui
{
    using GLuint = UInt32;
    using GLint = Int32;
    using GLsizeiptr = UInt32;
    using GLboolean = Boolean;
    using GLsizei = UInt32;
    using GLenum = Int32;

    using ImDrawIdx = UInt16;
    using ImTextureID = IntPtr;

    static class ImplOpenGL
    {
        const int GLSL_VERSION_STRING_LENGTH = 32;

        // OpenGL Data
        unsafe struct ImGui_ImplOpenGL3_Data
        {
            public GLuint           GlVersion;               // Extracted at runtime using GL_MAJOR_VERSION, GL_MINOR_VERSION queries (e.g. 320 for GL 3.2)
            public fixed char       GlslVersionString[GLSL_VERSION_STRING_LENGTH];   // Specified by user or detected based on compile time GL settings.
            public bool             GlProfileIsCompat;
            public GLint            GlProfileMask;
            public GLuint           FontTexture;
            public GLuint           ShaderHandle;
            public GLint            AttribLocationTex;       // Uniforms location
            public GLint            AttribLocationProjMtx;
            public GLuint           AttribLocationVtxPos;    // Vertex attributes location
            public GLuint           AttribLocationVtxUV;
            public GLuint           AttribLocationVtxColor;
            public uint             VboHandle, ElementsHandle;
            public GLsizeiptr       VertexBufferSize;
            public GLsizeiptr       IndexBufferSize;
            public bool             HasClipOrigin;
            public bool             UseBufferSubData;
        };

        // Backend data stored in io.BackendRendererUserData to allow support for multiple Dear ImGui contexts
        // It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
        static unsafe ImGui_ImplOpenGL3_Data* ImGui_ImplOpenGL3_GetBackendData()
        {
            return igGetCurrentContext() != IntPtr.Zero ? (ImGui_ImplOpenGL3_Data*)igGetIO()->BackendRendererUserData : null;
        }

        // OpenGL vertex attribute state (for ES 1.0 and ES 2.0 only)
#if !IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
        unsafe struct ImGui_ImplOpenGL3_VtxAttribState
        {
            public GLint Enabled, Size, Normalized, Stride;
            public VertexAttribPointerType Type;
            public void* Ptr;

            public void GetState(GLuint index)
            {
                fixed (ImGui_ImplOpenGL3_VtxAttribState* p = &this)
                {
                    glGetVertexAttribiv(index, (uint)VertexArrayPName.VertexAttribArrayEnabled, &p->Enabled);
                    glGetVertexAttribiv(index, (uint)VertexArrayPName.VertexAttribArraySize, &p->Size);
                    glGetVertexAttribiv(index, (uint)VertexArrayPName.VertexAttribArrayType, (int*)&p->Type);
                    glGetVertexAttribiv(index, (uint)VertexArrayPName.VertexAttribArrayNormalized, &p->Normalized);
                    glGetVertexAttribiv(index, (uint)VertexArrayPName.VertexAttribArrayStride, &p->Stride);
                    glGetVertexAttribPointerv(index, (uint)Extensions.VertexAttribArrayPointer, &p->Ptr);
                }
            }
            public void SetState(GLuint index)
            {
                glVertexAttribPointer(index, Size, Type, Normalized != 0, Stride, Ptr);
                if (Enabled != 0) glEnableVertexAttribArray(index); else glDisableVertexAttribArray(index);
            }
        };
#endif

        // Functions
        public static unsafe bool ImGui_ImplOpenGL3_Init(string glsl_version)
        {
            ImGuiIO* io = igGetIO();
            Debug.Assert(io->BackendRendererUserData == null, "Already initialized a renderer backend!");

            // Initialize our loader
#if !IMGUI_IMPL_OPENGL_ES2 && !IMGUI_IMPL_OPENGL_ES3 && !IMGUI_IMPL_OPENGL_LOADER_CUSTOM
            if (imgl3wInit() != 0)
            {
                Console.Error.Write("Failed to initialize OpenGL loader!\n");
                return false;
            }
#endif

            // Setup backend capabilities flags
            ImGui_ImplOpenGL3_Data* bd = (ImGui_ImplOpenGL3_Data*)Impl.New<ImGui_ImplOpenGL3_Data>();
            io->BackendRendererUserData = (void*)bd;
            io->BackendRendererName = (byte*)Marshal.StringToHGlobalAuto("imgui_impl_opengl3");

            // Query for GL version (e.g. 320 for GL 3.2)
#if !IMGUI_IMPL_OPENGL_ES2
            GLint major = 0;
            GLint minor = 0;
            gl.GetInteger(GLEnum.MajorVersion, &major);
            gl.GetInteger(GLEnum.MinorVersion, &minor);
            if (major == 0 && minor == 0)
            {
                // Query GL_VERSION in desktop GL 2.x, the string will start with "<major>.<minor>"
                string gl_version = GLHelper.PtrToStringUtf8((IntPtr)gl.GetString(GLEnum.Version));
                // kanón na vrabce
                var groups = Regex.Match(new string(bd->GlslVersionString), @"^(\d+)$\.(\d+)$").Groups;
                major = int.Parse(groups[1].Value);
                minor = int.Parse(groups[2].Value);
            }
            bd->GlVersion = (GLuint)(major * 100 + minor * 10);
#if GL_CONTEXT_PROFILE_MASK
            glGetIntegerv(GL_CONTEXT_PROFILE_MASK, &bd->GlProfileMask);
            bd->GlProfileIsCompat = (bd->GlProfileMask & GL_CONTEXT_COMPATIBILITY_PROFILE_BIT) != 0;
#endif

            bd->UseBufferSubData = false;
            /*
            // Query vendor to enable glBufferSubData kludge
#ifdef _WIN32
            if (const char* vendor = (const char*)glGetString(GL_VENDOR))
                if (strncmp(vendor, "Intel", 5) == 0)
                    bd->UseBufferSubData = true;
#endif
            */
#else
            bd->GlVersion = 200; // GLES 2
#endif

#if IMGUI_IMPL_OPENGL_DEBUG
            Console.Write("GL_MAJOR_VERSION = {0}\nGL_MINOR_VERSION = {1}\nGL_VENDOR = '{2}'\nGL_RENDERER = '{3}'\n", major, minor, (const char*)glGetString(StringName.Vendor), (const char*)glGetString(StringName.Renderer)); // [DEBUG]
#endif

#if IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET
            if (bd->GlVersion >= 320)
                io->BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;  // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.
#endif

            // Store GLSL version string so we can refer to it later in case we recreate shaders.
            // Note: GLSL version is NOT the same as GL version. Leave this to nullptr if unsure.
            if (glsl_version == null)
            {
#if IMGUI_IMPL_OPENGL_ES2
                glsl_version = "#version 100";
#elif IMGUI_IMPL_OPENGL_ES3
                glsl_version = "#version 300 es";
#elif __APPLE__
                glsl_version = "#version 150";
#else
                glsl_version = "#version 130";
#endif
            }
            Debug.Assert(glsl_version.Length + 2 < GLSL_VERSION_STRING_LENGTH);
            char[] buf = (glsl_version + "\n").ToCharArray();
            Marshal.Copy(buf, 0, (IntPtr)bd->GlslVersionString, buf.Length);

            // Make an arbitrary GL call (we don't actually need the result)
            // IF YOU GET A CRASH HERE: it probably means the OpenGL function loader didn't do its job. Let us know!
            GLint current_texture;
            gl.GetInteger(GLEnum.TextureBinding2D, &current_texture);

            // Detect extensions we support
            bd->HasClipOrigin = (bd->GlVersion >= 450);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_EXTENSIONS
            GLint num_extensions = 0;
            gl.GetInteger(GLEnum.NumExtensions, &num_extensions);
            for (GLint i = 0; i < num_extensions; i++)
            {
                string extension = GLHelper.PtrToStringUtf8((IntPtr)gl.GetString(GLEnum.Extensions, (uint)i));
                if (!String.IsNullOrEmpty(extension) && extension == "GL_ARB_clip_control")
                    bd->HasClipOrigin = true;
            }
#endif

            return true;
        }

        static unsafe void ImGui_ImplOpenGL3_Shutdown()
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();
            Debug.Assert(bd != null, "No renderer backend to shutdown, or already shutdown?");
            ImGuiIO* io = igGetIO();

            ImGui_ImplOpenGL3_DestroyDeviceObjects();
            io->BackendRendererName = null;
            io->BackendRendererUserData = null;
            io->BackendFlags &= ~ImGuiBackendFlags.RendererHasVtxOffset;
            Impl.Delete<ImGui_ImplOpenGL3_Data>(bd);
        }

        public static unsafe void ImGui_ImplOpenGL3_NewFrame()
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();
            Debug.Assert(bd != null, "Did you call ImGui_ImplOpenGL3_Init()?");

            if (bd->ShaderHandle == 0)
                ImGui_ImplOpenGL3_CreateDeviceObjects();
        }

        static unsafe void ImGui_ImplOpenGL3_SetupRenderState(ImDrawData* draw_data, int fb_width, int fb_height, GLuint vertex_array_object)
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();

            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, polygon fill
            gl.Enable(GLEnum.Blend);
            gl.BlendEquation(GLEnum.FuncAdd);
            gl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
            gl.Disable(GLEnum.CullFace);
            gl.Disable(GLEnum.DepthTest);
            gl.Disable(GLEnum.StencilTest);
            gl.Enable(GLEnum.ScissorTest);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
            if (bd->GlVersion >= 310)
                gl.Disable(GLEnum.PrimitiveRestart);
#endif
#if IMGUI_IMPL_HAS_POLYGON_MODE
            gl.Disable(GLEnum.PrimitiveRestart);
#endif

            // Support for GL 4.5 rarely used glClipControl(GL_UPPER_LEFT)
#if GL_CLIP_ORIGIN
            bool clip_origin_lower_left = true;
            if (bd->HasClipOrigin)
            {
                GLenum current_clip_origin = 0; glGetIntegerv(GL_CLIP_ORIGIN, (GLint*)&current_clip_origin);
                if (current_clip_origin == GL_UPPER_LEFT)
                    clip_origin_lower_left = false;
            }
#endif

            // Setup viewport, orthographic projection matrix
            // Our visible imgui space lies from draw_data->DisplayPos (top left) to draw_data->DisplayPos+data_data->DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
            gl.Viewport(0, 0, (uint)fb_width, (uint)fb_height);
            float L = draw_data->DisplayPos.X;
            float R = draw_data->DisplayPos.X + draw_data->DisplaySize.X;
            float T = draw_data->DisplayPos.Y;
            float B = draw_data->DisplayPos.Y + draw_data->DisplaySize.Y;
#if GL_CLIP_ORIGIN
            if (!clip_origin_lower_left) { float tmp = T; T = B; B = tmp; } // Swap top and bottom if origin is upper left
#endif
            float[,] ortho_projection = new float[4, 4]
            {
                { 2.0f/(R-L),   0.0f,         0.0f,   0.0f },
                { 0.0f,         2.0f/(T-B),   0.0f,   0.0f },
                { 0.0f,         0.0f,        -1.0f,   0.0f },
                { (R+L)/(L-R),  (T+B)/(B-T),  0.0f,   1.0f },
            };
            gl.UseProgram(bd->ShaderHandle);
            gl.Uniform1(bd->AttribLocationTex, 0);
            fixed (float* p = &ortho_projection[0, 0])
                gl.UniformMatrix4(bd->AttribLocationProjMtx, 1, false, p);

#if IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
            if (bd->GlVersion >= 330)
                gl.BindSampler(0, 0); // We use combined texture/sampler state. Applications using GL 3.3 may set that otherwise.
#endif

            //(void)vertex_array_object;
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            gl.BindVertexArray(vertex_array_object);
#endif

            // Bind vertex/index buffers and setup attributes for ImDrawVert
            gl.BindBuffer(GLEnum.ArrayBuffer, bd->VboHandle);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, bd->ElementsHandle);
            gl.EnableVertexAttribArray(bd->AttribLocationVtxPos);
            gl.EnableVertexAttribArray(bd->AttribLocationVtxUV);
            gl.EnableVertexAttribArray(bd->AttribLocationVtxColor);
            gl.VertexAttribPointer(bd->AttribLocationVtxPos,   2, GLEnum.Float,         false, (uint)sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), nameof(ImDrawVert.pos)));
            gl.VertexAttribPointer(bd->AttribLocationVtxUV,    2, GLEnum.Float,         false, (uint)sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), nameof(ImDrawVert.uv)));
            gl.VertexAttribPointer(bd->AttribLocationVtxColor, 4, GLEnum.UnsignedByte, true, (uint)sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), nameof(ImDrawVert.col)));
        }

        // OpenGL3 Render function.
        // Note that this implementation is little overcomplicated because we are saving/setting up/restoring every OpenGL state explicitly.
        // This is in order to be able to run within an OpenGL engine that doesn't do so.
        public static unsafe void ImGui_ImplOpenGL3_RenderDrawData(ImDrawData* draw_data)
        {
            // Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
            int fb_width = (int)(draw_data->DisplaySize.X * draw_data->FramebufferScale.X);
            int fb_height = (int)(draw_data->DisplaySize.Y * draw_data->FramebufferScale.Y);
            if (fb_width <= 0 || fb_height <= 0)
                return;

            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();

            // Backup GL state
            GLenum last_active_texture; gl.GetInteger(GLEnum.ActiveTexture, (GLint*)&last_active_texture);
            gl.ActiveTexture(GLEnum.Texture0);
            GLuint last_program; gl.GetInteger(GLEnum.CurrentProgram, (GLint*)&last_program);
            GLuint last_texture; gl.GetInteger(GLEnum.TextureBinding2D, (GLint*)&last_texture);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
            GLuint last_sampler; if (bd->GlVersion >= 330) { gl.GetInteger(GLEnum.SamplerBinding, (GLint*)&last_sampler); } else { last_sampler = 0; }
#endif
            GLuint last_array_buffer; gl.GetInteger(GLEnum.ArrayBufferBinding, (GLint*)&last_array_buffer);
#if !IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            // This is part of VAO on OpenGL 3.0+ and OpenGL ES 3.0+.
            GLint last_element_array_buffer; glGetIntegerv(GetPName.ElementArrayBufferBinding, &last_element_array_buffer);
            ImGui_ImplOpenGL3_VtxAttribState last_vtx_attrib_state_pos = default; last_vtx_attrib_state_pos.GetState(bd->AttribLocationVtxPos);
            ImGui_ImplOpenGL3_VtxAttribState last_vtx_attrib_state_uv = default; last_vtx_attrib_state_uv.GetState(bd->AttribLocationVtxUV);
            ImGui_ImplOpenGL3_VtxAttribState last_vtx_attrib_state_color = default; last_vtx_attrib_state_color.GetState(bd->AttribLocationVtxColor);
#endif
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            GLuint last_vertex_array_object; gl.GetInteger(GLEnum.VertexArrayBinding, (GLint*)&last_vertex_array_object);
#endif
#if IMGUI_IMPL_HAS_POLYGON_MODE
            (GLint, GLint) last_polygon_mode; gl.GetInteger(GLEnum.PolygonMode, (int*)&last_polygon_mode);
#endif
            (GLint, GLint, GLint, GLint) last_viewport; gl.GetInteger(GLEnum.Viewport, &last_viewport.Item1);
            (GLint, GLint, GLint, GLint) last_scissor_box; gl.GetInteger(GLEnum.ScissorBox, &last_scissor_box.Item1);
            GLenum last_blend_src_rgb; gl.GetInteger(GLEnum.BlendSrcRgb, (GLint*)&last_blend_src_rgb);
            GLenum last_blend_dst_rgb; gl.GetInteger(GLEnum.BlendDstRgb, (GLint*)&last_blend_dst_rgb);
            GLenum last_blend_src_alpha; gl.GetInteger(GLEnum.BlendSrcAlpha, (GLint*)&last_blend_src_alpha);
            GLenum last_blend_dst_alpha; gl.GetInteger(GLEnum.BlendDstAlpha, (GLint*)&last_blend_dst_alpha);
            GLenum last_blend_equation_rgb; gl.GetInteger(GLEnum.BlendEquationRgb, (GLint*)&last_blend_equation_rgb);
            GLenum last_blend_equation_alpha; gl.GetInteger(GLEnum.BlendEquationAlpha, (GLint*)&last_blend_equation_alpha);
            GLboolean last_enable_blend = gl.IsEnabled(GLEnum.Blend);
            GLboolean last_enable_cull_face = gl.IsEnabled(GLEnum.CullFace);
            GLboolean last_enable_depth_test = gl.IsEnabled(GLEnum.DepthTest);
            GLboolean last_enable_stencil_test = gl.IsEnabled(GLEnum.StencilTest);
            GLboolean last_enable_scissor_test = gl.IsEnabled(GLEnum.ScissorTest);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
            GLboolean last_enable_primitive_restart = (bd->GlVersion >= 310) ? gl.IsEnabled(GLEnum.PrimitiveRestart) : false;
#endif

            // Setup desired GL state
            // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
            // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
            GLuint vertex_array_object = 0;
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            gl.GenVertexArrays(1, &vertex_array_object);
#endif
            ImGui_ImplOpenGL3_SetupRenderState(draw_data, fb_width, fb_height, vertex_array_object);

            // Will project scissor/clipping rectangles into framebuffer space
            ImVec2 clip_off = *(ImVec2*)&draw_data->DisplayPos;         // (0,0) unless using multi-viewports
            ImVec2 clip_scale = *(ImVec2*)&draw_data->FramebufferScale; // (1,1) unless using retina display which are often (2,2)

            // Render command lists
            for (int n = 0; n < draw_data->CmdListsCount; n++)
            {
                ImDrawList* cmd_list = draw_data->CmdLists[n];

                // Upload vertex/index buffers
                // - OpenGL drivers are in a very sorry state nowadays....
                //   During 2021 we attempted to switch from glBufferData() to orphaning+glBufferSubData() following reports
                //   of leaks on Intel GPU when using multi-viewports on Windows.
                // - After this we kept hearing of various display corruptions issues. We started disabling on non-Intel GPU, but issues still got reported on Intel.
                // - We are now back to using exclusively glBufferData(). So bd->UseBufferSubData IS ALWAYS FALSE in this code.
                //   We are keeping the old code path for a while in case people finding new issues may want to test the bd->UseBufferSubData path.
                // - See https://github.com/ocornut/imgui/issues/4468 and please report any corruption issues.
                GLsizeiptr vtx_buffer_size = (GLsizeiptr)(cmd_list->VtxBuffer.Size * (int)sizeof(ImDrawVert));
                GLsizeiptr idx_buffer_size = (GLsizeiptr)(cmd_list->IdxBuffer.Size * (int)sizeof(ImDrawIdx));
                if (bd->UseBufferSubData)
                {
                    if ((ulong)bd->VertexBufferSize < (ulong)vtx_buffer_size)
                    {
                        bd->VertexBufferSize = vtx_buffer_size;
                        gl.BufferData(GLEnum.ArrayBuffer, bd->VertexBufferSize, null, GLEnum.StreamDraw);
                    }
                    if ((ulong)bd->IndexBufferSize < (ulong)idx_buffer_size)
                    {
                        bd->IndexBufferSize = idx_buffer_size;
                        gl.BufferData(GLEnum.ElementArrayBuffer, bd->IndexBufferSize, null, GLEnum.StreamDraw);
                    }
                    gl.BufferSubData(GLEnum.ArrayBuffer, 0, vtx_buffer_size, (void*)cmd_list->VtxBuffer.Data);
                    gl.BufferSubData(GLEnum.ElementArrayBuffer, 0, idx_buffer_size, (void*)cmd_list->IdxBuffer.Data);
                }
                else
                {
                    gl.BufferData(GLEnum.ArrayBuffer, vtx_buffer_size, (void*)cmd_list->VtxBuffer.Data, GLEnum.StreamDraw);
                    gl.BufferData(GLEnum.ElementArrayBuffer, idx_buffer_size, (void*)cmd_list->IdxBuffer.Data, GLEnum.StreamDraw);
                }

                ImDrawCmd* cmd_buffer = (ImDrawCmd*)cmd_list->CmdBuffer.Data;
                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmd* pcmd = &cmd_buffer[cmd_i];
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException(); // laziness

                        //// User callback, registered via ImDrawList::AddCallback()
                        //// (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                        //if (pcmd->UserCallback == ImDrawCallback_ResetRenderState)
                        //    ImGui_ImplOpenGL3_SetupRenderState(draw_data, fb_width, fb_height, vertex_array_object);
                        //else
                        //    pcmd->UserCallback(cmd_list, pcmd);
                    }
                    else
                    {
                        // Project scissor/clipping rectangles into framebuffer space
                        ImVec2 clip_min = new() { x = (pcmd->ClipRect.X - clip_off.x) * clip_scale.x, y = (pcmd->ClipRect.Y - clip_off.y) * clip_scale.y };
                        ImVec2 clip_max = new() { x = (pcmd->ClipRect.Z - clip_off.x) * clip_scale.x, y = (pcmd->ClipRect.W - clip_off.y) * clip_scale.y };
                        if (clip_max.x <= clip_min.x || clip_max.y <= clip_min.y)
                            continue;

                        // Apply scissor/clipping rectangle (Y is inverted in OpenGL)
                        gl.Scissor((int)clip_min.x, (int)((float)fb_height - clip_max.y), (uint)(clip_max.x - clip_min.x), (uint)(clip_max.y - clip_min.y));

                        // Bind texture, Draw
                        gl.BindTexture(GLEnum.Texture2D, (GLuint)pcmd->GetTexID());
#if IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET
                        if (bd->GlVersion >= 320)
                            gl.DrawElementsBaseVertex(GLEnum.Triangles, pcmd->ElemCount, sizeof(ImDrawIdx) == 2 ? GLEnum.UnsignedShort : GLEnum.UnsignedInt, (void*)(pcmd->IdxOffset * sizeof(ImDrawIdx)), (GLint)pcmd->VtxOffset);
                        else
#endif
                        gl.DrawElements(GLEnum.Triangles, pcmd->ElemCount, sizeof(ImDrawIdx) == 2 ? GLEnum.UnsignedShort : GLEnum.UnsignedInt, (void*)(pcmd->IdxOffset * sizeof(ImDrawIdx)));
                    }
                }
            }

            // Destroy the temporary VAO
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            gl.DeleteVertexArrays(1, &vertex_array_object);
#endif

            // Restore modified GL state
            // This "glIsProgram()" check is required because if the program is "pending deletion" at the time of binding backup, it will have been deleted by now and will cause an OpenGL error. See #6220.
            if (last_program == 0 || gl.IsProgram(last_program)) gl.UseProgram(last_program);
            gl.BindTexture(GLEnum.Texture2D, last_texture);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
            if (bd->GlVersion >= 330)
                gl.BindSampler(0, last_sampler);
#endif
            gl.ActiveTexture((GLEnum)last_active_texture);
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            gl.BindVertexArray(last_vertex_array_object);
#endif
            gl.BindBuffer(GLEnum.ArrayBuffer, last_array_buffer);
#if !IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            glBindBuffer(BufferTargetARB.ElementArrayBuffer, (uint)last_element_array_buffer);
            last_vtx_attrib_state_pos.SetState(bd->AttribLocationVtxPos);
            last_vtx_attrib_state_uv.SetState(bd->AttribLocationVtxUV);
            last_vtx_attrib_state_color.SetState(bd->AttribLocationVtxColor);
#endif
            gl.BlendEquationSeparate((GLEnum)last_blend_equation_rgb, (GLEnum)last_blend_equation_alpha);
            gl.BlendFuncSeparate((GLEnum)last_blend_src_rgb, (GLEnum)last_blend_dst_rgb, (GLEnum)last_blend_src_alpha, (GLEnum)last_blend_dst_alpha);
            if (last_enable_blend) gl.Enable(GLEnum.Blend); else gl.Disable(GLEnum.Blend);
            if (last_enable_cull_face) gl.Enable(GLEnum.CullFace); else gl.Disable(GLEnum.CullFace);
            if (last_enable_depth_test) gl.Enable(GLEnum.DepthTest); else gl.Disable(GLEnum.DepthTest);
            if (last_enable_stencil_test) gl.Enable(GLEnum.StencilTest); else gl.Disable(GLEnum.StencilTest);
            if (last_enable_scissor_test) gl.Enable(GLEnum.ScissorTest); else gl.Disable(GLEnum.ScissorTest);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
            if (bd->GlVersion >= 310) { if (last_enable_primitive_restart) gl.Enable(GLEnum.PrimitiveRestart); else gl.Disable(GLEnum.PrimitiveRestart); }
#endif

#if IMGUI_IMPL_HAS_POLYGON_MODE
            // Desktop OpenGL 3.0 and OpenGL 3.1 had separate polygon draw modes for front-facing and back-facing faces of polygons
            if (bd->GlVersion <= 310 || bd->GlProfileIsCompat)
            {
                gl.PolygonMode(GLEnum.Front, ((GLEnum*)&last_polygon_mode)[0]);
                gl.PolygonMode(GLEnum.Back, ((GLEnum*)&last_polygon_mode)[1]);
            }
            else
            {
                gl.PolygonMode(GLEnum.FrontAndBack, ((GLEnum*)&last_polygon_mode)[0]);
            }
#endif // IMGUI_IMPL_HAS_POLYGON_MODE

            gl.Viewport(((GLint*)&last_viewport)[0], ((GLint*)&last_viewport)[1], (uint)((GLint*)&last_viewport)[2], (uint)((GLint*)&last_viewport)[3]);
            gl.Scissor(((GLint*)&last_scissor_box)[0], ((GLint*)&last_scissor_box)[1], (uint)((GLint*)&last_scissor_box)[2], (uint)((GLint*)&last_scissor_box)[3]);
            //(void)bd; // Not all compilation paths use this
        }

        static unsafe bool ImGui_ImplOpenGL3_CreateFontsTexture()
        {
            ImGuiIO* io = igGetIO();
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();

            // Build texture atlas
            byte* pixels;
            int width, height;
            io->Fonts->GetTexDataAsRGBA32(&pixels, &width, &height);   // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders. If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

            // Upload texture to graphics system
            // (Bilinear sampling is required by default. Set 'io.Fonts->Flags |= ImFontAtlasFlags_NoBakedLines' or 'style.AntiAliasedLinesUseTex = false' to allow point/nearest sampling)
            GLint last_texture;
            gl.GetInteger(GLEnum.TextureBinding2D, &last_texture);
            gl.GenTextures(1, &bd->FontTexture);
            gl.BindTexture(GLEnum.Texture2D, bd->FontTexture);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
#if GL_UNPACK_ROW_LENGTH // Not on WebGL/ES
            glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
#endif
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, pixels);

            // Store our identifier
            io->Fonts->SetTexID((ImTextureID)bd->FontTexture);

            // Restore state
            gl.BindTexture(GLEnum.Texture2D, (uint)last_texture);

            return true;
        }

        static unsafe void ImGui_ImplOpenGL3_DestroyFontsTexture()
        {
            ImGuiIO* io = igGetIO();
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();
            if (bd->FontTexture != 0)
            {
                gl.DeleteTextures(1, &bd->FontTexture);
                io->Fonts->SetTexID(IntPtr.Zero);
                bd->FontTexture = 0;
            }
        }

        // If you get an error please report on github. You may try different GL context version or GLSL version. See GL<>GLSL version table at the top of this file.
        static unsafe bool CheckShader(GLuint handle, string desc)
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();
            GLint status = 0, log_length = 0;
            gl.GetShader(handle, GLEnum.CompileStatus, &status);
            gl.GetShader(handle, GLEnum.InfoLogLength, &log_length);
            if (status == 0)
                Console.Error.Write("ERROR: ImGui_ImplOpenGL3_CreateDeviceObjects: failed to compile {0}! With GLSL: {1}\n", desc, new string(bd->GlslVersionString));
            if (log_length > 1)
            {
                Console.Error.Write("{0}\n", gl.GetShaderInfoLog(handle));
            }
            return status != 0;
        }

        // If you get an error please report on GitHub. You may try different GL context version or GLSL version.
        static unsafe bool CheckProgram(GLuint handle, string desc)
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();
            GLint status = 0, log_length = 0;
            gl.GetProgram(handle, GLEnum.LinkStatus, &status);
            gl.GetProgram(handle, GLEnum.InfoLogLength, &log_length);
            if (status == 0)
                Console.Error.Write("ERROR: ImGui_ImplOpenGL3_CreateDeviceObjects: failed to link {0}! With GLSL {1}\n", desc, new string(bd->GlslVersionString));
            if (log_length > 1)
            {
                Console.Error.Write("{0}\n", gl.GetShaderInfoLog(handle));
            }
            return status != 0;
        }

        static unsafe bool ImGui_ImplOpenGL3_CreateDeviceObjects()
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();

            // Backup GL state
            GLint last_texture, last_array_buffer;
            gl.GetInteger(GLEnum.TextureBinding2D, &last_texture);
            gl.GetInteger(GLEnum.ArrayBufferBinding, &last_array_buffer);
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            GLint last_vertex_array;
            gl.GetInteger(GLEnum.VertexArrayBinding, &last_vertex_array);
#endif

            // Parse GLSL version string
            int glsl_version = 130;
            // kanón na vrabce
            glsl_version = int.Parse(Regex.Match(new string(bd->GlslVersionString), @"^#version (\d+)").Groups[1].Value);

            const string vertex_shader_glsl_120 =
                "uniform mat4 ProjMtx;\n" +
                "attribute vec2 Position;\n" +
                "attribute vec2 UV;\n" +
                "attribute vec4 Color;\n" +
                "varying vec2 Frag_UV;\n" +
                "varying vec4 Frag_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Frag_UV = UV;\n" +
                "    Frag_Color = Color;\n" +
                "    gl_Position = ProjMtx * vec4(Position.xy,0,1);\n" +
                "}\n";

            const string vertex_shader_glsl_130 =
                "uniform mat4 ProjMtx;\n" +
                "in vec2 Position;\n" +
                "in vec2 UV;\n" +
                "in vec4 Color;\n" +
                "out vec2 Frag_UV;\n" +
                "out vec4 Frag_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Frag_UV = UV;\n" +
                "    Frag_Color = Color;\n" +
                "    gl_Position = ProjMtx * vec4(Position.xy,0,1);\n" +
                "}\n";

            const string vertex_shader_glsl_300_es =
                "precision highp float;\n" +
                "layout (location = 0) in vec2 Position;\n" +
                "layout (location = 1) in vec2 UV;\n" +
                "layout (location = 2) in vec4 Color;\n" +
                "uniform mat4 ProjMtx;\n" +
                "out vec2 Frag_UV;\n" +
                "out vec4 Frag_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Frag_UV = UV;\n" +
                "    Frag_Color = Color;\n" +
                "    gl_Position = ProjMtx * vec4(Position.xy,0,1);\n" +
                "}\n";

            const string vertex_shader_glsl_410_core =
                "layout (location = 0) in vec2 Position;\n" +
                "layout (location = 1) in vec2 UV;\n" +
                "layout (location = 2) in vec4 Color;\n" +
                "uniform mat4 ProjMtx;\n" +
                "out vec2 Frag_UV;\n" +
                "out vec4 Frag_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Frag_UV = UV;\n" +
                "    Frag_Color = Color;\n" +
                "    gl_Position = ProjMtx * vec4(Position.xy,0,1);\n" +
                "}\n";

            const string fragment_shader_glsl_120 =
                "#ifdef GL_ES\n" +
                "    precision mediump float;\n" +
                "#endif\n" +
                "uniform sampler2D Texture;\n" +
                "varying vec2 Frag_UV;\n" +
                "varying vec4 Frag_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    gl_FragColor = Frag_Color * texture2D(Texture, Frag_UV.st);\n" +
                "}\n";

            const string fragment_shader_glsl_130 =
                "uniform sampler2D Texture;\n" +
                "in vec2 Frag_UV;\n" +
                "in vec4 Frag_Color;\n" +
                "out vec4 Out_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" +
                "}\n";

            const string fragment_shader_glsl_300_es =
                "precision mediump float;\n" +
                "uniform sampler2D Texture;\n" +
                "in vec2 Frag_UV;\n" +
                "in vec4 Frag_Color;\n" +
                "layout (location = 0) out vec4 Out_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" +
                "}\n";

            const string fragment_shader_glsl_410_core =
                "in vec2 Frag_UV;\n" +
                "in vec4 Frag_Color;\n" +
                "uniform sampler2D Texture;\n" +
                "layout (location = 0) out vec4 Out_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" +
                "}\n";

            // Select shaders matching our GLSL versions
            string vertex_shader = null;
            string fragment_shader = null;
            if (glsl_version < 130)
            {
                vertex_shader = vertex_shader_glsl_120;
                fragment_shader = fragment_shader_glsl_120;
            }
            else if (glsl_version >= 410)
            {
                vertex_shader = vertex_shader_glsl_410_core;
                fragment_shader = fragment_shader_glsl_410_core;
            }
            else if (glsl_version == 300)
            {
                vertex_shader = vertex_shader_glsl_300_es;
                fragment_shader = fragment_shader_glsl_300_es;
            }
            else
            {
                vertex_shader = vertex_shader_glsl_130;
                fragment_shader = fragment_shader_glsl_130;
            }

            // Create shaders
            string vertex_shader_with_version = new string(bd->GlslVersionString) + vertex_shader;
            GLuint vert_handle = gl.CreateShader(GLEnum.VertexShader);
            gl.ShaderSource(vert_handle, vertex_shader_with_version);
            gl.CompileShader(vert_handle);
            CheckShader(vert_handle, "vertex shader");

            string fragment_shader_with_version = new string(bd->GlslVersionString) + fragment_shader;
            GLuint frag_handle = gl.CreateShader(GLEnum.FragmentShader);
            gl.ShaderSource(frag_handle, fragment_shader_with_version);
            gl.CompileShader(frag_handle);
            CheckShader(frag_handle, "fragment shader");

            // Link
            bd->ShaderHandle = gl.CreateProgram();
            gl.AttachShader(bd->ShaderHandle, vert_handle);
            gl.AttachShader(bd->ShaderHandle, frag_handle);
            gl.LinkProgram(bd->ShaderHandle);
            CheckProgram(bd->ShaderHandle, "shader program");

            gl.DetachShader(bd->ShaderHandle, vert_handle);
            gl.DetachShader(bd->ShaderHandle, frag_handle);
            gl.DeleteShader(vert_handle);
            gl.DeleteShader(frag_handle);

            bd->AttribLocationTex = gl.GetUniformLocation(bd->ShaderHandle, "Texture");
            bd->AttribLocationProjMtx = gl.GetUniformLocation(bd->ShaderHandle, "ProjMtx");
            bd->AttribLocationVtxPos = (GLuint)gl.GetAttribLocation(bd->ShaderHandle, "Position");
            bd->AttribLocationVtxUV = (GLuint)gl.GetAttribLocation(bd->ShaderHandle, "UV");
            bd->AttribLocationVtxColor = (GLuint)gl.GetAttribLocation(bd->ShaderHandle, "Color");

            // Create buffers
            gl.GenBuffers(1, &bd->VboHandle);
            gl.GenBuffers(1, &bd->ElementsHandle);

            ImGui_ImplOpenGL3_CreateFontsTexture();

            // Restore modified GL state
            gl.BindTexture(GLEnum.Texture2D, (uint)last_texture);
            gl.BindBuffer(GLEnum.ArrayBuffer, (uint)last_array_buffer);
#if IMGUI_IMPL_OPENGL_USE_VERTEX_ARRAY
            gl.BindVertexArray((uint)last_vertex_array);
#endif

            return true;
        }

        static unsafe void ImGui_ImplOpenGL3_DestroyDeviceObjects()
        {
            ImGui_ImplOpenGL3_Data* bd = ImGui_ImplOpenGL3_GetBackendData();
            if (bd->VboHandle != 0)      { gl.DeleteBuffers(1, &bd->VboHandle); bd->VboHandle = 0; }
            if (bd->ElementsHandle != 0) { gl.DeleteBuffers(1, &bd->ElementsHandle); bd->ElementsHandle = 0; }
            if (bd->ShaderHandle != 0)   { gl.DeleteProgram(bd->ShaderHandle); bd->ShaderHandle = 0; }
            ImGui_ImplOpenGL3_DestroyFontsTexture();
        }
    }
}

//#if defined(__GNUC__)
//#pragma GCC diagnostic pop
//#endif
//#if defined(__clang__)
//#pragma clang diagnostic pop
//#endif