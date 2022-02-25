using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Events;

namespace CrossEngine.Display
{
    internal struct WindowProperties
    {
        public uint Width, Height;
        public string Title;

    
        public WindowProperties(string title, uint width = 1600, uint height = 900)
        {
            this.Title = title;
            this.Width = width;
            this.Height = height;
        }
    }

    public abstract class Window
    {
        public delegate void EventCallbackFunction(Event e);

        protected struct WindowData
        {
            public uint Width, Height;
            public string Title;

            public EventCallbackFunction EventCallback;
        }

        protected WindowData Data;

        public uint Width { get => Data.Width; set { Data.Width = value; UpdateWindowSize(); } }
        public uint Height { get => Data.Height; set { Data.Height = value; UpdateWindowSize(); } }
        public string Title { get => Data.Title; set { Data.Title = value; UpdateWindowTitle(); } }

        public abstract double Time { get; }
        public abstract bool ShouldClose { get; }
        public abstract bool VSync { get; set; }


        //public abstract void SetIcon(System.Drawing.Image image);
        protected internal abstract void CreateWindow();
        protected internal abstract void DestroyWindow();
        protected internal abstract void UpdateWindow();
        protected internal abstract void PollWindowEvents();

        protected abstract void UpdateWindowSize();
        protected abstract void UpdateWindowTitle();

        protected internal abstract void SetEventCallback(EventCallbackFunction callback);
    }
}
