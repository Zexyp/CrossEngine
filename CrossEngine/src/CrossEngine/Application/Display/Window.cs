using System;
using GLFW;
using static OpenGL.GL;

using System.Runtime.InteropServices;
using System.Diagnostics;

using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Utils;

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

    public class Window
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

        internal GLFW.Window Handle { get; private set; }

        public uint Width { get => data.width; set => Glfw.SetWindowSize(Handle, (int)(data.width = value), (int)data.height); }
        public uint Height { get => data.height; set => Glfw.SetWindowSize(Handle, (int)data.width, (int)(data.height = value)); }
        public string Title { get => data.title; set => Glfw.SetWindowTitle(Handle, data.title = value); }

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
                {
                    Log.Core.Fatal("GLFW failed to initialize");
                    Debug.Assert(false, "GLFW failed to initialize");
                }

                Glfw.SetErrorCallback(GLFWErrorCallback); // debug
            }

            // opengl 3.3 core profile
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

#if DEBUG
            Glfw.WindowHint(Hint.OpenglDebugContext, true);
#endif

            //Glfw.WindowHint(Hint.Doublebuffer, true);

            // keeping just most importatnt things now

            //Glfw.WindowHint(Hint.Focused, true); // focus because it could be also background window
            //Glfw.WindowHint(Hint.Resizable, false); // resizable

            // creation
            Handle = Glfw.CreateWindow((int)data.width, (int)data.height, data.title, Monitor.None, GLFW.Window.None);
            windowCount++;

            if (Handle == GLFW.Window.None)
            {
                Log.Core.Fatal("window creation failed!");
                Debug.Assert(false, "failed to create window");
                return;
            }

            // optional centering of the window
            //Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            //Glfw.SetWindowPosition(window, (screen.Width - width) / 2, (screen.Height - height) / 2);

            // telling glfw that we want to work with our newly created window
            Glfw.MakeContextCurrent(Handle);

            Import(Glfw.GetProcAddress);

            Log.Core.Info("\n    vendor: {0}\n    renderer: {1}\n    version: {2}", glGetString(GL_VENDOR), glGetString(GL_RENDERER), glGetString(GL_VERSION));

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

        internal void SetEventCallback(EventCallbackFunction callback)
        {
            data.EventCallback = callback;
        }

        public void SetVSync(bool enable)
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
    }
}
