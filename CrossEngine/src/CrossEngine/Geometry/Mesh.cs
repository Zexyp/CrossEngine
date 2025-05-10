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
    AABox Bounds => throw new NotSupportedException();
}

public interface IIndexedMesh : IMesh
{
    Array Indices { get; }
}

public interface IPosition
{
    Vector3 Position { get; }
}

public class Mesh<T> : IMesh where T : struct, IPosition
{
    public WeakReference<VertexArray> VA { get; private set; }
    public T[] Vertices;
    AABox IMesh.Bounds => _bounds;

    Array IMesh.Vertices => Vertices;
    WeakReference<VertexBuffer> vb;
    private AABox _bounds;
    
    public Mesh(T[] vertices)
    {
        this.Vertices = vertices;

        var min = Vertices[0].Position;
        var max = Vertices[0].Position;
        for (int i = 1; i < Vertices.Length; i++)
        {
            min = Vector3.Min(min, Vertices[i].Position);
            max = Vector3.Max(max, Vertices[i].Position);
        }

        _bounds = AABox.CreateFromExtents(min, max);
    }
    
    public virtual void Dispose()
    {
        Vertices = null;
    }
}

public class IndexedMesh<T> : Mesh<T>, IIndexedMesh where T : struct, IPosition
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
