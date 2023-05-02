using System;

using CrossEngine.Inputs;

namespace CrossEngine.Events
{
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
}
