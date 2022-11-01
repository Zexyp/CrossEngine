using System;
using GLFW;

using System.Runtime.InteropServices;

using CrossEngine.Events;
using CrossEngine.Logging;

namespace CrossEngine.Platform.Windows
{
    using WindowHandle = GLFW.Window;

    class GlfwWindow : CrossEngine.Display.Window
    {
        public override double Time => Glfw.Time;
        public override bool ShouldClose
        {
            get => Glfw.WindowShouldClose(_nativeHandle);
            set => Glfw.SetWindowShouldClose(_nativeHandle, value);
        }

        public override IntPtr Handle => _nativeHandle;

        private WindowHandle _nativeHandle = WindowHandle.None;

        public GlfwWindow(uint width = 1600, uint height = 900, string title = "Pew")
        {
            Width = width;
            Height = height;
            Title = title;
        }

        protected internal override void CreateWindow()
        {
            Glfw.SetErrorCallback(GlfwErrorCallback);

            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            //Glfw.WindowHint(Hint.Doublebuffer, true);
            //Glfw.WindowHint(Hint.Decorated, true);
            //Glfw.WindowHint(Hint.OpenglForwardCompatible, true);

            _nativeHandle = Glfw.CreateWindow(
                (int)Data.Width,
                (int)Data.Height,
                Data.Title, Data.Fullscreen ? Glfw.PrimaryMonitor : Monitor.None,
                WindowHandle.None);

            Glfw.MakeContextCurrent(_nativeHandle);

            SetupCallbacks();

            // vsync
            Glfw.SwapInterval(Data.VSync ? 1 : 0);
        }

        protected internal override void DestroyWindow()
        {
            Glfw.DestroyWindow(_nativeHandle);
            _nativeHandle = WindowHandle.None;
        }

        protected internal override void SetEventCallback(EventCallbackFunction callback)
        {
            Data.EventCallback = callback;
        }

        protected internal override void UpdateWindow()
        {
            Glfw.SwapBuffers(_nativeHandle);
        }

        protected internal override void PollWindowEvents()
        {
            Glfw.PollEvents();
        }

        public override unsafe void SetIcon(void* data, uint width, uint height)
        {
            GLFW.Image iconImage = new GLFW.Image((int)width, (int)height, new IntPtr(data));
            Glfw.SetWindowIcon(_nativeHandle, 1, new GLFW.Image[] { iconImage });
        }

        protected override void UpdateSize()
        {
            Glfw.SetWindowSize(_nativeHandle, (int)Data.Width, (int)Data.Height);
        }

        protected override void UpdateTitle()
        {
            Glfw.SetWindowTitle(_nativeHandle, Title);
        }

        protected override void UpdateVSync()
        {
            Glfw.SwapInterval(Data.VSync ? 1 : 0);
        }

        protected override void UpdateFullscreen()
        {
            Glfw.SetWindowMonitor(_nativeHandle, Data.Fullscreen ? Glfw.PrimaryMonitor : Monitor.None, Glfw.PrimaryMonitor.WorkArea.X, Glfw.PrimaryMonitor.WorkArea.Y, (int)Data.Width, (int)Data.Height, 60);
        }

        protected override (uint Width, uint Height) GetMonitorSize() => ((uint)Glfw.PrimaryMonitor.WorkArea.Width, (uint)Glfw.PrimaryMonitor.WorkArea.Height);

        private void GlfwErrorCallback(ErrorCode code, IntPtr message)
        {
            Application.CoreLog.Error("[GLFW] " + (int)code + " (" + code.ToString() + "): " + Marshal.PtrToStringAnsi(message));
        }

        private SizeCallback _windowSizeCallbackHolder;
        private WindowCallback _closeCallbackHolder;
        private KeyCallback _keyCallbackHolder;
        private CharCallback _charCallbackHolder;
        private MouseButtonCallback _mouseButtonCallbackHolder;
        private MouseCallback _scrollCallbackHolder;
        private MouseCallback _cursorPositionCallbackHolder;

        private void SetupCallbacks()
        {
            Glfw.SetWindowSizeCallback(_nativeHandle, _windowSizeCallbackHolder = (WindowHandle window, int width, int height) =>
            {
                Data.Width = (uint)width;
                Data.Height = (uint)height;

                Data.EventCallback?.Invoke(new WindowResizeEvent((uint)width, (uint)height));
            });

            Glfw.SetCloseCallback(_nativeHandle, _closeCallbackHolder = (WindowHandle window) =>
            {
                Data.EventCallback?.Invoke(new WindowCloseEvent());
            });

            Glfw.SetKeyCallback(_nativeHandle, _keyCallbackHolder = (WindowHandle window, Keys key, int scanCode, InputState state, ModifierKeys mods) =>
            {
                switch (state)
                {
                    case InputState.Press:
                        {
                            Data.EventCallback?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputState.Release:
                        {
                            Data.EventCallback?.Invoke(new KeyReleasedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputState.Repeat:
                        {
                            Data.EventCallback?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key, 1));
                        }
                        break;
                }
            });

            Glfw.SetCharCallback(_nativeHandle, _charCallbackHolder = (WindowHandle window, uint codePoint) =>
            {
                Data.EventCallback?.Invoke(new KeyTypedEvent((CrossEngine.Inputs.Key)codePoint));
            });

            Glfw.SetMouseButtonCallback(_nativeHandle, _mouseButtonCallbackHolder = (WindowHandle window, MouseButton button, InputState state, ModifierKeys modifiers) =>
            {
                switch (state)
                {
                    case InputState.Press:
                        {
                            Data.EventCallback?.Invoke(new MouseButtonPressedEvent((CrossEngine.Inputs.Mouse)button));
                        }
                        break;
                    case InputState.Release:
                        {
                            Data.EventCallback?.Invoke(new MouseButtonReleasedEvent((CrossEngine.Inputs.Mouse)button));
                        }
                        break;
                }
            });

            Glfw.SetScrollCallback(_nativeHandle, _scrollCallbackHolder = (WindowHandle window, double x, double y) =>
            {
                Data.EventCallback?.Invoke(new MouseScrolledEvent((float)x, (float)y));
            });

            Glfw.SetCursorPositionCallback(_nativeHandle, _cursorPositionCallbackHolder = (WindowHandle window, double x, double y) =>
            {
                Data.EventCallback?.Invoke(new MouseMovedEvent((float)x, (float)y));
            });
        }
    }
}
