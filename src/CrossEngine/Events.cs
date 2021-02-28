using System;

using CrossEngine.InputSystem;

namespace CrossEngine
{
    public class MouseEventArgs : EventArgs
    {
        public double x;
        public double y;
    }
    public class KeyboardEventArgs : EventArgs
    {
        public Key key;
    }
    public class WindowResizedEventArgs : EventArgs
    {
        public int width;
        public int height;
    }

    public class Events
    {
        #region Mouse
        public static event EventHandler<MouseEventArgs> OnMouseScrolled;
        #endregion
        #region Keyboard
        public static event EventHandler<KeyboardEventArgs> OnKeyPressed;
        public static event EventHandler<KeyboardEventArgs> OnKeyReleased;
        #endregion
        #region Window
        public static event EventHandler<WindowResizedEventArgs> OnWindowResized;
        #endregion

        #region Render
        public static event EventHandler OnRenderStart;
        public static event EventHandler OnRenderEnd;
        #endregion

        // now the sending is anonymous i know

        public static void SendOnMouseScrolledEvent(double x, double y)
        {
            OnMouseScrolled?.Invoke(null, new MouseEventArgs() { x = x, y = y });
        }
        public static void SendOnKeyPressedEvent(Key key)
        {
            OnKeyPressed?.Invoke(null, new KeyboardEventArgs() { key = key });
        }
        public static void SendOnKeyReleasedEvent(Key key)
        {
            OnKeyReleased?.Invoke(null, new KeyboardEventArgs() { key = key });
        }
        public static void SendOnWindowResized(int width, int height)
        {
            OnWindowResized?.Invoke(null, new WindowResizedEventArgs() { width = width, height = height });
        }

        public static void SendOnRenderStart()
        {
            OnRenderStart?.Invoke(null, EventArgs.Empty);
        }
        public static void SendOnRenderEnd()
        {
            OnRenderEnd?.Invoke(null, EventArgs.Empty);
        }
    }
}
