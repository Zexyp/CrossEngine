using System;
using System.Collections;
using System.Numerics;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Meshes;

public interface IMesh : IDisposable
{
    WeakReference<VertexArray> VA { get; }
    IList Vertices { get; }
    bool Indexed { get; }
}

public class Mesh<T> : IMesh where T : struct
{
    public WeakReference<VertexArray> VA { get; private set; }
    public T[] Vertices;

    bool IMesh.Indexed => false;
    IList IMesh.Vertices => Vertices;
    WeakReference<VertexBuffer> vb;
    
    
    public Mesh(T[] vertices)
    {
        this.Vertices = vertices;
    }

    public virtual unsafe void Setup()
    {
        VA = VertexArray.Create();
        fixed (T* verteciesp = &Vertices[0])
            vb = VertexBuffer.Create(verteciesp, (uint)(sizeof(T) * Vertices.Length));

        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType<T>();

        vb.GetValue().SetLayout(layout);
        VA.GetValue().AddVertexBuffer(vb);
    }

    public virtual void Dispose()
    {
        VA.Dispose();
        vb.Dispose();
        
        vb = null;
        VA = null;
    }
}

public class IndexedMesh<T> : Mesh<T>, IMesh where T : struct
{

    public uint[] Indices;

    bool IMesh.Indexed => true;
    WeakReference<IndexBuffer> ib;

    public IndexedMesh(T[] vertices, uint[] indices) : base(vertices)
    {
        this.Indices = indices;
    }

    public override unsafe void Setup()
    {
        base.Setup();

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
