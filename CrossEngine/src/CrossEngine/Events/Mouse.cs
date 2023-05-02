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
}
