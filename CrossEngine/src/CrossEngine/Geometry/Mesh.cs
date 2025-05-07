using System;
using System.Collections;
using System.Numerics;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Culling;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Geometry;

// TODO: mesh elements
public interface IMesh : IDisposable
{
    WeakReference<VertexArray> VA { get; }
    Array Vertices { get; }
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
    
    public virtual void Dispose()
    {
        Vertices = null;
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

    public override void Dispose()
    {
        base.Dispose();

        Indices = null;
    }
}
