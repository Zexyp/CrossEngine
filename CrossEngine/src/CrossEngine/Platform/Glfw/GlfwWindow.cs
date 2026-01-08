using System;
using Silk.NET.GLFW;

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Inputs;
using System.Numerics;
using GlfwClass = Silk.NET.GLFW.Glfw;
using System.Diagnostics;

namespace CrossEngine.Platform.Glfw
{
    unsafe public class GlfwWindow : CrossEngine.Display.Window
    {
        internal static GlfwClass glfw;

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

        public void RequestWindowAttention()
        {
            // oh funny that was ez
            glfw.RequestWindowAttention(_nativeHandle);
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
            int x, y, width, height;
            glfw.SetWindowAttrib(_nativeHandle, WindowAttributeSetter.Decorated, !Data.Fullscreen);
            glfw.GetMonitorWorkarea(glfw.GetPrimaryMonitor(), out x, out y, out width, out height);
            glfw.SetWindowMonitor(_nativeHandle, Data.Fullscreen ? glfw.GetWindowMonitor(_nativeHandle) : null, x, y, width, height, 60);
        }

        protected override (uint Width, uint Height) GetMonitorSize()
        {
            int width, height;
            glfw.GetMonitorWorkarea(glfw.GetPrimaryMonitor(), out _, out _, out width, out height);
            return ((uint)width, (uint)height);
        }

        public override void Dispose()
        {
            
        }

        // try static lamdas?
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
                Debug.Assert(_nativeHandle == window);
                Data.Width = (uint)width;
                Data.Height = (uint)height;

                Event?.Invoke(new WindowResizeEvent((uint)width, (uint)height));
            });

            glfw.SetWindowPosCallback(_nativeHandle, _positionCallbackHolder = (WindowHandle* window, int x, int y) =>
            {
                Debug.Assert(_nativeHandle == window);
                Event?.Invoke(new WindowMovedEvent((float)x, (float)y));
            });

            glfw.SetWindowRefreshCallback(_nativeHandle, _refreshCallbackHolder = (WindowHandle* window) =>
            {
                Debug.Assert(_nativeHandle == window);
                Event?.Invoke(new WindowRefreshEvent());
            });

            glfw.SetWindowCloseCallback(_nativeHandle, _closeCallbackHolder = (WindowHandle* window) =>
            {
                Debug.Assert(_nativeHandle == window);
                Event?.Invoke(new WindowCloseEvent());
            });

            glfw.SetWindowFocusCallback(_nativeHandle, _focusCallbackHolder = (WindowHandle* window, bool focused) =>
            {
                Debug.Assert(_nativeHandle == window);
                Event?.Invoke(new WindowFucusEvent(focused));
            });

            glfw.SetKeyCallback(_nativeHandle, _keyCallbackHolder = (WindowHandle* window, Keys key, int scanCode, InputAction state, KeyModifiers mods) =>
            {
                Debug.Assert(_nativeHandle == window);
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
                Debug.Assert(_nativeHandle == window);
                Event?.Invoke(new KeyCharEvent((char)codePoint));
            });

            glfw.SetMouseButtonCallback(_nativeHandle, _mouseButtonCallbackHolder = (WindowHandle* window, MouseButton button, InputAction state, KeyModifiers modifiers) =>
            {
                Debug.Assert(_nativeHandle == window);
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
                Debug.Assert(_nativeHandle == window);
                Mouse.Scroll(new((float)x, (float)y));
                Event?.Invoke(new MouseScrolledEvent((float)x, (float)y));
            });

            glfw.SetCursorPosCallback(_nativeHandle, _cursorPositionCallbackHolder = (WindowHandle* window, double x, double y) =>
            {
                Debug.Assert(_nativeHandle == window);
                Mouse.Position(new((float)x, (float)y));
                Event?.Invoke(new MouseMovedEvent((float)x, (float)y));
            });
        }

        public override GraphicsContext InitGraphics(GraphicsApi api)
        {
            Debug.Assert(Graphics == null);

            switch (api)
            {
                case GraphicsApi.OpenGL: return Graphics = new OpenGL.GLContext(_nativeHandle);
#if WINDOWS
                case GraphicsApi.GDI: return Graphics = new Windows.GdiContext(Process.GetCurrentProcess().MainWindowHandle);
#endif
                default: throw new PlatformNotSupportedException();
            }
        }
    }
}
