using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Rendering
{
    internal abstract class GraphicsContext
    {
        public abstract void Init();
        public abstract void SwapBuffers();
    }
}
