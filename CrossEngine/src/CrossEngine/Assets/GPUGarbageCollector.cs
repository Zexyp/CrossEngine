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
        public static int ClearEveryMiliseconds = 5000;

        static readonly Dictionary<GPUObjectType, List<uint>> _marked = new Dictionary<GPUObjectType, List<uint>>(Array.ConvertAll(
                                                                                        Enum.GetValues(typeof(GPUObjectType))
                                                                                            .Cast<int>()
                                                                                            .Distinct()
                                                                                            .ToArray(),
                                                                                        (val) => new KeyValuePair<GPUObjectType, List<uint>>((GPUObjectType)val, new List<uint>())));
        static readonly Dictionary<GPUObjectType, Action<List<uint>>> _handlers = new Dictionary<GPUObjectType, Action<List<uint>>>
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
            EventLoop.CallLater(ClearEveryMiliseconds, ScheduledCollectCallback);
        }

        private static void ScheduledCollectCallback()
        {
            Collect();
            EventLoop.CallLater(ClearEveryMiliseconds, ScheduledCollectCallback);
        }

        internal static void MarkObject(GPUObjectType objectType, uint id)
        {
            if (!_marked[objectType].Contains(id))
            {
                _marked[objectType].Add(id);
                Log.Core.Trace($"[GPU GC] {objectType} id {id} marked");
            }
            else
                Log.Core.Error($"[GPU GC] {objectType} id {id} already marked!");
        }

        public static void Collect()
        {
            Stopwatch sw = Stopwatch.StartNew();

            int count = 0;

            foreach (KeyValuePair<GPUObjectType, List<uint>> pair in _marked)
            {
                if (pair.Value.Count > 0)
                {
                    count += pair.Value.Count;
                    _handlers[pair.Key](pair.Value);
                    pair.Value.Clear();
                }
            }

            if (count > 0) Log.Core.Trace($"[GPU GC] cleared {count} objects in {sw.ElapsedMilliseconds} ms");
        }
    }
}
