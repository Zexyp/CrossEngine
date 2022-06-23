// Desktop GL 3.2+ has glDrawElementsBaseVertex() which GL ES and WebGL don't have.
#define IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET

// Desktop GL 3.3+ has glBindSampler()
#define IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER

// Desktop GL 3.1+ has GL_PRIMITIVE_RESTART state
#define IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART

// Desktop GL use extension detection
#define IMGUI_IMPL_OPENGL_MAY_HAVE_EXTENSIONS

using System;
using GLFW;
using ImGuiNET;
using static OpenGL.GL;

using System.Numerics;
using System.Runtime.InteropServices;

namespace CrossEngine.Utils
{
    using ImDrawIdx = UInt16;
    public class ImGuiController
    {
        #region GLFW
        static Window windowHandle;

        static Cursor[] mouseCursors = new Cursor[(int)ImGuiMouseCursor.COUNT];

        static MouseButtonCallback prevUserCallbackMousebutton;
        static MouseCallback prevUserCallbackScroll;
        static KeyCallback prevUserCallbackKey;
        static CharCallback prevUserCallbackChar;

        static MouseButtonCallback mousebuttonCallbackHolder;
        static MouseCallback scrollCallbackHolder;
        static KeyCallback keyCallbackHolder;
        static CharCallback charCallbackHolder;

        static bool installedCallbacks;

        static bool[] mouseJustPressed = new bool[(int)ImGuiMouseButton.COUNT];

        static double time;



        public static bool ImGui_ImplGlfw_InitForOpenGL(Window window, bool installCallbacks)
        {
            return ImGui_ImplGlfw_Init(window, installCallbacks);
        }

        static bool ImGui_ImplGlfw_Init(Window window, bool installCallbacks)
        {
            windowHandle = window;
            time = 0.0f;

            ImGuiIOPtr io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
            //io.BackendPlatformName = "imgui_impl_glfw";

            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Insert] = (int)Keys.Insert;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.KeyPadEnter] = (int)Keys.NumpadEnter;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;

//            io.SetClipboardTextFn = ImGui_ImplGlfw_SetClipboardText;
//            io.GetClipboardTextFn = ImGui_ImplGlfw_GetClipboardText;
//            io.ClipboardUserData = g_Window;
//#if defined(_WIN32)
//            io.ImeWindowHandle = (void*)glfwGetWin32Window(g_Window);
//#endif

            ErrorCallback prev_error_callback = Glfw.SetErrorCallback(null);

            mouseCursors[(int)ImGuiMouseCursor.Arrow] = Glfw.CreateStandardCursor(CursorType.Arrow);
            mouseCursors[(int)ImGuiMouseCursor.TextInput] = Glfw.CreateStandardCursor(CursorType.Beam);
            mouseCursors[(int)ImGuiMouseCursor.ResizeNS] = Glfw.CreateStandardCursor(CursorType.ResizeVertical);
            mouseCursors[(int)ImGuiMouseCursor.ResizeEW] = Glfw.CreateStandardCursor(CursorType.ResizeHorizontal);
            mouseCursors[(int)ImGuiMouseCursor.Hand] = Glfw.CreateStandardCursor(CursorType.Hand);

            // if GLFW_HAS_NEW_CURSORS
            //mouseCursors[(int)ImGuiMouseCursor.ResizeAll] = GLFW.Glfw.CreateStandardCursor(GLFW.CursorType. GLFW_RESIZE_ALL_CURSOR);
            //mouseCursors[(int)ImGuiMouseCursor.ResizeNESW] = GLFW.Glfw.CreateStandardCursor(GLFW.CursorType. GLFW_RESIZE_NESW_CURSOR);
            //mouseCursors[(int)ImGuiMouseCursor.ResizeNWSE] = GLFW.Glfw.CreateStandardCursor(GLFW.CursorType. GLFW_RESIZE_NWSE_CURSOR);
            //mouseCursors[(int)ImGuiMouseCursor.NotAllowed] = GLFW.Glfw.CreateStandardCursor(GLFW.CursorType. GLFW_NOT_ALLOWED_CURSOR);

            mouseCursors[(int)ImGuiMouseCursor.ResizeAll] = Glfw.CreateStandardCursor(CursorType.Arrow);
            mouseCursors[(int)ImGuiMouseCursor.ResizeNESW] = Glfw.CreateStandardCursor(CursorType.Arrow);
            mouseCursors[(int)ImGuiMouseCursor.ResizeNWSE] = Glfw.CreateStandardCursor(CursorType.Arrow);
            mouseCursors[(int)ImGuiMouseCursor.NotAllowed] = Glfw.CreateStandardCursor(CursorType.Arrow);

