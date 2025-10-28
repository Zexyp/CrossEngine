using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using CrossEngine.Logging;
using CrossEngine.Rendering;

namespace CrossEngine.Debugging
{
    public static class GpuGC
    {
        private static readonly Logger Log = new Logger("gpugc");

        private struct GPUObjectCreationInfo
        {
            public DateTime Time;
            public StackTrace Trace;
        }

        private static readonly Dictionary<GpuObject, GPUObjectCreationInfo> _objs = new Dictionary<GpuObject, GPUObjectCreationInfo>();
        private static readonly ConcurrentQueue<GpuObject> _toBeDestroyed = new();

        internal static void Destroy(GpuObject obj)
        {
            _toBeDestroyed.Enqueue(obj);
        }
        
        internal static void Collect()
        {
            while (_toBeDestroyed.TryDequeue(out var obj))
                obj.Destroy();
        }
        
        internal static void Register(GpuObject obj)
        {
            Log.Trace($"registering '{obj.GetType().Name}'");
            lock (_objs)
                _objs.Add(obj, new() { Time = DateTime.Now, Trace = new StackTrace() });
        }

        internal static void Unregister(GpuObject obj)
        {
            Log.Trace($"unregistering '{obj.GetType().Name}'");
            lock (_objs)
                _objs.Remove(obj);
        }
        
        public static void PrintCollected()
        {
            lock (_objs)
                foreach (var item in _objs)
                {
                    Log.Warn($"{item.Key.ToString()} at [{item.Value.Time}]:\n{item.Value.Trace.ToString()}");
                }
        }
    }
}
