using CrossEngine.Debugging;
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
        internal List<VertexBuffer> vertexBuffers = new List<VertexBuffer>();
        internal IndexBuffer indexBuffer;

        public GdiVertexArray()
        {
        }

        protected internal override void Destroy()
        {
            
        }

        public override void AddVertexBuffer(VertexBuffer vertexBuffer)
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

        public override VertexBuffer[] GetVertexBuffers() => vertexBuffers.ToArray();

        public override IndexBuffer GetIndexBuffer() => indexBuffer;
        public override void SetIndexBuffer(IndexBuffer indexBuffer) => this.indexBuffer = indexBuffer;
    }
}