            Glfw.SetErrorCallback(prev_error_callback);

            prevUserCallbackMousebutton = null;
            prevUserCallbackScroll = null;
            prevUserCallbackKey = null;
            prevUserCallbackChar = null;
            if (installCallbacks)
            {
                installedCallbacks = true;
                prevUserCallbackMousebutton = Glfw.SetMouseButtonCallback(windowHandle, mousebuttonCallbackHolder = ImGui_ImplGlfw_MouseButtonCallback);
                prevUserCallbackScroll = Glfw.SetScrollCallback(windowHandle, scrollCallbackHolder = ImGui_ImplGlfw_ScrollCallback);
                prevUserCallbackKey = Glfw.SetKeyCallback(windowHandle, keyCallbackHolder = ImGui_ImplGlfw_KeyCallback);
                prevUserCallbackChar = Glfw.SetCharCallback(windowHandle, charCallbackHolder = ImGui_ImplGlfw_CharCallback);
            }

            return true;
        }

        static public void ImGui_ImplGlfw_Shutdown()
        {
            if (installedCallbacks)
            {
                Glfw.SetMouseButtonCallback(windowHandle, prevUserCallbackMousebutton);
                Glfw.SetScrollCallback(windowHandle, prevUserCallbackScroll);
                Glfw.SetKeyCallback(windowHandle, prevUserCallbackKey);
                Glfw.SetCharCallback(windowHandle, prevUserCallbackChar);
                installedCallbacks = false;
            }

            for (ImGuiMouseCursor cursor_n = 0; cursor_n < ImGuiMouseCursor.COUNT; cursor_n++)
            {
                Glfw.DestroyCursor(mouseCursors[(int)cursor_n]);
                mouseCursors[(int)cursor_n] = new Cursor(); // idk if this clearing is nessesary
            }
        }

        static void ImGui_ImplGlfw_UpdateMousePosAndButtons()
        {
            // Update buttons
            ImGuiIOPtr io = ImGui.GetIO();
            for (int i = 0; i < io.MouseDown.Count; i++)
            {
                // If a mouse press event came, always pass it as "mouse held this frame", so we don't miss click-release events that are shorter than 1 frame.
                io.MouseDown[i] = mouseJustPressed[i] || Glfw.GetMouseButton(windowHandle, (MouseButton)i) != 0;
                mouseJustPressed[i] = false;
            }

            // Update mouse position
            Vector2 mouse_pos_backup = io.MousePos;
            io.MousePos = new Vector2(-float.MaxValue, -float.MaxValue);

            //const bool focused = true; // Emscripten

            bool focused = Glfw.GetWindowAttribute(windowHandle, WindowAttribute.Focused);

            if (focused)
            {
                if (io.WantSetMousePos)
                {
                    Glfw.SetCursorPosition(windowHandle, (double)mouse_pos_backup.X, (double)mouse_pos_backup.Y);
                }
                else
                {
                    double mouse_x, mouse_y;
                    Glfw.GetCursorPosition(windowHandle, out mouse_x, out mouse_y);
                    io.MousePos = new Vector2((float)mouse_x, (float)mouse_y);
                }
            }
        }

        static void ImGui_ImplGlfw_UpdateMouseCursor()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) > 0 || Glfw.GetInputMode(windowHandle, InputMode.Cursor) == (int)CursorMode.Disabled)
                return;

            ImGuiMouseCursor imgui_cursor = ImGui.GetMouseCursor();
            if (imgui_cursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
            {
                // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                Glfw.SetInputMode(windowHandle, InputMode.Cursor, (int)CursorMode.Hidden);
            }
            else
            {
                // Show OS mouse cursor
                // FIXME-PLATFORM: Unfocused windows seems to fail changing the mouse cursor with GLFW 3.2, but 3.3 works here.
                Glfw.SetCursor(windowHandle, mouseCursors[(int)imgui_cursor] != Cursor.None ? mouseCursors[(int)imgui_cursor] : mouseCursors[(int)ImGuiMouseCursor.Arrow]);
                Glfw.SetInputMode(windowHandle, InputMode.Cursor, (int)CursorMode.Normal);
            }
        }

        public static void ImGui_ImplGlfw_NewFrame()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            System.Diagnostics.Debug.Assert(io.Fonts.IsBuilt() && "Font atlas not built! It is generally built by the renderer backend. Missing call to renderer _NewFrame() function? e.g. ImGui_ImplOpenGL3_NewFrame()." != null);

            // Setup display size (every frame to accommodate for window resizing)
            int w, h;
            int display_w, display_h;
            Glfw.GetWindowSize(windowHandle, out w, out h);
            Glfw.GetFramebufferSize(windowHandle, out display_w, out display_h);
            io.DisplaySize = new Vector2((float)w, (float)h);
            if (w > 0 && h > 0)
                io.DisplayFramebufferScale = new Vector2((float)display_w / w, (float)display_h / h);

            // Setup time step
            double current_time = Glfw.Time;
            io.DeltaTime = time > 0.0 ? (float)(current_time - time) : (float)(1.0f / 60.0f);
            time = current_time;

            ImGui_ImplGlfw_UpdateMousePosAndButtons();
            ImGui_ImplGlfw_UpdateMouseCursor();

            // Update game controllers (if enabled and available)
            //ImGui_ImplGlfw_UpdateGamepads();
        }

        #region Callbacks
        private static void ImGui_ImplGlfw_MouseButtonCallback(Window window, MouseButton button, InputState state, ModifierKeys modifiers)
        {
            prevUserCallbackMousebutton?.Invoke(window, button, state, modifiers);

            if (state == InputState.Press && button >= 0 && (int)button < mouseJustPressed.Length)
                mouseJustPressed[(int)button] = true;
        }

        private static void ImGui_ImplGlfw_ScrollCallback(Window window, double x, double y)
        {
            prevUserCallbackScroll?.Invoke(window, x, y);

            ImGuiIOPtr io = ImGui.GetIO();
            io.MouseWheelH += (float)x;
            io.MouseWheel += (float)y;
        }

        private static void ImGui_ImplGlfw_KeyCallback(Window window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            prevUserCallbackKey?.Invoke(window, key, scanCode, state, mods);

            ImGuiIOPtr io = ImGui.GetIO();
            if (key >= 0 && (int)key < io.KeysDown.Count)
            {
                if (state == InputState.Press)
                    io.KeysDown[(int)key] = true;
                if (state == InputState.Release)
                    io.KeysDown[(int)key] = false;
            }

            // Modifiers are not reliable across systems
            io.KeyCtrl = io.KeysDown[(int)Keys.LeftControl] || io.KeysDown[(int)Keys.RightControl];
            io.KeyShift = io.KeysDown[(int)Keys.LeftShift] || io.KeysDown[(int)Keys.RightShift];
            io.KeyAlt = io.KeysDown[(int)Keys.LeftAlt] || io.KeysDown[(int)Keys.RightAlt];

            //io.KeySuper = false;

            io.KeySuper = io.KeysDown[(int)Keys.LeftSuper] || io.KeysDown[(int)Keys.RightSuper];
        }

        private static void ImGui_ImplGlfw_CharCallback(Window window, uint codePoint)
        {
            prevUserCallbackChar?.Invoke(window, codePoint);

            ImGuiIOPtr io = ImGui.GetIO();
            io.AddInputCharacter(codePoint);
        }
        #endregion
        #endregion

        #region OpenGL3
        static uint glVersion = 0;
        static string glslVersionString = new string(' ', 32);
        //static bool hasClipOrigin;

        static uint shaderHandle;
        static int attribLocationTex;
        static int attribLocationProjMtx;
        static uint vboHandle;
        static uint elementsHandle;
        static uint attribLocationVtxPos;
        static uint attribLocationVtxUV;
        static uint attribLocationVtxColor;
        static uint vertHandle;
        static uint fragHandle;
        static uint fontTexture;

        public static unsafe bool ImGui_ImplOpenGL3_Init(string glsl_version = null)
        {
            #region GL Version
            // desktop OpenGL
            int major = 0;
            int minor = 0;
            glGetIntegerv(GL_MAJOR_VERSION, &major);
            glGetIntegerv(GL_MINOR_VERSION, &minor);
            if (major == 0 && minor == 0)
            {
                // Query GL_VERSION in desktop GL 2.x, the string will start with "<major>.<minor>"
                string versionStr = glGetString(GL_VERSION);
                string[] parts = versionStr.Split('.');
                major = Convert.ToInt32(parts[0]);
                minor = Convert.ToInt32(parts[1]);
            }
            glVersion = (uint)(major * 100 + minor * 10);

            // GLES 2
            //glVersion = 200;
        
            // Setup backend capabilities flags
            ImGuiIOPtr io = ImGui.GetIO();
            //io.BackendRendererName = "imgui_impl_opengl3";
#if IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET
            if (glVersion >= 320)
                io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;  // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.
#endif
            #endregion

            #region GLSL Version
            if (glsl_version == null)
                glsl_version = "#version 130 core"; // core profile needs to be specified

            //System.Diagnostics.Debug.Assert(glsl_version.Length + 2 < glslVersionString.Length);
            glslVersionString = glsl_version;
            glslVersionString += "\n";
            #endregion


            // Make an arbitrary GL call (we don't actually need the result)
            // IF YOU GET A CRASH HERE: it probably means that you haven't initialized the OpenGL function loader used by this code.
            // Desktop OpenGL 3/4 need a function loader. See the IMGUI_IMPL_OPENGL_LOADER_xxx explanation above.
            int current_texture;
            glGetIntegerv(GL_TEXTURE_BINDING_2D, &current_texture);
            

            // Detect extensions we support
            //hasClipOrigin = (glVersion >= 450);
//#if IMGUI_IMPL_OPENGL_MAY_HAVE_EXTENSIONS
            //int num_extensions = 0;
            //glGetIntegerv(GL_NUM_EXTENSIONS, &num_extensions);
            //for (uint i = 0; i < num_extensions; i++)
            //{
            //    string extension = glGetStringi(GL_EXTENSIONS, i);
            //    if (extension.Contains("GL_ARB_clip_control"))
            //        hasClipOrigin = true;
            //}
//#endif
        
            return true;
        }

        public static void ImGui_ImplOpenGL3_Shutdown()
        {
            ImGui_ImplOpenGL3_DestroyDeviceObjects();
        }

        public static void ImGui_ImplOpenGL3_NewFrame()
        {
            if (shaderHandle == 0)
                ImGui_ImplOpenGL3_CreateDeviceObjects();
        }

        static unsafe void ImGui_ImplOpenGL3_SetupRenderState(ImDrawDataPtr draw_data, int fb_width, int fb_height, uint vertex_array_object)
        {
            #region Enables/Disables
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, polygon fill
            glEnable(GL_BLEND);
            glBlendEquation(GL_FUNC_ADD);
            glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
            glDisable(GL_CULL_FACE);
            glDisable(GL_DEPTH_TEST);
            glDisable(GL_STENCIL_TEST);
            glEnable(GL_SCISSOR_TEST);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
            if (glVersion >= 310)
                glDisable(GL_PRIMITIVE_RESTART);
#endif
            #endregion

            // lol let's sometime later draw it with wireframe
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

            // Support for GL 4.5 rarely used glClipControl(GL_UPPER_LEFT)
//#if defined(GL_CLIP_ORIGIN)
            //bool clip_origin_lower_left = true;
            //if (hasClipOrigin)
            //{
            //    int current_clip_origin = 0; glGetIntegerv(GL_CLIP_ORIGIN, (GLint*)&current_clip_origin);
            //    if (current_clip_origin == GL_UPPER_LEFT)
            //        clip_origin_lower_left = false;
            //}
//#endif

            #region View
            // Setup viewport, orthographic projection matrix
            // Our visible imgui space lies from draw_data->DisplayPos (top left) to draw_data->DisplayPos+data_data->DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
            glViewport(0, 0, fb_width, fb_height);
            float L = draw_data.DisplayPos.X;
            float R = draw_data.DisplayPos.X + draw_data.DisplaySize.X;
            float T = draw_data.DisplayPos.Y;
            float B = draw_data.DisplayPos.Y + draw_data.DisplaySize.Y;
//#if defined(GL_CLIP_ORIGIN)
            //if (!clip_origin_lower_left) { float tmp = T; T = B; B = tmp; } // Swap top and bottom if origin is upper left
//#endif
            float[,] ortho_projection = new float[4, 4]
            {
                { 2.0f / (R - L),    0.0f,              0.0f, 0.0f },
                { 0.0f,              2.0f / (T - B),    0.0f, 0.0f },
                { 0.0f,              0.0f,             -1.0f, 0.0f },
                { (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f },
            };

            glUseProgram(shaderHandle);
            glUniform1i(attribLocationTex, 0);
            fixed (float* p = &ortho_projection[0, 0])
                glUniformMatrix4fv(attribLocationProjMtx, 1, false, p);
            #endregion

#if IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
            if (glVersion >= 330)
                glBindSampler(0, 0); // We use combined texture/sampler state. Applications using GL 3.3 may set that otherwise.
#endif

//#ifndef IMGUI_IMPL_OPENGL_ES2
            glBindVertexArray(vertex_array_object);
//#endif
            // Bind vertex/index buffers and setup attributes for ImDrawVert
            glBindBuffer(GL_ARRAY_BUFFER, vboHandle);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, elementsHandle);
            glEnableVertexAttribArray(attribLocationVtxPos);
            glEnableVertexAttribArray(attribLocationVtxUV);
            glEnableVertexAttribArray(attribLocationVtxColor);
            glVertexAttribPointer(attribLocationVtxPos, 2, GL_FLOAT, false, sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), "pos"));
            glVertexAttribPointer(attribLocationVtxUV, 2, GL_FLOAT, false, sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), "uv"));
            glVertexAttribPointer(attribLocationVtxColor, 4, GL_UNSIGNED_BYTE, true, sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), "col"));
            //glEnableVertexAttribArray(0);
            //glEnableVertexAttribArray(1);
            //glEnableVertexAttribArray(2);
            //glVertexAttribPointer(0, 2, GL_FLOAT, false, sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), "pos"));
            //glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), "uv"));
            //glVertexAttribPointer(2, 4, GL_UNSIGNED_BYTE, true, sizeof(ImDrawVert), (void*)Marshal.OffsetOf(typeof(ImDrawVert), "col"));
        }

        public static unsafe void ImGui_ImplOpenGL3_RenderDrawData(ImDrawDataPtr draw_data)
        {
            // Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
            int fb_width = (int)(draw_data.DisplaySize.X * draw_data.FramebufferScale.X);
            int fb_height = (int)(draw_data.DisplaySize.Y * draw_data.FramebufferScale.Y);
            if (fb_width <= 0 || fb_height <= 0)
                return;

            #region Backup State
            // Backup GL state
            int last_active_texture; glGetIntegerv(GL_ACTIVE_TEXTURE, &last_active_texture);
            glActiveTexture(GL_TEXTURE0);

            uint last_program; glGetIntegerv(GL_CURRENT_PROGRAM, (int*)&last_program);
            uint last_texture; glGetIntegerv(GL_TEXTURE_BINDING_2D, (int*)&last_texture);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
            uint last_sampler; if (glVersion >= 330) { glGetIntegerv(GL_SAMPLER_BINDING, (int*)&last_sampler); } else { last_sampler = 0; }
#endif
            uint last_array_buffer; glGetIntegerv(GL_ARRAY_BUFFER_BINDING, (int*)&last_array_buffer);
//#ifndef IMGUI_IMPL_OPENGL_ES2
            uint last_vertex_array_object; glGetIntegerv(GL_VERTEX_ARRAY_BINDING, (int*)&last_vertex_array_object);
//#endif

            int[] last_polygon_mode = new int[2]; 
            fixed(int* p = &last_polygon_mode[0])
                glGetIntegerv(GL_POLYGON_MODE, p);

            int[] last_viewport = new int[4];
            fixed (int* p = &last_viewport[0])
                glGetIntegerv(GL_VIEWPORT, p);
            int[] last_scissor_box = new int[4];
            fixed (int* p = &last_scissor_box[0])
                glGetIntegerv(GL_SCISSOR_BOX, p);
            int last_blend_src_rgb; glGetIntegerv(GL_BLEND_SRC_RGB, &last_blend_src_rgb);
            int last_blend_dst_rgb; glGetIntegerv(GL_BLEND_DST_RGB, &last_blend_dst_rgb);
            int last_blend_src_alpha; glGetIntegerv(GL_BLEND_SRC_ALPHA, &last_blend_src_alpha);
            int last_blend_dst_alpha; glGetIntegerv(GL_BLEND_DST_ALPHA, &last_blend_dst_alpha);
            int last_blend_equation_rgb; glGetIntegerv(GL_BLEND_EQUATION_RGB, &last_blend_equation_rgb);
            int last_blend_equation_alpha; glGetIntegerv(GL_BLEND_EQUATION_ALPHA, &last_blend_equation_alpha);
            bool last_enable_blend = glIsEnabled(GL_BLEND);
            bool last_enable_cull_face = glIsEnabled(GL_CULL_FACE);
            bool last_enable_depth_test = glIsEnabled(GL_DEPTH_TEST);
            bool last_enable_stencil_test = glIsEnabled(GL_STENCIL_TEST);
            bool last_enable_scissor_test = glIsEnabled(GL_SCISSOR_TEST);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
            bool last_enable_primitive_restart = (glVersion >= 310) ? glIsEnabled(GL_PRIMITIVE_RESTART) : false;
#endif
            #endregion

            // Setup desired GL state
            // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
            // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
            uint vertex_array_object = 0;

            glGenVertexArrays(1, &vertex_array_object);

            ImGui_ImplOpenGL3_SetupRenderState(draw_data, fb_width, fb_height, vertex_array_object);

            // Will project scissor/clipping rectangles into framebuffer space
            Vector2 clip_off = draw_data.DisplayPos;         // (0,0) unless using multi-viewports
            Vector2 clip_scale = draw_data.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                // Upload vertex/index buffers
                glBufferData(GL_ARRAY_BUFFER, cmd_list.VtxBuffer.Size * (int)sizeof(ImDrawVert), cmd_list.VtxBuffer.Data, GL_STREAM_DRAW);
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, cmd_list.IdxBuffer.Size * (int)sizeof(ImDrawIdx), cmd_list.IdxBuffer.Data, GL_STREAM_DRAW);

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    //if (pcmd.UserCallback != IntPtr.Zero)
                    //{
                    //    // User callback, registered via ImDrawList::AddCallback()
                    //    // (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                    //    if (pcmd.UserCallback == ImDrawCallback.ResetRenderState)
                    //        ImGui_ImplOpenGL3_SetupRenderState(draw_data, fb_width, fb_height, vertex_array_object);
                    //    else
                    //        pcmd.UserCallback(cmd_list, pcmd);
                    //}
                    //else
                    //{
                    // Project scissor/clipping rectangles into framebuffer space
                    Vector4 clip_rect;
                    clip_rect.X = (pcmd.ClipRect.X - clip_off.X) * clip_scale.X;
                    clip_rect.Y = (pcmd.ClipRect.Y - clip_off.Y) * clip_scale.Y;
                    clip_rect.Z = (pcmd.ClipRect.Z - clip_off.X) * clip_scale.X;
                    clip_rect.W = (pcmd.ClipRect.W - clip_off.Y) * clip_scale.Y;

                    if (clip_rect.X < fb_width && clip_rect.Y < fb_height && clip_rect.Z >= 0.0f && clip_rect.W >= 0.0f)
                    {
                        // Apply scissor/clipping rectangle
                        glScissor((int)clip_rect.X, (int)(fb_height - clip_rect.W), (int)(clip_rect.Z - clip_rect.X), (int)(clip_rect.W - clip_rect.Y));

                        // Bind texture, Draw
                        glBindTexture(GL_TEXTURE_2D, (uint)pcmd.TextureId.ToPointer());
#if IMGUI_IMPL_OPENGL_MAY_HAVE_VTX_OFFSET
                        if (glVersion >= 320)
                            glDrawElementsBaseVertex(GL_TRIANGLES, (int)pcmd.ElemCount, sizeof(ImDrawIdx) == 2 ? GL_UNSIGNED_SHORT : GL_UNSIGNED_INT, (void*)(pcmd.IdxOffset * sizeof(ImDrawIdx)), (int)pcmd.VtxOffset);
                        else
#endif
                            glDrawElements(GL_TRIANGLES, (int)pcmd.ElemCount, sizeof(ImDrawIdx) == 2 ? GL_UNSIGNED_SHORT : GL_UNSIGNED_INT, (void*)(pcmd.IdxOffset * sizeof(ImDrawIdx)));
                    }
                    //}
                }
            }

            // Destroy the temporary VAO
