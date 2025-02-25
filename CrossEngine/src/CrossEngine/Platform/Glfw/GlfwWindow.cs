using System;
using Silk.NET.GLFW;

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Inputs;
using System.Numerics;
using GlfwClass = Silk.NET.GLFW.Glfw;

namespace CrossEngine.Platform.Glfw
{
    unsafe class GlfwWindow : CrossEngine.Display.Window
    {
        internal static GlfwClass glfw = Silk.NET.GLFW.Glfw.GetApi();

        public override double Time => glfw.GetTime();
        public override bool ShouldClose
        {
            get => glfw.WindowShouldClose(_nativeHandle);
            set => glfw.SetWindowShouldClose(_nativeHandle, value);
        }
        public override IntPtr Handle => (IntPtr)_nativeHandle;
        public WindowHandle* NativeHandle => _nativeHandle;

        public override event Action<Event> Event;

        private WindowHandle* _nativeHandle = null;
        private static Logger Log = new Logger("GLFW");

        public GlfwWindow(uint width = 1600, uint height = 900, string title = "Pew")
        {
            Width = width;
            Height = height;
            Title = title;
        }

        public override void Init()
        {
            glfw.Init();

            glfw.SetErrorCallback(GlfwErrorCallback);

            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            //Glfw.WindowHint(Hint.Doublebuffer, true);
            //Glfw.WindowHint(Hint.Decorated, true);
            //Glfw.WindowHint(Hint.OpenglForwardCompatible, true);
        }

        public override void Create()
        {
            _nativeHandle = glfw.CreateWindow(
                (int)Data.Width,
                (int)Data.Height,
                Data.Title, Data.Fullscreen ? glfw.GetPrimaryMonitor() : null,
                null);

            glfw.MakeContextCurrent(_nativeHandle);

            SetupCallbacks();

            // vsync
            glfw.SwapInterval(Data.VSync ? 1 : 0);

            // oh funny that was ez
            glfw.RequestWindowAttention(_nativeHandle);

            Context = new GLContext(_nativeHandle);
            Context.Init();
        }

        public override void Destroy()
        {
            glfw.DestroyWindow(_nativeHandle);
            _nativeHandle = null;
        }

        public override void PollEvents()
        {
            glfw.PollEvents();
        }

        public override unsafe void SetIcon(void* data, uint width, uint height)
        {
            Image iconImage = new Image() { Width = (int)width, Height = (int)height, Pixels = (byte*)data };
            glfw.SetWindowIcon(_nativeHandle, 1, &iconImage);
        }

        protected override void UpdateSize()
        {
            glfw.SetWindowSize(_nativeHandle, (int)Data.Width, (int)Data.Height);
        }

        protected override void UpdateTitle()
        {
            glfw.SetWindowTitle(_nativeHandle, Title);
        }

        protected override void UpdateVSync()
        {
            glfw.SwapInterval(Data.VSync ? 1 : 0);
        }

        protected override void UpdateFullscreen()
        {
            throw new NotImplementedException();
            //glfw.SetWindowMonitor(_nativeHandle, Data.Fullscreen ? glfw.GetPrimaryMonitor() : GLFW.Monitor.None, glfw.GetPrimaryMonitor().WorkArea.X, glfw.PrimaryMonitor.WorkArea.Y, (int)Data.Width, (int)Data.Height, 60);
        }

        protected override (uint Width, uint Height) GetMonitorSize() => throw new NotImplementedException(); //((uint)glfw.PrimaryMonitor.WorkArea.Width, (uint)glfw.PrimaryMonitor.WorkArea.Height);

        private void GlfwErrorCallback(ErrorCode code, string message)
        {
            Log.Error(((int)code) + " (" + code.ToString() + "): " + message);
        }

        public override void Dispose()
        {
            glfw.Terminate();
        }

