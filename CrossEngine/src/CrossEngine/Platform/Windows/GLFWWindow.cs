using System;
using GLFW;

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Inputs;
using System.Numerics;

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
        public WindowHandle NativeHandle => _nativeHandle;

        public override event OnEventFunction Event;

        private WindowHandle _nativeHandle = WindowHandle.None;
        private static Logger Log = new Logger("GLFW");

        public GlfwWindow(uint width = 1600, uint height = 900, string title = "Pew")
        {
            Width = width;
            Height = height;
            Title = title;
        }

        public override void Create()
        {
            Glfw.Init();

            //Glfw.SetErrorCallback(GlfwErrorCallback);

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
                Data.Title, Data.Fullscreen ? Glfw.PrimaryMonitor : GLFW.Monitor.None,
                WindowHandle.None);

            Glfw.MakeContextCurrent(_nativeHandle);

            SetupCallbacks();

            // vsync
            Glfw.SwapInterval(Data.VSync ? 1 : 0);

            // oh funny that was ez
            Glfw.RequestWindowAttention(_nativeHandle);

            Context = new GLContext(_nativeHandle);
            Context.Init();
        }

        public override void Destroy()
        {
            Glfw.DestroyWindow(_nativeHandle);
            _nativeHandle = WindowHandle.None;
        }

        public override void PollEvents()
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
            Glfw.SetWindowMonitor(_nativeHandle, Data.Fullscreen ? Glfw.PrimaryMonitor : GLFW.Monitor.None, Glfw.PrimaryMonitor.WorkArea.X, Glfw.PrimaryMonitor.WorkArea.Y, (int)Data.Width, (int)Data.Height, 60);
        }

        protected override (uint Width, uint Height) GetMonitorSize() => ((uint)Glfw.PrimaryMonitor.WorkArea.Width, (uint)Glfw.PrimaryMonitor.WorkArea.Height);

        //private void GlfwErrorCallback(ErrorCode code, IntPtr message)
        //{
        //    Log.Error(((int)code) + " (" + code.ToString() + "): " + Marshal.PtrToStringAnsi(message));
        //}

        private SizeCallback _windowSizeCallbackHolder;
        private WindowCallback _closeCallbackHolder;
        private KeyCallback _keyCallbackHolder;
        private CharCallback _charCallbackHolder;
        private MouseButtonCallback _mouseButtonCallbackHolder;
        private MouseCallback _scrollCallbackHolder;
        private MouseCallback _cursorPositionCallbackHolder;
        private FocusCallback _focusCallbackHolder;
        private PositionCallback _positionCallbackHolder;
        private WindowCallback _refreshCallbackHolder;

        private void SetupCallbacks()
        {
            // window
            Glfw.SetWindowSizeCallback(_nativeHandle, _windowSizeCallbackHolder = (IntPtr window, int width, int height) =>
            {
                Data.Width = (uint)width;
                Data.Height = (uint)height;

                Event?.Invoke(new WindowResizeEvent((uint)width, (uint)height));
            });

            Glfw.SetWindowPositionCallback(_nativeHandle, _positionCallbackHolder = (IntPtr window, double x, double y) =>
            {
                Event?.Invoke(new WindowMovedEvent((float)x, (float)y));
            });

            Glfw.SetWindowRefreshCallback(_nativeHandle, _refreshCallbackHolder = (IntPtr window) =>
            {
                Event?.Invoke(new WindowRefreshEvent());
            });

            Glfw.SetCloseCallback(_nativeHandle, _closeCallbackHolder = (IntPtr window) =>
            {
                Event?.Invoke(new WindowCloseEvent());
            });

            Glfw.SetWindowFocusCallback(_nativeHandle, _focusCallbackHolder = (IntPtr window, bool focused) =>
            {
                Event?.Invoke(new WindowFucusEvent(focused));
            });

            Glfw.SetKeyCallback(_nativeHandle, _keyCallbackHolder = (IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods) =>
            {
                switch (state)
                {
                    case InputState.Press:
                        {
                            Keyboard.Add((CrossEngine.Inputs.Key)key);
                            Event?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputState.Release:
                        {
                            Keyboard.Remove((CrossEngine.Inputs.Key)key);
                            Event?.Invoke(new KeyReleasedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputState.Repeat:
                        {
                            Keyboard.Add((CrossEngine.Inputs.Key)key);
                            Event?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key, true));
                        }
                        break;
                }
            });

            Glfw.SetCharCallback(_nativeHandle, _charCallbackHolder = (IntPtr window, uint codePoint) =>
            {
                Event?.Invoke(new KeyCharEvent((char)codePoint));
            });

            Glfw.SetMouseButtonCallback(_nativeHandle, _mouseButtonCallbackHolder = (IntPtr window, MouseButton button, InputState state, ModifierKeys modifiers) =>
            {
                switch (state)
                {
                    case InputState.Press:
                        {
                            Mouse.Add((CrossEngine.Inputs.Button)button);
                            Event?.Invoke(new MousePressedEvent((CrossEngine.Inputs.Button)button));
                        }
                        break;
                    case InputState.Release:
                        {
                            Mouse.Remove((CrossEngine.Inputs.Button)button);
                            Event?.Invoke(new MouseReleasedEvent((CrossEngine.Inputs.Button)button));
                        }
                        break;
                }
            });

            Glfw.SetScrollCallback(_nativeHandle, _scrollCallbackHolder = (IntPtr window, double x, double y) =>
            {
                Mouse.Scroll(new((float)x, (float)y));
                Event?.Invoke(new MouseScrolledEvent((float)x, (float)y));
            });

            Glfw.SetCursorPositionCallback(_nativeHandle, _cursorPositionCallbackHolder = (IntPtr window, double x, double y) =>
            {
                Mouse.Position(new((float)x, (float)y));
                Event?.Invoke(new MouseMovedEvent((float)x, (float)y));
            });
        }
    }
}
