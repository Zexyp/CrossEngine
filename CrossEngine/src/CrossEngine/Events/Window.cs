using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Events
{
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

    public class WindowMovedEvent : WindowEvent
    {
        public readonly float X;
        public readonly float Y;

        public WindowMovedEvent(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public class WindowRefreshEvent : WindowEvent
    {

    }

    public class WindowCloseEvent : WindowEvent
    {

    }

    public class WindowFucusEvent : WindowEvent
    {
        public readonly bool Focused;

        public WindowFucusEvent(bool focused)
        {
            this.Focused = focused;
        }
    }
}
