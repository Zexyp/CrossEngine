using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Logging;

namespace CrossEngine.Assets.GC
{
    enum GPUObjectType
    {
        Texture,
        VertexBuffer,
        IndexBuffer,
        VertexArray,
        UniformBuffer,
        Framebuffer,
        Renderbuffer
    }

    public static unsafe class GPUGarbageCollector
    {
        static readonly Dictionary<GPUObjectType, List<uint>> Marked;
        static readonly Dictionary<GPUObjectType, Action<List<uint>>> Handlers = new Dictionary<GPUObjectType, Action<List<uint>>>
        {
            { GPUObjectType.Texture, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteTextures(list.Count, p);
            } },

            { GPUObjectType.VertexBuffer, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteBuffers(list.Count, p);
            } },
            { GPUObjectType.IndexBuffer, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteBuffers(list.Count, p);
            } },
            { GPUObjectType.UniformBuffer, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteBuffers(list.Count, p);
            } },

            { GPUObjectType.VertexArray, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteVertexArrays(list.Count, p);
            } },

            { GPUObjectType.Framebuffer, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteFramebuffers(list.Count, p);
            } },
            { GPUObjectType.Renderbuffer, (list) => {
                fixed (uint* p = &list.ToArray()[0])
                    glDeleteRenderbuffers(list.Count, p);
            } },
        };

        static GPUGarbageCollector()
        {
            var values = Enum.GetValues(typeof(GPUObjectType));
            Marked = new Dictionary<GPUObjectType, List<uint>>(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                Marked.Add((GPUObjectType)values.GetValue(i), new List<uint>());
            }
        }

        internal static void MarkObject(GPUObjectType objectType, uint id)
        {
            if (!Marked[objectType].Contains(id))
            {
                Marked[objectType].Add(id);
                Log.Core.Trace($"[GPUGarbageCollector] {objectType} id {id} marked");
            }
            else
                Log.Core.Error($"[GPUGarbageCollector] {objectType} id {id} already marked!");
        }

        public static void Collect()
        {
            Stopwatch sw = Stopwatch.StartNew();

            int count = 0;

            foreach (KeyValuePair<GPUObjectType, List<uint>> pair in Marked)
            {
                if (pair.Value.Count > 0)
                    Handlers[pair.Key](pair.Value);
                count += pair.Value.Count;
            }

            Log.Core.Trace($"[GPUGarbageCollector] cleared {count} objects in {sw.ElapsedMilliseconds} ms");
        }
    }
}
