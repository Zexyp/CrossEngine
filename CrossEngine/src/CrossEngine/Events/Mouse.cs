using System;

using CrossEngine.Inputs;

namespace CrossEngine.Events
{
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
        public readonly Button ButtonCode;

        protected MouseButtonEvent(Button code)
        {
            ButtonCode = code;
        }
    }

    public class MousePressedEvent : MouseButtonEvent
    {
        public MousePressedEvent(Button code) : base(code)
        {

        }
    }

    public class MouseReleasedEvent : MouseButtonEvent
    {
        public MouseReleasedEvent(Button code) : base(code)
        {

        }
    }
}
