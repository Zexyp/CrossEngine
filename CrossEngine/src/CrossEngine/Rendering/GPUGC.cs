﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using CrossEngine.Logging;

namespace CrossEngine.Debugging
{
    public static class GPUGC
    {
        private static readonly Logger Log = new Logger("gpugc");

        private struct GPUObjectCreationInfo
        {
            public DateTime Time;
            public StackTrace Trace;
        }

        private static readonly Dictionary<IDisposable, GPUObjectCreationInfo> _objs = new Dictionary<IDisposable, GPUObjectCreationInfo>();

        internal static void Register(IDisposable obj)
        {
            Log.Trace($"registering '{obj.GetType().Name}'");
            lock (_objs)
                _objs.Add(obj, new() { Time = DateTime.Now, Trace = new StackTrace() });
        }

        internal static void Unregister(IDisposable obj)
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

        internal static void Collect()
        {
            lock (_objs)
            {
                Log.Trace("disposing...");
                int c = _objs.Count;
                foreach (var item in _objs)
                {
                    item.Key.Dispose();
                }
                Log.Trace($"disposed {c} objects");
            }    
        }
    }
}
