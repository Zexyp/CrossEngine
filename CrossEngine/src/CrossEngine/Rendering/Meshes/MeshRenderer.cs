using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CrossEngine.Geometry;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Materials;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Rendering.Meshes;

public class MeshRenderer : IDisposable
{
    WeakReference<VertexBuffer> vb;
    WeakReference<IndexBuffer> ib;
    WeakReference<VertexArray> va;

    public static MeshRenderer FromMesh(IMesh mesh)
    {
        throw new NotImplementedException();
    }

    public unsafe void Setup(IMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        
        Debug.Assert(va == null && vb == null && ib == null);

        var elementType = mesh.Vertices.GetType().GetElementType();
        int elementSize = Marshal.SizeOf(elementType);
        
        va = VertexArray.Create();
        
        void* verteciesp = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(mesh.Vertices));
        vb = VertexBuffer.Create(verteciesp, (uint)(elementSize * mesh.Vertices.Length));
        
        // this is really nice
        BufferLayout layout = BufferLayout.FromStructType(elementType);

        vb.GetValue().SetLayout(layout);
        va.GetValue().AddVertexBuffer(vb);

        if (mesh is IIndexedMesh indexed)
        {
            void* indicesp = Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(indexed.Indices));
            ib = IndexBuffer.Create(indicesp, (uint)indexed.Indices.Length, IndexDataType.UInt);
            
            va.GetValue().SetIndexBuffer(ib);
        }
    }

    public void Dispose()
    {
        vb.Dispose();
        va.Dispose();
        ib?.Dispose();
        
        vb = null;
        va = null;
        ib = null;
    }

    //public void Draw()
    //{
    //    if (ib is null)
    //        GraphicsContext.Current.Api.DrawIndexed(va);
    //    else
    //        GraphicsContext.Current.Api.DrawArray(va, (uint)Mesh.Vertices.Length);
    //}
}