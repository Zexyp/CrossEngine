using CrossEngine.Debugging;
using CrossEngine.Rendering.Buffers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    unsafe class GdiVertexBuffer : VertexBuffer
    {
        BufferLayout _layout;
        internal UnmanagedMemoryStream stream;
        void* streamData;

        public unsafe GdiVertexBuffer(void* data, uint size)
        {
            GC.KeepAlive(this);
            GPUGC.Register(this);

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
            Debug.Assert(offset == 0);

            if (streamData != null)
                NativeMemory.Free(streamData);
            stream?.Dispose();
            if (data != null)
            {
                uint length = size;
                streamData = NativeMemory.Alloc(length);
                NativeMemory.Copy(data, streamData, length);
                stream = new UnmanagedMemoryStream((byte*)streamData, length);
            }
            else
            {
                streamData = null;
                stream = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            stream?.Dispose();
            stream = null;
        }

        public override BufferLayout GetLayout() => _layout;
        public override void SetLayout(BufferLayout layout) => _layout = layout;
    }
}