//#ifndef IMGUI_IMPL_OPENGL_ES2
            glDeleteVertexArrays(1, &vertex_array_object);
//#endif

            #region Backup Restore
            // Restore modified GL state
            glUseProgram(last_program);
            glBindTexture(GL_TEXTURE_2D, last_texture);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_BIND_SAMPLER
            if (glVersion >= 330)
                glBindSampler(0, last_sampler);
#endif
            glActiveTexture(last_active_texture);

            glBindVertexArray(last_vertex_array_object);

            glBindBuffer(GL_ARRAY_BUFFER, last_array_buffer);
            glBlendEquationSeparate(last_blend_equation_rgb, last_blend_equation_alpha);
            glBlendFuncSeparate(last_blend_src_rgb, last_blend_dst_rgb, last_blend_src_alpha, last_blend_dst_alpha);
            if (last_enable_blend) glEnable(GL_BLEND); else glDisable(GL_BLEND);
            if (last_enable_cull_face) glEnable(GL_CULL_FACE); else glDisable(GL_CULL_FACE);
            if (last_enable_depth_test) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST);
            if (last_enable_stencil_test) glEnable(GL_STENCIL_TEST); else glDisable(GL_STENCIL_TEST);
            if (last_enable_scissor_test) glEnable(GL_SCISSOR_TEST); else glDisable(GL_SCISSOR_TEST);
