using System;

using CrossEngine.Inputs;

namespace CrossEngine.Events
{
    public delegate void OnEventFunction<T>(T e) where T : Event;

    abstract public class Event
    {
        public virtual bool Handled { get; set; } = false;

        public override string ToString()
        {
            Type type = this.GetType();
            return type.Name + ": {" + String.Join("; ", Array.ConvertAll(type.GetFields(), item => item.GetValue(this).ToString())) + "}";
        }
    }

    #region Window
    public abstract class WindowEvent : Event { }

    public class WindowResizeEvent : WindowEvent
    {
        public readonly uint Width;
        public readonly uint Height;

        public WindowResizeEvent(uint width, uint height)
        {
            this.Width = width;
            this.Height = height;
        }
    }

    public class WindowCloseEvent : WindowEvent
    {

    }
    #endregion

    #region Key
    abstract public class KeyEvent : Event
    {
        public readonly Key KeyCode;

        protected KeyEvent(Key code)
        {
            KeyCode = code;
        }
    }

    public class KeyPressedEvent : KeyEvent
    {
        public readonly int RepeatCount;

        public KeyPressedEvent(Key keyCode, int repeat = 0) : base(keyCode)
        {
            this.RepeatCount = repeat;
        }
    }

    public class KeyReleasedEvent : KeyEvent
    {
        public KeyReleasedEvent(Key keyCode) : base(keyCode)
        {

        }
    }

    public class KeyTypedEvent : KeyEvent
    {
        public KeyTypedEvent(Key keyCode) : base(keyCode)
        {

        }
    }
    #endregion

    #region Mouse
    public abstract class MouseEvent : Event { }

    public class MouseMovedEvent : MouseEvent
    {
        public readonly float X;
        public readonly float Y;

        public MouseMovedEvent(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public class MouseScrolledEvent : MouseEvent
    {
        public readonly float X;
        public readonly float Y;

        public MouseScrolledEvent(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    abstract public class MouseButtonEvent : MouseEvent
    {
        public readonly Mouse ButtonCode;

        protected MouseButtonEvent(Mouse code)
        {
            ButtonCode = code;
        }
    }

    public class MouseButtonPressedEvent : MouseButtonEvent
    {
        public MouseButtonPressedEvent(Mouse buttonCode) : base(buttonCode)
        {
            
        }
    }

    public class MouseButtonReleasedEvent : MouseButtonEvent
    {
        public MouseButtonReleasedEvent(Mouse buttonCode) : base(buttonCode)
        {
            
        }
    }
    #endregion
}
