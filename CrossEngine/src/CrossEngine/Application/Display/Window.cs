using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Utils;
using GLFW;
using System;
using System.Runtime.InteropServices;
using static OpenGL.GL;

namespace CrossEngine.Rendering.Display
{
    internal struct WindowProperties
    {
        public uint width, height;
        public string title;

        public WindowProperties(string title, uint width = 1600, uint height = 900)
        {
            this.title = title;
            this.width = width;
            this.height = height;
        }
    }

    internal class Window
    {
        public delegate void EventCallbackFunction(Event e);

        internal struct WindowData
        {
            public uint width, height;
            public string title;

            public EventCallbackFunction EventCallback;
        }

        public static double Time { get { return Glfw.Time; } }

        private static int windowCount = 0;

        public GLFW.Window Handle { get; private set; }

        public uint Width { get => data.width; }
        public uint Height { get => data.height; }

        private WindowData data;

        public bool ShouldClose
        {
            get
            {
                return Glfw.WindowShouldClose(Handle);
            }
            set
            {
                Glfw.SetWindowShouldClose(Handle, value);
            }
        }

        internal Window(WindowProperties props)
        {
            Init(props);
        }

        private void GLFWErrorCallback(ErrorCode code, IntPtr message)
        {
            Log.Core.Error("[GLFW] " + (int)code + " (" + code.ToString() + "): " + Marshal.PtrToStringAnsi(message));
        }

        private unsafe void Init(WindowProperties props)
        {
            data.width = props.width;
            data.height = props.height;
            data.title = props.title;

            if (windowCount == 0)
            {
                bool state = Glfw.Init();

                if (state)
                    Log.Core.Info("GLFW initialized");
                else
                    Log.Core.Error("GLFW failed to initialize");

                Glfw.SetErrorCallback(GLFWErrorCallback); // debug
            }

            // opengl 3.3 core profile
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            //Glfw.WindowHint(Hint.OpenglDebugContext, true); // debug

            //Glfw.WindowHint(Hint.Doublebuffer, true);

            // keeping just most importatnt things now

            //Glfw.WindowHint(Hint.Focused, true); // focuse because it could be also background window
            //Glfw.WindowHint(Hint.Resizable, false); // resizable

            // creation
            Handle = Glfw.CreateWindow((int)data.width, (int)data.height, data.title, Monitor.None, GLFW.Window.None);
            windowCount++;

            if (Handle == GLFW.Window.None)
            {
                Log.Core.Error("window creation failed!");
                return;
            }

            // optional centering of the window
            //Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            //Glfw.SetWindowPosition(window, (screen.Width - width) / 2, (screen.Height - height) / 2);

            // telling both opengl and glfw that we want to work with our newly created window
            Glfw.MakeContextCurrent(Handle); // TODO: fix this (active context)
            Import(Glfw.GetProcAddress);

            Log.Core.Trace("\n    vendor: {0}\n    renderer: {1}\n    version: {2}", glGetString(GL_VENDOR), glGetString(GL_RENDERER), glGetString(GL_VERSION));

            SetVSync(true);

            // telling opengl the size of the window
            //glViewport(0, 0, (int)data.width, (int)data.height);

            // setup callbacks for events
            SetupCallbacks();

            using (System.Drawing.Bitmap icon = Properties.Resources.DefaultWindowIcon.ToBitmap())
                SetIcon(icon);
        }

