using System;
using CrossEngine.Debugging;

namespace CrossEngine.Rendering;

public abstract class GpuObject : IDisposable
{
    public bool Disposed => disposed;
    private bool disposed = false;
    public virtual uint RendererId => throw new NotSupportedException();

    public GpuObject()
    {
        GpuGC.Register(this);
    }
    
    ~GpuObject()
    {
        Dispose(disposing: false);
        GpuGC.Unregister(this);
    }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        GpuGC.Unregister(this);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
            return;
        
        if (disposing)
        {
            // managed resources here
        }

        // unmanaged resources here
            
        GpuGC.Destroy(this);

        this.disposed = true;
    }
    
    // unmanaged resources
    abstract internal protected void Destroy();
}