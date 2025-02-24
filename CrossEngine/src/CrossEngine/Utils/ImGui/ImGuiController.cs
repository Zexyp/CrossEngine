﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#define GL

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using CrossEngine.Inputs;
using ImGuiNET;
using CrossEngine.Utils;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using CrossEngine.Display;
using CrossEngine.Events;
using Silk.NET.GLFW;
using CrossEngine.Platform.Glfw;

#if GLES
using Silk.NET.OpenGLES;
#elif GL
using Silk.NET.OpenGL;
#elif LEGACY
using Silk.NET.OpenGL.Legacy;
#endif
using static CrossEngine.Platform.Glfw.GlfwWindow;


namespace CrossEngine.Utils.ImGui
{
    using ImGui = ImGuiNET.ImGui;

    static class Util
    {
        [Pure]
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        [Conditional("DEBUG")]
        public static void CheckGlError(this GL gl, string title)
        {
            var error = gl.GetError();
            if (error != GLEnum.NoError)
            {
                Debug.Print($"{title}: {error}");
            }
        }
    }

    // no communism
    unsafe class MyImGuiController : IDisposable
    {
        // TODO: cursors

        private GL _gl;
        private bool _frameBegun;
        private readonly List<char> _pressedChars = new List<char>();

        private int _attribLocationTex;
        private int _attribLocationProjMtx;
        private int _attribLocationVtxPos;
        private int _attribLocationVtxUV;
        private int _attribLocationVtxColor;
        private uint _vboHandle;
        private uint _elementsHandle;
        private uint _vertexArrayObject;

        private Texture _fontTexture;
        private Shader _shader;

        private int _windowWidth;
        private int _windowHeight;

        public IntPtr Context;

        private Window _window;
        private float _scrollY;
        private float _scrollX;

        private Cursor*[] _mouseCursors = new Cursor*[(int)ImGuiMouseCursor.COUNT];

        /// <summary>
        /// Constructs a new ImGuiController with font configuration and onConfigure Action.
        /// </summary>
        public MyImGuiController(GL gl, Window window, Action onConfigureIO = null)
        {
            Init(gl, window);

            var io = ImGuiNET.ImGui.GetIO();
            //if (imGuiFontConfig is not null)
            //{
            //    var glyphRange = imGuiFontConfig.Value.GetGlyphRange?.Invoke(io) ?? default(IntPtr);
            //
            //    io.Fonts.AddFontFromFileTTF(imGuiFontConfig.Value.FontPath, imGuiFontConfig.Value.FontSize, null, glyphRange);
            //}
            
            onConfigureIO?.Invoke();
                
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            BeginFrame();
        }

        public void MakeCurrent()
        {
            ImGuiNET.ImGui.SetCurrentContext(Context);
        }

