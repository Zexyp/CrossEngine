using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Rendering;

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

    public abstract class Window : IDisposable
    {
        protected struct WindowData
        {
            public uint Width, Height;
            public bool VSync, Fullscreen;
            public string Title;
        }

        protected WindowData Data;

        public GraphicsContext Context { get; protected set; }

        public uint Width { get => Data.Width; set { Data.Width = value; if (Handle != IntPtr.Zero) UpdateSize(); } }
        public uint Height { get => Data.Height; set { Data.Height = value; if (Handle != IntPtr.Zero) UpdateSize(); } }
        public string Title { get => Data.Title; set { Data.Title = value; if (Handle != IntPtr.Zero) UpdateTitle(); } }
        public bool VSync { get => Data.VSync; set { Data.VSync = value; if (Handle != IntPtr.Zero) UpdateVSync(); } }
        public bool Fullscreen => Data.Fullscreen;

        public abstract double Time { get; }
        public abstract bool ShouldClose { get; set; }
        public abstract IntPtr Handle { get; }

        public readonly Keyboard Keyboard = new Keyboard();
        public readonly Mouse Mouse = new Mouse();

        // event emission
        public abstract event Action<Event> Event;

        public Window()
        {
            Data.VSync = true;
            Data.Fullscreen = false;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        //public abstract void SetIcon(System.Drawing.Image image);
        public abstract void Create();
        public abstract void Destroy();
        public abstract void PollEvents();
        public abstract unsafe void SetIcon(void* data, uint width, uint height);

        protected abstract void UpdateSize();
        protected abstract void UpdateTitle();
        protected abstract void UpdateVSync();
        protected abstract void UpdateFullscreen();
        protected abstract (uint Width, uint Height) GetMonitorSize();

        public void Resize(uint width, uint height)
        {
            Data.Width = width;
            Data.Height = height;
            UpdateSize();
        }

        public void SetFullscreen(bool enable, bool matchResolution = true)
        {
            if (enable && matchResolution)
            {
                (var x, var y) = GetMonitorSize();
                Resize(x, y);
            }
            Data.Fullscreen = enable;
            UpdateFullscreen();
        }
    }
}
