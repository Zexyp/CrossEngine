using System;

namespace CrossEngine.Rendering
{
    public abstract class GraphicsContext : IDisposable
    {
        [ThreadStatic]
        static GraphicsContext _current;
        public static GraphicsContext? Current { get => _current; private set => _current = value; }
        public RendererApi Api { get; internal set; }

        public abstract void Init();
        public abstract void Shutdown();
        public abstract void SwapBuffers();
        public abstract void MakeCurrent();
        public virtual void Dispose() { }

        internal static void SetupCurrent(GraphicsContext context)
        {
            context?.MakeCurrent();
            Current = context;
        }
    }
}
