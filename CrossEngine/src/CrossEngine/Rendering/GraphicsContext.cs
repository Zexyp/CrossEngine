namespace CrossEngine.Rendering
{
    public abstract class GraphicsContext
    {
        public abstract void Init();
        public abstract void Shutdown();
        public abstract void SwapBuffers();
    }
}