        private void Init(GL gl, Window window)
        {
            _gl = gl;
            _window = window;
            _windowWidth = (int)_window.Width;
            _windowHeight = (int)_window.Height;

            Context = ImGuiNET.ImGui.CreateContext();
            ImGuiNET.ImGui.SetCurrentContext(Context);
            ImGuiNET.ImGui.StyleColorsDark();
            var io = ImGui.GetIO();

            
            _mouseCursors[(int)ImGuiMouseCursor.Arrow] = glfw.CreateStandardCursor(CursorShape.Arrow);
            _mouseCursors[(int)ImGuiMouseCursor.TextInput] = glfw.CreateStandardCursor(CursorShape.IBeam);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeNS] = glfw.CreateStandardCursor(CursorShape.VResize);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeEW] = glfw.CreateStandardCursor(CursorShape.HResize);
            _mouseCursors[(int)ImGuiMouseCursor.Hand] = glfw.CreateStandardCursor(CursorShape.Hand);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeAll] = glfw.CreateStandardCursor(CursorShape.AllResize);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeNESW] = glfw.CreateStandardCursor(CursorShape.NeswResize);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeNWSE] = glfw.CreateStandardCursor(CursorShape.NwseResize);
            _mouseCursors[(int)ImGuiMouseCursor.NotAllowed] = glfw.CreateStandardCursor(CursorShape.NotAllowed);
        }

        private void UpdateMouseCursor()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0 || (CursorModeValue)glfw.GetInputMode(((CrossEngine.Platform.Glfw.GlfwWindow)_window).NativeHandle, CursorStateAttribute.Cursor) == CursorModeValue.CursorDisabled)
                return;

            ImGuiMouseCursor imgui_cursor = ImGui.GetMouseCursor();
            // (those braces are here to reduce diff with multi-viewports support in 'docking' branch)
            {
                WindowHandle* window = ((CrossEngine.Platform.Glfw.GlfwWindow)_window).NativeHandle;
                if (imgui_cursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
                {
                    // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                    glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorHidden);
                }
                else
                {
                    // Show OS mouse cursor
                    // FIXME-PLATFORM: Unfocused windows seems to fail changing the mouse cursor with GLFW 3.2, but 3.3 works here.
                    glfw.SetCursor(window, _mouseCursors[(int)imgui_cursor] != null ? _mouseCursors[(int)imgui_cursor] : _mouseCursors[(int)ImGuiMouseCursor.Arrow]);
                    glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
                }
            }
        }

        private void BeginFrame()
        {
            ImGuiNET.ImGui.NewFrame();
            _frameBegun = true;
            _window.Event += OnEvent;
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _window.Event -= OnEvent;

            _gl.DeleteBuffer(_vboHandle);
            _gl.DeleteBuffer(_elementsHandle);
            _gl.DeleteVertexArray(_vertexArrayObject);

            _fontTexture.Dispose();
            _shader.Dispose();

            ImGuiNET.ImGui.DestroyContext(Context);
        }

        private void OnEvent(Event e)
        {
            new EventDispatcher(e)
                .Dispatch<WindowResizeEvent>(wre => WindowResized(new IntVec2((int)wre.Width, (int)wre.Height)))
                .Dispatch<KeyCharEvent>(kce => OnKeyChar(kce.Char))
                .Dispatch<MouseScrolledEvent>(mse => { _scrollY += mse.Y; _scrollX += mse.X; });
        }

        private void OnKeyChar(char ch)
        {
            _pressedChars.Add(ch);
        }

        private void WindowResized(IntVec2 size)
        {
            _windowWidth = size.X;
            _windowHeight = size.Y;
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                var oldCtx = ImGuiNET.ImGui.GetCurrentContext();

                if (oldCtx != Context)
                {
                    ImGuiNET.ImGui.SetCurrentContext(Context);
                }

                _frameBegun = false;
                ImGuiNET.ImGui.Render();
                RenderImDrawData(ImGuiNET.ImGui.GetDrawData());

                UpdateMouseCursor();

                if (oldCtx != Context)
                {
                    ImGuiNET.ImGui.SetCurrentContext(oldCtx);
                }
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            var oldCtx = ImGuiNET.ImGui.GetCurrentContext();

            if (oldCtx != Context)
            {
                ImGuiNET.ImGui.SetCurrentContext(Context);
            }

            if (_frameBegun)
            {
                ImGuiNET.ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();

            _frameBegun = true;
            ImGuiNET.ImGui.NewFrame();

            if (oldCtx != Context)
            {
                ImGuiNET.ImGui.SetCurrentContext(oldCtx);
            }
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            var io = ImGuiNET.ImGui.GetIO();
            io.DisplaySize = new Vector2(_windowWidth, _windowHeight);

            if (_windowWidth > 0 && _windowHeight > 0)
            {
                io.DisplayFramebufferScale = new Vector2(1, 1);
            }

            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        private static Key[] keyEnumArr = (Key[])Enum.GetValues(typeof(Key));
        private void UpdateImGuiInput()
        {
            var io = ImGuiNET.ImGui.GetIO();

            //var mouseState = _input.Mice[0].CaptureState();
            //var keyboardState = _input.Keyboards[0];

            io.MouseDown[0] = _window.Mouse.Get(Button.Left);
            io.MouseDown[1] = _window.Mouse.Get(Button.Right);
            io.MouseDown[2] = _window.Mouse.Get(Button.Middle);

            var mousePos = _window.Mouse.CursorPosition;
            var point = new Point((int)mousePos.X, (int)mousePos.Y);
            io.MousePos = new Vector2(point.X, point.Y);

            io.MouseWheel = _scrollY;
            io.MouseWheelH = _scrollX;
            _scrollX = 0;
            _scrollY = 0;

            foreach (var key in keyEnumArr)
            {
                if (key == Key.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = _window.Keyboard.Get(key);
            }

            foreach (var c in _pressedChars)
            {
                io.AddInputCharacter(c);
            }

            _pressedChars.Clear();

            io.KeyCtrl = _window.Keyboard.Get(Key.LeftControl) || _window.Keyboard.Get(Key.RightControl);
            io.KeyAlt = _window.Keyboard.Get(Key.LeftAlt) || _window.Keyboard.Get(Key.RightAlt);
            io.KeyShift = _window.Keyboard.Get(Key.LeftShift) || _window.Keyboard.Get(Key.RightShift);
            io.KeySuper = _window.Keyboard.Get(Key.LeftSuper) || _window.Keyboard.Get(Key.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            _pressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            var io = ImGuiNET.ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.ArrowLeft;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.ArrowRight;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.ArrowUp;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.ArrowDown;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        }

        private unsafe void SetupRenderState(ImDrawDataPtr drawDataPtr, int framebufferWidth, int framebufferHeight)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, polygon fill
            _gl.Enable(GLEnum.Blend);
            _gl.BlendEquation(GLEnum.FuncAdd);
            _gl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
            _gl.Disable(GLEnum.CullFace);
            _gl.Disable(GLEnum.DepthTest);
            _gl.Disable(GLEnum.StencilTest);
            _gl.Enable(GLEnum.ScissorTest);
    #if !GLES && !LEGACY
            _gl.Disable(GLEnum.PrimitiveRestart);
            _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
    #endif

            float L = drawDataPtr.DisplayPos.X;
            float R = drawDataPtr.DisplayPos.X + drawDataPtr.DisplaySize.X;
            float T = drawDataPtr.DisplayPos.Y;
            float B = drawDataPtr.DisplayPos.Y + drawDataPtr.DisplaySize.Y;

            Span<float> orthoProjection = stackalloc float[] {
                2.0f / (R - L),
                0.0f,
                0.0f,
                0.0f,
                0.0f,
                2.0f / (T - B),
                0.0f,
                0.0f,
                0.0f,
                0.0f,
                -1.0f,
                0.0f,
                (R + L) / (L - R),
                (T + B) / (B - T),
                0.0f,
                1.0f,
            };

            _shader.UseShader();
            _gl.Uniform1(_attribLocationTex, 0);
            _gl.UniformMatrix4(_attribLocationProjMtx, 1, false, orthoProjection);
            _gl.CheckGlError("Projection");

            _gl.BindSampler(0, 0);

            // Setup desired GL state
            // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
            // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
            _vertexArrayObject = _gl.GenVertexArray();
            _gl.BindVertexArray(_vertexArrayObject);
            _gl.CheckGlError("VAO");

            // Bind vertex/index buffers and setup attributes for ImDrawVert
            _gl.BindBuffer(GLEnum.ArrayBuffer, _vboHandle);
            _gl.BindBuffer(GLEnum.ElementArrayBuffer, _elementsHandle);
            _gl.EnableVertexAttribArray((uint)_attribLocationVtxPos);
            _gl.EnableVertexAttribArray((uint)_attribLocationVtxUV);
            _gl.EnableVertexAttribArray((uint)_attribLocationVtxColor);
            _gl.VertexAttribPointer((uint)_attribLocationVtxPos, 2, GLEnum.Float, false, (uint)sizeof(ImDrawVert), (void*)0);
            _gl.VertexAttribPointer((uint)_attribLocationVtxUV, 2, GLEnum.Float, false, (uint)sizeof(ImDrawVert), (void*)8);
            _gl.VertexAttribPointer((uint)_attribLocationVtxColor, 4, GLEnum.UnsignedByte, true, (uint)sizeof(ImDrawVert), (void*)16);
        }

        private unsafe void RenderImDrawData(ImDrawDataPtr drawDataPtr)
        {
            int framebufferWidth = (int)(drawDataPtr.DisplaySize.X * drawDataPtr.FramebufferScale.X);
            int framebufferHeight = (int)(drawDataPtr.DisplaySize.Y * drawDataPtr.FramebufferScale.Y);
            if (framebufferWidth <= 0 || framebufferHeight <= 0)
                return;

            // Backup GL state
            _gl.GetInteger(GLEnum.ActiveTexture, out int lastActiveTexture);
            _gl.ActiveTexture(GLEnum.Texture0);

            _gl.GetInteger(GLEnum.CurrentProgram, out int lastProgram);
            _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);

            _gl.GetInteger(GLEnum.SamplerBinding, out int lastSampler);

            _gl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
            _gl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArrayObject);

    #if !GLES
            Span<int> lastPolygonMode = stackalloc int[2];
            _gl.GetInteger(GLEnum.PolygonMode, lastPolygonMode);
    #endif

            Span<int> lastScissorBox = stackalloc int[4];
            _gl.GetInteger(GLEnum.ScissorBox, lastScissorBox);

            _gl.GetInteger(GLEnum.BlendSrcRgb, out int lastBlendSrcRgb);
            _gl.GetInteger(GLEnum.BlendDstRgb, out int lastBlendDstRgb);

            _gl.GetInteger(GLEnum.BlendSrcAlpha, out int lastBlendSrcAlpha);
            _gl.GetInteger(GLEnum.BlendDstAlpha, out int lastBlendDstAlpha);

            _gl.GetInteger(GLEnum.BlendEquationRgb, out int lastBlendEquationRgb);
            _gl.GetInteger(GLEnum.BlendEquationAlpha, out int lastBlendEquationAlpha);

            bool lastEnableBlend = _gl.IsEnabled(GLEnum.Blend);
            bool lastEnableCullFace = _gl.IsEnabled(GLEnum.CullFace);
            bool lastEnableDepthTest = _gl.IsEnabled(GLEnum.DepthTest);
            bool lastEnableStencilTest = _gl.IsEnabled(GLEnum.StencilTest);
            bool lastEnableScissorTest = _gl.IsEnabled(GLEnum.ScissorTest);

    #if !GLES && !LEGACY
            bool lastEnablePrimitiveRestart = _gl.IsEnabled(GLEnum.PrimitiveRestart);
    #endif

            SetupRenderState(drawDataPtr, framebufferWidth, framebufferHeight);

            // Will project scissor/clipping rectangles into framebuffer space
            Vector2 clipOff = drawDataPtr.DisplayPos;         // (0,0) unless using multi-viewports
            Vector2 clipScale = drawDataPtr.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

            // Render command lists
            for (int n = 0; n < drawDataPtr.CmdListsCount; n++)
            {
                ImDrawListPtr cmdListPtr = drawDataPtr.CmdLists[n];

                // Upload vertex/index buffers

                _gl.BufferData(GLEnum.ArrayBuffer, (nuint)(cmdListPtr.VtxBuffer.Size * sizeof(ImDrawVert)), (void*)cmdListPtr.VtxBuffer.Data, GLEnum.StreamDraw);
                _gl.CheckGlError($"Data Vert {n}");
                _gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(cmdListPtr.IdxBuffer.Size * sizeof(ushort)), (void*)cmdListPtr.IdxBuffer.Data, GLEnum.StreamDraw);
                _gl.CheckGlError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmdListPtr.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr cmdPtr = cmdListPtr.CmdBuffer[cmd_i];

                    if (cmdPtr.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        Vector4 clipRect;
                        clipRect.X = (cmdPtr.ClipRect.X - clipOff.X) * clipScale.X;
                        clipRect.Y = (cmdPtr.ClipRect.Y - clipOff.Y) * clipScale.Y;
                        clipRect.Z = (cmdPtr.ClipRect.Z - clipOff.X) * clipScale.X;
                        clipRect.W = (cmdPtr.ClipRect.W - clipOff.Y) * clipScale.Y;

                        if (clipRect.X < framebufferWidth && clipRect.Y < framebufferHeight && clipRect.Z >= 0.0f && clipRect.W >= 0.0f)
                        {
                            // Apply scissor/clipping rectangle
                            _gl.Scissor((int)clipRect.X, (int)(framebufferHeight - clipRect.W), (uint)(clipRect.Z - clipRect.X), (uint)(clipRect.W - clipRect.Y));
                            _gl.CheckGlError("Scissor");

                            // Bind texture, Draw
                            _gl.BindTexture(GLEnum.Texture2D, (uint)cmdPtr.TextureId);
                            _gl.CheckGlError("Texture");

                            _gl.DrawElementsBaseVertex(GLEnum.Triangles, cmdPtr.ElemCount, GLEnum.UnsignedShort, (void*)(cmdPtr.IdxOffset * sizeof(ushort)), (int)cmdPtr.VtxOffset);
                            _gl.CheckGlError("Draw");
                        }
                    }
                }
            }

            // Destroy the temporary VAO
            _gl.DeleteVertexArray(_vertexArrayObject);
            _vertexArrayObject = 0;

            // Restore modified GL state
            _gl.UseProgram((uint)lastProgram);
            _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);

            _gl.BindSampler(0, (uint)lastSampler);

            _gl.ActiveTexture((GLEnum)lastActiveTexture);

            _gl.BindVertexArray((uint)lastVertexArrayObject);

            _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);
            _gl.BlendEquationSeparate((GLEnum)lastBlendEquationRgb, (GLEnum)lastBlendEquationAlpha);
            _gl.BlendFuncSeparate((GLEnum)lastBlendSrcRgb, (GLEnum)lastBlendDstRgb, (GLEnum)lastBlendSrcAlpha, (GLEnum)lastBlendDstAlpha);

            if (lastEnableBlend)
            {
                _gl.Enable(GLEnum.Blend);
            }
            else
            {
                _gl.Disable(GLEnum.Blend);
            }

            if (lastEnableCullFace)
            {
                _gl.Enable(GLEnum.CullFace);
            }
            else
            {
                _gl.Disable(GLEnum.CullFace);
            }

            if (lastEnableDepthTest)
            {
                _gl.Enable(GLEnum.DepthTest);
            }
            else
            {
                _gl.Disable(GLEnum.DepthTest);
            }
            if (lastEnableStencilTest)
            {
                _gl.Enable(GLEnum.StencilTest);
            }
            else
            {
                _gl.Disable(GLEnum.StencilTest);
            }

            if (lastEnableScissorTest)
            {
                _gl.Enable(GLEnum.ScissorTest);
            }
            else
            {
                _gl.Disable(GLEnum.ScissorTest);
            }

    #if !GLES && !LEGACY
            if (lastEnablePrimitiveRestart)
            {
                _gl.Enable(GLEnum.PrimitiveRestart);
            }
            else
            {
                _gl.Disable(GLEnum.PrimitiveRestart);
            }

            _gl.PolygonMode(GLEnum.FrontAndBack, (GLEnum)lastPolygonMode[0]);
    #endif

            _gl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint)lastScissorBox[2], (uint)lastScissorBox[3]);
        }

        private void CreateDeviceResources()
        {
            // Backup GL state

            _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);
            _gl.GetInteger(GLEnum.ArrayBufferBinding, out int lastArrayBuffer);
            _gl.GetInteger(GLEnum.VertexArrayBinding, out int lastVertexArray);

            string vertexSource =
    #if GLES
                    @"#version 300 es
            precision highp float;
            
            layout (location = 0) in vec2 Position;
            layout (location = 1) in vec2 UV;
            layout (location = 2) in vec4 Color;
            uniform mat4 ProjMtx;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = ProjMtx * vec4(Position.xy,0.0,1.0);
            }";
    #elif GL
                    @"#version 330
            layout (location = 0) in vec2 Position;
            layout (location = 1) in vec2 UV;
            layout (location = 2) in vec4 Color;
            uniform mat4 ProjMtx;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = ProjMtx * vec4(Position.xy,0,1);
            }";
    #elif LEGACY
                    @"#version 110
            attribute vec2 Position;
            attribute vec2 UV;
            attribute vec4 Color;

            uniform mat4 ProjMtx;

            varying vec2 Frag_UV;
            varying vec4 Frag_Color;

            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = ProjMtx * vec4(Position.xy,0,1);
            }";
    #endif


            string fragmentSource =
    #if GLES
                    @"#version 300 es
            precision highp float;
        
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            uniform sampler2D Texture;
            layout (location = 0) out vec4 Out_Color;
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }";
    #elif GL
                    @"#version 330
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            uniform sampler2D Texture;
            layout (location = 0) out vec4 Out_Color;
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }";
    #elif LEGACY
                    @"#version 110
            varying vec2 Frag_UV;
            varying vec4 Frag_Color;

            uniform sampler2D Texture;

            void main()
            {
                gl_FragColor = Frag_Color * texture2D(Texture, Frag_UV.st);
            }";
    #endif

            _shader = new Shader(_gl, vertexSource, fragmentSource);

            _attribLocationTex = _shader.GetUniformLocation("Texture");
            _attribLocationProjMtx = _shader.GetUniformLocation("ProjMtx");
            _attribLocationVtxPos = _shader.GetAttribLocation("Position");
            _attribLocationVtxUV = _shader.GetAttribLocation("UV");
            _attribLocationVtxColor = _shader.GetAttribLocation("Color");

            _vboHandle = _gl.GenBuffer();
            _elementsHandle = _gl.GenBuffer();

            RecreateFontDeviceTexture();

            // Restore modified GL state
            _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);
            _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);

            _gl.BindVertexArray((uint)lastVertexArray);

            _gl.CheckGlError("End of ImGui setup");
        }

        /// <summary>
        /// Creates the texture used to render text.
        /// </summary>
        private unsafe void RecreateFontDeviceTexture()
        {
            // Build texture atlas
            var io = ImGuiNET.ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);   // Load as RGBA 32-bit (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders. If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

            // Upload texture to graphics system
            _gl.GetInteger(GLEnum.TextureBinding2D, out int lastTexture);

            _fontTexture = new Texture(_gl, width, height, pixels);
            _fontTexture.Bind();
            _fontTexture.SetMagFilter(TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(TextureMinFilter.Linear);

            // Store our identifier
            io.Fonts.SetTexID((IntPtr)_fontTexture.GlTexture);

            // Restore state
            _gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);
        }
    }
}