using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Rendering
{
    public interface ISurface
    {
        public WeakReference<Framebuffer> Buffer { get; }
        public Vector2 Size { get; }
        public GraphicsContext Context { get; }

        public event Action<ISurface, float, float> Resize;
        public event Action<ISurface> Update;

        void DoResize(float width, float height) => throw new NotSupportedException();
        void DoUpdate() => throw new NotSupportedException();
    }

    public class FramebufferSurface : ISurface
    {
        public WeakReference<Framebuffer> Buffer { get; set; }

        public Vector2 Size { get; private set; }
        public GraphicsContext Context { get; set; }

        public event Action<ISurface, float, float> Resize;
        public event Action<ISurface> Update;

        public FramebufferSurface(WeakReference<Framebuffer> buffer = null)
        {
            Buffer = buffer;
        }

        public void DoResize(float width, float height)
        {
            Size = new(width, height);
            Buffer.GetValue().Resize((uint)width, (uint)height);
            Resize?.Invoke(this, width, height);
        }

        public void DoUpdate()
        {
            Update?.Invoke(this);
        }
    }
}
