using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using CrossEngine.Logging;

namespace CrossEngine.Debugging
{
    internal static class GPUGC
    {
        private static readonly Logger Log = new Logger("gpugc");

        private struct GPUObjectCreationInfo
        {
            public DateTime Time;
            public StackTrace Trace;
        }

        private static readonly Dictionary<object, GPUObjectCreationInfo> _objs = new Dictionary<object, GPUObjectCreationInfo>();

        public static void Register(object obj)
        {
            lock (_objs)
                _objs.Add(obj, new() { Time = DateTime.Now, Trace = new StackTrace() });
        }

        public static void Unregister(object obj)
        {
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
