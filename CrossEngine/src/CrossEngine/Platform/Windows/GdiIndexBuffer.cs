using CrossEngine.Debugging;
using CrossEngine.Rendering.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    unsafe class GdiIndexBuffer : IndexBuffer
    {
        internal UnmanagedMemoryStream stream;
        void* streamData = null;

        public unsafe GdiIndexBuffer(void* indices, uint count, IndexDataType dataType)
        {
            GC.KeepAlive(this);
            GPUGC.Register(this);

            DataType = dataType;
            Count = count;

            SetData(indices, count);
        }

        public override void Bind()
        {
            state.ib = this;
        }

        public override void Unbind()
        {
            state.ib = null;
        }

        public override unsafe void SetData(void* data, uint count, uint offset = 0)
        {
            Debug.Assert(offset == 0);

            if (streamData != null)
                NativeMemory.Free(streamData);
            stream?.Dispose();
            if (data != null)
            {
                uint length = Count * GetIndexDataTypeSize(DataType);
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

        private static uint GetIndexDataTypeSize(IndexDataType dataType)
        {
            switch (dataType)
            {
                case IndexDataType.UInt: return 4;
                case IndexDataType.UShort: return 2;
                case IndexDataType.UByte: return 1;
            }

            Debug.Assert(false, $"Unknown {nameof(IndexDataType)} value");
            return 0;
        }
    }
}