#if IMGUI_IMPL_OPENGL_MAY_HAVE_PRIMITIVE_RESTART
            if (glVersion >= 310)
            {
                if (last_enable_primitive_restart) glEnable(GL_PRIMITIVE_RESTART); else glDisable(GL_PRIMITIVE_RESTART);
            }
#endif

            glPolygonMode(GL_FRONT_AND_BACK, last_polygon_mode[0]);

            glViewport(last_viewport[0], last_viewport[1], last_viewport[2], last_viewport[3]);
            glScissor(last_scissor_box[0], last_scissor_box[1], last_scissor_box[2], last_scissor_box[3]);
            #endregion
        }

        static unsafe bool ImGui_ImplOpenGL3_CreateFontsTexture()
        {
            // Build texture atlas
            ImGuiIOPtr io = ImGui.GetIO();
            byte* pixels;
            int width, height;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height);   // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders. If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

            // Upload texture to graphics system

            // backup
            uint last_texture;
            glGetIntegerv(GL_TEXTURE_BINDING_2D, (int*)&last_texture);

            fixed (uint* p = &fontTexture)
                glGenTextures(1, p);
            glBindTexture(GL_TEXTURE_2D, fontTexture);
            // filtering needs to be set!
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

//#ifdef GL_UNPACK_ROW_LENGTH
            glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
