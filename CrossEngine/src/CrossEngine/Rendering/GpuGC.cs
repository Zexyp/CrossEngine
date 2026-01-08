using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CrossEngine.Logging;
using CrossEngine.Rendering;

namespace CrossEngine.Rendering
{
    public static class GpuGC
    {
        private static readonly Logger Log = new Logger("gpugc");

        private struct GpuObjectCreationInfo
        {
            public DateTime Time;
            public StackTrace Trace;
        }

        private static readonly Dictionary<GpuObject, GpuObjectCreationInfo> _objs = new Dictionary<GpuObject, GpuObjectCreationInfo>();
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
        
        public static void PrintCollected(bool trim = false)
        {
            lock (_objs)
                foreach (var item in _objs)
                {
                    string line = null;
                    if (trim)
                    {
                        const int Skip = 3;
                        line = "    ...\n" + string.Join("\n", item.Value.Trace.GetFrames().Skip(Skip).Select(f =>
                        {
                            var method = f.GetMethod();
                            return $"    at {method.DeclaringType.Name}.{method.Name}";
                        }));
                    }
                    else
                        line = $"{item.Value.Trace}";
                    
                    Log.Warn($"{item.Key.ToString()} at [{item.Value.Time}]:\n{line}");
                }
        }
    }
}
