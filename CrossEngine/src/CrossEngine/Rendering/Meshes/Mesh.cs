using System;
using System.Numerics;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Meshes;

public interface IMesh
{
    WeakReference<VertexArray> VA { get; }
}

public class Mesh<T> : IMesh where T : struct
{
    public WeakReference<VertexArray> VA { get; private set; }
    WeakReference<VertexBuffer> vb;
    
    public T[] Vertices;
    
    public Mesh(T[] vertices)
    {
        this.Vertices = vertices;

        SetupMesh();
    }

    private unsafe void SetupMesh()
    {
        VA = VertexArray.Create();
        VA.GetValue().Bind(); // needs to be bound because it was ONLY created

        fixed (T* verteciesp = &Vertices[0])
            vb = VertexBuffer.Create(verteciesp, (uint)(sizeof(T) * Vertices.Length));

        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType<T>();

        vb.GetValue().SetLayout(layout);
        VA.GetValue().AddVertexBuffer(vb);

        VA.GetValue().Unbind();
    }

    public void Dispose()
    {
        VA.Dispose();
        vb.Dispose();
        
        vb = null;
        VA = null;
    }
}

/*
public class IndexedMesh<T> : IMesh where T : struct
{
    public WeakReference<VertexArray> VA { get; private set; }
    WeakReference<IndexBuffer> ib;
    WeakReference<VertexBuffer> vb;

    T[] vertices;
    uint[] indices;

    public IndexedMesh(T[] vertices, uint[] indices)
    {
        this.vertices = vertices;
        this.indices = indices;

        SetupMesh();
    }

    private unsafe void SetupMesh()
    {
        VA = VertexArray.Create();
        VA.GetValue().Bind(); // needs to be bound because it was ONLY created

        fixed (T* verteciesp = &vertices[0])
            vb = VertexBuffer.Create(verteciesp, (uint)(sizeof(T) * vertices.Length));
        fixed (uint* indicesp = &indices[0])
            ib = IndexBuffer.Create(indicesp, (uint)indices.Length, IndexDataType.UInt);

        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType<T>();

        vb.GetValue().SetLayout(layout);
        VA.GetValue().AddVertexBuffer(vb);
        VA.GetValue().SetIndexBuffer(ib);

        VA.GetValue().Unbind();
    }

    public void Dispose()
    {
        VA.Dispose();
        vb.Dispose();
        ib.Dispose();

        vb = null;
        ib = null;
        VA = null;
    }
}
*/
