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
        // mby graphics context??

        public event Action<ISurface, float, float> Resize;
        public event Action<ISurface> Update;
    }
}