//#endif
            // using matching data format with requsted data format
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, pixels);

            // Store our identifier
            io.Fonts.SetTexID(new IntPtr(fontTexture));

            // Restore state
            glBindTexture(GL_TEXTURE_2D, last_texture);

            return true;
        }

        static unsafe void ImGui_ImplOpenGL3_DestroyFontsTexture()
        {
            if (fontTexture > 0)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                fixed (uint* p = &fontTexture)
                    glDeleteTextures(1, p);
                io.Fonts.SetTexID(new IntPtr(0));
                fontTexture = 0;
            }
        }

        private static unsafe bool CheckCompileErrors(uint shader, string desc)
        {
            int compiled = 0;
            glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
            if (compiled == GL_FALSE)
            {
                int length = 0;
                glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &length);

                char[] infoLog = new char[length];
                Console.WriteLine("ERROR: ImGui_ImplOpenGL3_CreateDeviceObjects: failed to compile {0}\n{1}", desc, glGetShaderInfoLog(shader, length));

                return true;
            }
            return false;
        }

        private static unsafe bool CheckLinkingErrors(uint program, string desc)
        {
            int status = 0;
            glGetProgramiv(program, GL_LINK_STATUS, &status);
            if (status == GL_FALSE)
            {
                int length = 0;
                glGetProgramiv(program, GL_INFO_LOG_LENGTH, &length);

                char[] infoLog = new char[length];
                Console.WriteLine("ERROR: ImGui_ImplOpenGL3_CreateDeviceObjects: failed to link {0}! (with GLSL '{1}')\n{2}", desc, glslVersionString, glGetProgramInfoLog(program, length));

                return true;
            }
            return false;
        }

        public static unsafe bool ImGui_ImplOpenGL3_CreateDeviceObjects()
        {
            // Backup GL state
            uint last_texture, last_array_buffer;
            glGetIntegerv(GL_TEXTURE_BINDING_2D, (int*)&last_texture);
            glGetIntegerv(GL_ARRAY_BUFFER_BINDING, (int*)&last_array_buffer);

            uint last_vertex_array;
            glGetIntegerv(GL_VERTEX_ARRAY_BINDING, (int*)&last_vertex_array);

            // Parse GLSL version string
            int glsl_version = 130;
            if (int.TryParse(System.Text.RegularExpressions.Regex.Match(glslVersionString, "#version [0-9]{3}").Value, out int ver))
                glsl_version = ver;

            #region Sources
            string vertex_shader_glsl_120 =
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

            string vertex_shader_glsl_130 =
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

            string vertex_shader_glsl_300_es =
                "precision mediump float;\n" +
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

            string vertex_shader_glsl_410_core =
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

            string fragment_shader_glsl_120 =
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

            string fragment_shader_glsl_130 =
                "uniform sampler2D Texture;\n" +
                "in vec2 Frag_UV;\n" +
                "in vec4 Frag_Color;\n" +
                "out vec4 Out_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" +
                "}\n";

            string fragment_shader_glsl_300_es =
                "precision mediump float;\n" +
                "uniform sampler2D Texture;\n" +
                "in vec2 Frag_UV;\n" +
                "in vec4 Frag_Color;\n" +
                "layout (location = 0) out vec4 Out_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" +
                "}\n";

            string fragment_shader_glsl_410_core =
                "in vec2 Frag_UV;\n" +
                "in vec4 Frag_Color;\n" +
                "uniform sampler2D Texture;\n" +
                "layout (location = 0) out vec4 Out_Color;\n" +
                "void main()\n" +
                "{\n" +
                "    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);\n" +
                "}\n";
            #endregion

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
            //string vertex_shader_with_version = glslVersionString + vertex_shader;
            string vertex_shader_with_version = glslVersionString + vertex_shader_glsl_410_core;
            vertHandle = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vertHandle, vertex_shader_with_version);
            glCompileShader(vertHandle);
            CheckCompileErrors(vertHandle, "vertex shader");

            //string fragment_shader_with_version = glslVersionString + fragment_shader;
            string fragment_shader_with_version = glslVersionString + fragment_shader_glsl_410_core;
            fragHandle = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fragHandle, fragment_shader_with_version);
            glCompileShader(fragHandle);
            CheckCompileErrors(fragHandle, "fragment shader");

            shaderHandle = glCreateProgram();
            glAttachShader(shaderHandle, vertHandle);
            glAttachShader(shaderHandle, fragHandle);
            glLinkProgram(shaderHandle);
            CheckLinkingErrors(shaderHandle, "shader program");

            attribLocationTex = glGetUniformLocation(shaderHandle, "Texture");
            attribLocationProjMtx = glGetUniformLocation(shaderHandle, "ProjMtx");
            attribLocationVtxPos = (uint)glGetAttribLocation(shaderHandle, "Position");
            attribLocationVtxUV = (uint)glGetAttribLocation(shaderHandle, "UV");
            attribLocationVtxColor = (uint)glGetAttribLocation(shaderHandle, "Color");

            // Create buffers
            fixed (uint* p = &vboHandle)
                glGenBuffers(1, p);
            fixed (uint* p = &elementsHandle)
                glGenBuffers(1, p);

            ImGui_ImplOpenGL3_CreateFontsTexture();

            // Restore modified GL state
            glBindTexture(GL_TEXTURE_2D, last_texture);

            glBindVertexArray(last_vertex_array);

            return true;
        }

        static unsafe void ImGui_ImplOpenGL3_DestroyDeviceObjects()
        {
            if (vboHandle > 0)
            {
                fixed (uint* p = &vboHandle)
                    glDeleteBuffers(1, p); vboHandle = 0;
            }
            if (elementsHandle > 0)
            {
                fixed (uint* p = &elementsHandle)
                    glDeleteBuffers(1, p); elementsHandle = 0;
            }
            if (shaderHandle > 0 && vertHandle > 0) { glDetachShader(shaderHandle, vertHandle); }
            if (shaderHandle > 0 && fragHandle > 0) { glDetachShader(shaderHandle, fragHandle); }
            if (vertHandle > 0) { glDeleteShader(vertHandle); vertHandle = 0; }
            if (fragHandle > 0) { glDeleteShader(fragHandle); fragHandle = 0; }
            if (shaderHandle > 0) { glDeleteProgram(shaderHandle); shaderHandle = 0; }

            ImGui_ImplOpenGL3_DestroyFontsTexture();
        }
        #endregion
    }
}
