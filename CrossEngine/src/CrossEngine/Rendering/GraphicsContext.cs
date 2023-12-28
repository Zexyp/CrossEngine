using System;

namespace CrossEngine.Rendering
{
    public abstract class GraphicsContext : IDisposable
    {
        public abstract void Init();
        public abstract void Shutdown();
        public abstract void SwapBuffers();
        public virtual void Dispose() { }
    }
}
