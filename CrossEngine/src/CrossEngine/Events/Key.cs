using System;

using CrossEngine.Inputs;
using GLFW;

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
        public readonly bool Repeated;

        public KeyPressedEvent(Key keyCode, bool repeat = false) : base(keyCode)
        {
            this.Repeated = repeat;
        }
    }

    public class KeyReleasedEvent : KeyEvent
    {
        public KeyReleasedEvent(Key keyCode) : base(keyCode)
        {

        }
    }

    public class KeyCharEvent : KeyEvent
    {
        public readonly char Char;
        public KeyCharEvent(char ch) : base(Key.Unknown)
        {
            Console.WriteLine($"char {ch}");
            Char = ch;
        }
    }
}
