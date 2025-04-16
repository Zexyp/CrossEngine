using CrossEngine.Rendering.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    class GdiVertexArray : VertexArray
    {
        internal List<WeakReference<VertexBuffer>> vertexBuffers = new List<WeakReference<VertexBuffer>>();
        internal WeakReference<IndexBuffer> indexBuffer;

        public override void AddVertexBuffer(WeakReference<VertexBuffer> vertexBuffer)
        {
            vertexBuffers.Add(vertexBuffer);
        }

        public override void Bind()
        {
            state.va = this;
        }

        public override void Unbind()
        {
            state.va = null;
        }

        public override WeakReference<VertexBuffer>[] GetVertexBuffers() => vertexBuffers.ToArray();

        public override WeakReference<IndexBuffer> GetIndexBuffer() => indexBuffer;
        public override void SetIndexBuffer(WeakReference<IndexBuffer> indexBuffer) => this.indexBuffer = indexBuffer;
    }
}