        #region Callback Holders
        private SizeCallback windowSizeCallbackHolder;
        private WindowCallback closeCallbackHolder;
        private KeyCallback keyCallbackHolder;
        private CharCallback charCallbackHolder;
        private MouseButtonCallback mouseButtonCallbackHolder;
        private MouseCallback scrollCallbackHolder;
        private MouseCallback cursorPositionCallbackHolder;
        #endregion
        private void SetupCallbacks()
        {
            Glfw.SetWindowSizeCallback(Handle, windowSizeCallbackHolder = (GLFW.Window window, int width, int height) =>
            {
                data.width = (uint)width;
                data.height = (uint)height;

                data.EventCallback?.Invoke(new WindowResizeEvent((uint)width, (uint)height));
            });

            Glfw.SetCloseCallback(Handle, closeCallbackHolder = (GLFW.Window window) =>
            {
                data.EventCallback?.Invoke(new WindowCloseEvent());
            });

            Glfw.SetKeyCallback(Handle, keyCallbackHolder = (GLFW.Window window, Keys key, int scanCode, InputState state, ModifierKeys mods) =>
            {
                switch (state)
                {
                    case InputState.Press:
                        {
                            data.EventCallback?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputState.Release:
                        {
                            data.EventCallback?.Invoke(new KeyReleasedEvent((CrossEngine.Inputs.Key)key));
                        }
                        break;
                    case InputState.Repeat:
                        {
                            data.EventCallback?.Invoke(new KeyPressedEvent((CrossEngine.Inputs.Key)key, 1));
                        }
                        break;
                }
            });

            Glfw.SetCharCallback(Handle, charCallbackHolder = (GLFW.Window window, uint codePoint) =>
            {
                data.EventCallback?.Invoke(new KeyTypedEvent((CrossEngine.Inputs.Key)codePoint));
            });

            Glfw.SetMouseButtonCallback(Handle, mouseButtonCallbackHolder = (GLFW.Window window, MouseButton button, InputState state, ModifierKeys modifiers) =>
            {
                switch (state)
                {
                    case InputState.Press:
                        {
                            data.EventCallback?.Invoke(new MouseButtonPressedEvent((CrossEngine.Inputs.Mouse)button));
                        }
                        break;
                    case InputState.Release:
                        {
                            data.EventCallback?.Invoke(new MouseButtonReleasedEvent((CrossEngine.Inputs.Mouse)button));
                        }
                        break;
                }
            });

            Glfw.SetScrollCallback(Handle, scrollCallbackHolder = (GLFW.Window window, double x, double y) =>
            {
                data.EventCallback?.Invoke(new MouseScrolledEvent((float)x, (float)y));
            });

            Glfw.SetCursorPositionCallback(Handle, cursorPositionCallbackHolder = (GLFW.Window window, double x, double y) =>
            {
                data.EventCallback?.Invoke(new MouseMovedEvent((float)x, (float)y));
            });
        }

        public void CloseWindow()
        {
            Glfw.DestroyWindow(Handle);
            windowCount--;
            if (windowCount == 0)
            {
                Glfw.Terminate();
            }
        }

        public void Update()
        {
            Glfw.PollEvents();
            Glfw.SwapBuffers(Handle);
        }

        public void SetEventCallback(EventCallbackFunction callback)
        {
            data.EventCallback = callback;
        }

        private void SetVSync(bool enable)
        {
            if (enable)
            {
                Glfw.SwapInterval(1); // 1 is VSync on
                Log.Core.Trace("vsync is on");
            }
            else
            {
                Glfw.SwapInterval(0);
                Log.Core.Trace("vsync is off");
            }
        }

        public void SetIcon(System.Drawing.Image icon)
        {
            ImageUtils.SwapChannels(icon, ImageUtils.ColorChannel.Red, ImageUtils.ColorChannel.Blue); // this will come and bite later :D

            System.Drawing.Imaging.BitmapData bitmapData = ((System.Drawing.Bitmap)icon).LockBits(new System.Drawing.Rectangle(0, 0, icon.Width, icon.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, icon.PixelFormat);

            GLFW.Image iconImage = new GLFW.Image(icon.Width, icon.Height, bitmapData.Scan0);
            Glfw.SetWindowIcon(Handle, 1, new GLFW.Image[] { iconImage });

            ((System.Drawing.Bitmap)icon).UnlockBits(bitmapData);

            Log.Core.Trace("window icon set");
        }

        /*
        public void ShouldClose(bool should) => Glfw.SetWindowShouldClose(window, should);

        static void UpdateSize()
        {
            Glfw.GetFramebufferSize(Window, out int width, out int height);
            glViewport(0, 0, width, height);

            // class variables
            WindowSize = new Vector2(width, height);
            WindowAspectRatio = (float)width / (float)height;
        }

        public static void SetWindowSize(int width, int height, bool center = false)
        {
            // setting
            Glfw.SetWindowSize(Window, width, height);
            // centering
            if (center)
            {
                Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
                Glfw.SetWindowPosition(Window, (screen.Width - width) / 2, (screen.Height - height) / 2);
            }

            UpdateSize();
        }

        public static void EnableCursor(bool enabled)
        {
            if (enabled) Glfw.SetInputMode(Window, InputMode.Cursor, (int)CursorMode.Normal);
            else Glfw.SetInputMode(Window, InputMode.Cursor, (int)CursorMode.Disabled);
        }

        public static void SetIcon(System.Drawing.Image icon)
        {
            ImageOperations.SwapChannels(icon, ImageOperations.ColorChannel.Red, ImageOperations.ColorChannel.Blue);
            System.Drawing.Imaging.BitmapData iconData = ((Bitmap)icon).LockBits(new Rectangle(0, 0, icon.Width, icon.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, icon.PixelFormat);
            GLFW.Image iconImage = new GLFW.Image(icon.Width, icon.Height, iconData.Scan0);
            Glfw.SetWindowIcon(Window, 1, new GLFW.Image[] { iconImage });
        }

        public static void SetIcon(string path)
        {
            if (File.Exists(path))
                SetIcon(new Bitmap(path));
            else
                Log.Error("icon not found!");
        }


        #region Callbacks
        public static void SetupCallbacks()
        {
            // callbacks
            WindowCallbackHolder.SetFramebufferSizeCallback(WindowSizeCallback);
            WindowCallbackHolder.SetWindowFocusCallback(WindowFocusCallback);

            // events
            Events.OnWindowResized += OnWindowResized;
        }

        public static void WindowSizeCallback(Window window, int width, int height)
        {
            Events.SendOnWindowResized(width, height);
        }

        public static void WindowFocusCallback(Window window, bool focusing)
        {
            if (focusing) Glfw.SwapInterval(0);
            else Glfw.SwapInterval(1);
        }
        #endregion

        #region Events
        public static void OnWindowResized(object sender, WindowResizedEventArgs args)
        {
            UpdateSize();
        }
        #endregion
        /*
        public static class WindowCallbackHolder
        {
            // mouse
            static MouseCallback cursor_position_callback;
            static MouseButtonCallback mouse_button_callback;
            static MouseCallback scroll_callback;

            // keyboard
            static KeyCallback key_callback;

            // window
            static SizeCallback framebuffer_size_callback;
            static FocusCallback focus_callback;

            // mouse
            public static void SetCursorPositionCallback(MouseCallback cursorPositionCallback)
            {
                cursor_position_callback = cursorPositionCallback;
                Glfw.SetCursorPositionCallback(Window, cursor_position_callback);
            }

            public static void SetMouseButtonCallback(MouseButtonCallback mouseButtonCallback)
            {
                mouse_button_callback = mouseButtonCallback;
                Glfw.SetMouseButtonCallback(Window, mouse_button_callback);
            }

            public static void SetScrollCallback(MouseCallback scrollCallback)
            {
                scroll_callback = scrollCallback;
                Glfw.SetScrollCallback(Window, scroll_callback);
            }

            // keyboard
            public static void SetKeyCallback(KeyCallback keyCallback)
            {
                key_callback = keyCallback;
                Glfw.SetKeyCallback(Window, key_callback);
            }

            // window
            public static void SetFramebufferSizeCallback(SizeCallback framebufferSizeCallback)
            {
                framebuffer_size_callback = framebufferSizeCallback;
                Glfw.SetFramebufferSizeCallback(Window, framebuffer_size_callback);
            }

            public static void SetWindowFocusCallback(FocusCallback focusCallback)
            {
                focus_callback = focusCallback;
                Glfw.SetWindowFocusCallback(Window, focus_callback);
            }
        }
        */
    }
}
