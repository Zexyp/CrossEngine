using System;
using System.Collections;
using System.Numerics;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Meshes;

public interface IMesh : IDisposable
{
    WeakReference<VertexArray> VA { get; }
    Array Vertices { get; }
    void SetupGpuResources();
    void FreeGpuResources();
}

public interface IIndexedMesh : IMesh
{
    Array Indices { get; }
}

public class Mesh<T> : IMesh where T : struct
{
    public WeakReference<VertexArray> VA { get; private set; }
    public T[] Vertices;

    Array IMesh.Vertices => Vertices;
    WeakReference<VertexBuffer> vb;
    
    public Mesh(T[] vertices)
    {
        this.Vertices = vertices;
    }

    public virtual unsafe void SetupGpuResources()
    {
        /*
        var elementType = Mesh.Vertices.GetType().GetElementType();
        int elementSize = Marshal.SizeOf(elementType);
        
        va = VertexArray.Create();
        
        void* verteciesp = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(Mesh.Vertices));
        vb = VertexBuffer.Create(verteciesp, (uint)(elementSize * Mesh.Vertices.Length));
        
        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType(elementType);

        vb.GetValue().SetLayout(layout);
        va.GetValue().AddVertexBuffer(vb);
        */
        
        VA = VertexArray.Create();
        fixed (T* verteciesp = &Vertices[0])
            vb = VertexBuffer.Create(verteciesp, (uint)(sizeof(T) * Vertices.Length));

        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType<T>();

        vb.GetValue().SetLayout(layout);
        VA.GetValue().AddVertexBuffer(vb);
    }

    public virtual void FreeGpuResources()
    {
        Dispose();
    }

    public virtual void Dispose()
    {
        VA.Dispose();
        vb.Dispose();
        
        vb = null;
        VA = null;
    }
}

public class IndexedMesh<T> : Mesh<T>, IIndexedMesh where T : struct
{
    public uint[] Indices;

    Array IIndexedMesh.Indices => Indices;
    WeakReference<IndexBuffer> ib;

    public IndexedMesh(T[] vertices, uint[] indices) : base(vertices)
    {
        this.Indices = indices;
    }

    public override unsafe void SetupGpuResources()
    {
        base.SetupGpuResources();

        /*
        void* indicesp = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(indexed.Indices));
        ib = IndexBuffer.Create(indicesp, (uint)indexed.Indices.Length, IndexDataType.UInt);
            
        va.GetValue().SetIndexBuffer(ib);
        */
        
        fixed (uint* indicesp = &Indices[0])
            ib = IndexBuffer.Create(indicesp, (uint)Indices.Length, IndexDataType.UInt);

        VA.GetValue().SetIndexBuffer(ib);
    }

    public override void Dispose()
    {
        base.Dispose();

        ib.Dispose();

        ib = null;
    }
}
