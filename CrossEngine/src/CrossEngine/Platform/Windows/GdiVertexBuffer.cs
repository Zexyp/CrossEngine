using CrossEngine.Rendering.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    class GdiVertexBuffer : VertexBuffer
    {
        BufferLayout _layout;
        internal UnmanagedMemoryStream stream;

        public unsafe GdiVertexBuffer(void* data, uint size)
        {
            SetData(data, size);
        }

        public override void Bind()
        {
            state.vb = this;
        }

        public override void Unbind()
        {
            state.vb = null;
        }

        public override unsafe void SetData(void* data, uint size, uint offset = 0)
        {
            stream?.Dispose();
            if (data != null) stream = new UnmanagedMemoryStream((byte*)data, size);
            else stream = null;
        }

        public override BufferLayout GetLayout() => _layout;
        public override void SetLayout(BufferLayout layout) => _layout = layout;
    }
}
