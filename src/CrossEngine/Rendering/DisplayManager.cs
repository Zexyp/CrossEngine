using System;
using GLFW;
using static OpenGL.GL;

using System.Numerics;
using System.Drawing;
using System.IO;

using CrossEngine.Rendering.Texturing;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Display
{
    public static class DisplayManager
    {
        public static Window Window { get; private set; }

        public static Vector2 WindowSize { get; private set; }
        public static float WindowAspectRatio { get; private set; }

        public static void CreateWindow(int width, int height, string title, bool resizable = true)
        {
            // class variables
            WindowSize = new Vector2(width, height);
            WindowAspectRatio = (float)width / (float)height;

            Glfw.Init();

            // opengl 3.3 core profile
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            //Glfw.WindowHint(Hint.Doublebuffer, true);

            Glfw.WindowHint(Hint.Focused, true); // focuse because it could be also background window
            Glfw.WindowHint(Hint.Resizable, resizable); // resizable

            // creation
            Window = Glfw.CreateWindow(width, height, title, Monitor.None, Window.None);

            if(Window == Window.None)
            {
                Log.Error("window creation failed!");
                return;
            }

            // optional centering of the window
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            Glfw.SetWindowPosition(Window, (screen.Width - width) / 2, (screen.Height - height) / 2);

            // telling both opengl and glfw that we want to work with our newly created window
            Glfw.MakeContextCurrent(Window);
            Import(Glfw.GetProcAddress);

            // telling opengl the size of the window
            glViewport(0, 0, width, height);
            Glfw.SwapInterval(0); // VSync is off (1 if VSync on)

            // setup callbacks for events
            SetupCallbacks();
        }

        public static void CloseWindow() => Glfw.Terminate();

        public static void ShouldClose(bool should) => Glfw.SetWindowShouldClose(Window, should);

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
    }
}
