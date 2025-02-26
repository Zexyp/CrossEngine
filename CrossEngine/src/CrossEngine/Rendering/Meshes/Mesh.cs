using System;
using System.Numerics;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Meshes;

public class Mesh<T> : IDisposable where T : struct
{
    WeakReference<VertexArray> va;
    WeakReference<IndexBuffer> ib;
    WeakReference<VertexBuffer> vb;
    
    T[] vertices;
    uint[] indices;
    
    public Mesh(T[] vertices, uint[] indices)
    {
        this.vertices = vertices;
        this.indices = indices;

        SetupMesh();
    }

    private unsafe void SetupMesh()
    {
        va = VertexArray.Create();
        va.GetValue().Bind(); // needs to be bound because it was ONLY created

        fixed (T* verteciesp = &vertices[0])
            vb = VertexBuffer.Create(verteciesp, (uint)(sizeof(T) * vertices.Length));
        fixed (uint* indicesp = &indices[0])
            ib = IndexBuffer.Create(indicesp, (uint)indices.Length, IndexDataType.UInt);

        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType<T>();

        vb.GetValue().SetLayout(layout);
        va.GetValue().AddVertexBuffer(vb);
        va.GetValue().SetIndexBuffer(ib);

        va.GetValue().Unbind();
    }

    public void Dispose()
    {
        va.Dispose();
        vb.Dispose();
        ib.Dispose();
        
        vb = null;
        ib = null;
        va = null;
    }
}