        private Silk.NET.GLFW.GlfwCallbacks.WindowSizeCallback _windowSizeCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.WindowCloseCallback _closeCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.KeyCallback _keyCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.CharCallback _charCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.MouseButtonCallback _mouseButtonCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.ScrollCallback _scrollCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.CursorPosCallback _cursorPositionCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.WindowFocusCallback _focusCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.WindowPosCallback _positionCallbackHolder;
        private Silk.NET.GLFW.GlfwCallbacks.WindowRefreshCallback _refreshCallbackHolder;

        private unsafe void SetupCallbacks()
        {
            // window
            glfw.SetWindowSizeCallback(_nativeHandle, _windowSizeCallbackHolder = (WindowHandle* window, int width, int height) =>
            {
                Data.Width = (uint)width;
                Data.Height = (uint)height;

                Event?.Invoke(new WindowResizeEvent((uint)width, (uint)height));
            });

            glfw.SetWindowPosCallback(_nativeHandle, _positionCallbackHolder = (WindowHandle* window, int x, int y) =>
            {
                Event?.Invoke(new WindowMovedEvent((float)x, (float)y));
            });

            glfw.SetWindowRefreshCallback(_nativeHandle, _refreshCallbackHolder = (WindowHandle* window) =>
            {
                Event?.Invoke(new WindowRefreshEvent());
            });

            glfw.SetWindowCloseCallback(_nativeHandle, _closeCallbackHolder = (WindowHandle* window) =>
            {
                Event?.Invoke(new WindowCloseEvent());
            });

            glfw.SetWindowFocusCallback(_nativeHandle, _focusCallbackHolder = (WindowHandle* window, bool focused) =>
            {
                Event?.Invoke(new WindowFucusEvent(focused));
            });

            glfw.SetKeyCallback(_nativeHandle, _keyCallbackHolder = (WindowHandle* window, Keys key, int scanCode, InputAction state, KeyModifiers mods) =>
            {
                switch (state)
                {
                    case InputAction.Press:
                        {
                            Keyboard.Add((CrossEngine.Inputs.Key)key);
                            Event?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputAction.Release:
                        {
                            Keyboard.Remove((CrossEngine.Inputs.Key)key);
                            Event?.Invoke(new KeyReleasedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputAction.Repeat:
                        {
                            Keyboard.Add((CrossEngine.Inputs.Key)key);
                            Event?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key, true));
                        }
                        break;
                }
            });

            glfw.SetCharCallback(_nativeHandle, _charCallbackHolder = (WindowHandle* window, uint codePoint) =>
            {
                Event?.Invoke(new KeyCharEvent((char)codePoint));
            });

            glfw.SetMouseButtonCallback(_nativeHandle, _mouseButtonCallbackHolder = (WindowHandle* window, MouseButton button, InputAction state, KeyModifiers modifiers) =>
            {
                switch (state)
                {
                    case InputAction.Press:
                        {
                            Mouse.Add((CrossEngine.Inputs.Button)button);
                            Event?.Invoke(new MousePressedEvent((CrossEngine.Inputs.Button)button));
                        }
                        break;
                    case InputAction.Release:
                        {
                            Mouse.Remove((CrossEngine.Inputs.Button)button);
                            Event?.Invoke(new MouseReleasedEvent((CrossEngine.Inputs.Button)button));
                        }
                        break;
                }
            });

            glfw.SetScrollCallback(_nativeHandle, _scrollCallbackHolder = (WindowHandle* window, double x, double y) =>
            {
                Mouse.Scroll(new((float)x, (float)y));
                Event?.Invoke(new MouseScrolledEvent((float)x, (float)y));
            });

            glfw.SetCursorPosCallback(_nativeHandle, _cursorPositionCallbackHolder = (WindowHandle* window, double x, double y) =>
            {
                Mouse.Position(new((float)x, (float)y));
                Event?.Invoke(new MouseMovedEvent((float)x, (float)y));
            });
        }
    }
}
