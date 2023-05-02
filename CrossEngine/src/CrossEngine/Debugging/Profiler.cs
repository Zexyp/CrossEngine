using System;

using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;

using CrossEngine.Logging;

namespace CrossEngine.Profiling
{
    public static class Profiler
    {
        static readonly Logger Log = new Logger("profiler");

        #region Structures
        struct ProfilerSession
        {
            public string Name;

            public ProfilerSession(string name)
            {
                Name = name;
            }
        }

        public struct ProfileResult
        {
            public string Name;

            public double Start;
            public double ElapsedTime;
            public int ThreadID;

            public ProfileResult(string name, int tid, double start) : this()
            {
                Name = name;
                ThreadID = tid;
                Start = start;
            }

            public override string ToString()
            {
                return $"ThreadID: {ThreadID}; Name: {Name}";
            }
        }
        #endregion

        #region Private Fields
        static Mutex mutex = new Mutex();
        static FileStream outputStream;
        static StreamWriter streamWriter;
        static ProfilerSession? currentSession = null;
        static readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };
        static Stopwatch stopwatch = Stopwatch.StartNew();
        static ConcurrentDictionary<int, Stack<ProfileResult>> profileResultsStacks = new ConcurrentDictionary<int, Stack<ProfileResult>>();
        #endregion

        #region Public Methods
        [Conditional("PROFILING")]
        public static void BeginSession(string name, string filepath)
        {
            mutex.WaitOne();

            if (currentSession != null)
            {
                Log.Error("session was already created");
                return;
            }

            try
            {
                outputStream = File.Open(filepath, FileMode.Create, FileAccess.Write, FileShare.Read);
            }
            catch (IOException)
            {
                Log.Error("file cannot be created");
                return;
            }

            streamWriter = new StreamWriter(outputStream);

            currentSession = new ProfilerSession(name);
            WriteHeader();

            Log.Info("profiling session started");

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void EndSession()
        {
            if (AreProfileResultsEmpty()) Log.Trace("waiting for threads to finish scopes");
            SpinWait.SpinUntil(AreProfileResultsEmpty);

            mutex.WaitOne();

            InternalEndSession();
            Log.Info("profiling session ended");

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void BeginScope(string name)
        {
            if (currentSession == null)
            {
                //Log.Error($"no session to profile a scope '{name}'");
                return;
            }

            int ctid = Thread.CurrentThread.ManagedThreadId;
            ProfileResult result = new ProfileResult(name, ctid, stopwatch.Elapsed.TotalMilliseconds);

            mutex.WaitOne();

            GetThreadProfileResultsStack(ctid).Push(result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void BeginScope()
        {
            var mth = new StackTrace().GetFrame(1).GetMethod();
            string name = $"{mth.ReflectedType.Name}.{mth.Name}";

            BeginScope(name);
        }

        [Conditional("PROFILING")]
        public static void EndScope()
        {
            if (currentSession == null)
            {
                //Log.Error("no session to end a scope");
                return;
            }

            mutex.WaitOne();

            ProfileResult result = GetThreadProfileResultsStack(Thread.CurrentThread.ManagedThreadId).Pop();
            result.ElapsedTime = stopwatch.Elapsed.TotalMilliseconds - result.Start;
            WriteProfile(ref result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void Function(string name)
        {
            if (currentSession == null)
            {
                //Log.Error($"no session to profile a function '{name}'");
                return;
            }

            mutex.WaitOne();

            ProfileResult result = new ProfileResult(name, Thread.CurrentThread.ManagedThreadId, stopwatch.Elapsed.TotalMilliseconds);
            WriteProfile(ref result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void Function()
        {
            var mth = new StackTrace().GetFrame(1).GetMethod();
            string name = $"{mth.ReflectedType.Name}.{mth.Name}";

            Function(name);
        }
        #endregion

        #region Private Methods
        private static Stack<ProfileResult> GetThreadProfileResultsStack(int currentThreadId)
        {
            if (!profileResultsStacks.ContainsKey(currentThreadId))
                Debug.Assert(profileResultsStacks.TryAdd(currentThreadId, new Stack<ProfileResult>()), "this should work");
            return profileResultsStacks[currentThreadId];
        }

        private static bool AreProfileResultsEmpty()
        {
            foreach (var stack in profileResultsStacks.Values) if (stack.Count > 0) return false;
            return true;
        }

        private static void WriteProfile(ref ProfileResult result)
		{
			string json = "";

			json += ",{";
			json += "\"cat\":\"function\",";
			json += "\"dur\":" + result.ElapsedTime.ToString(nfi) + ',';
			json += "\"name\":\"" + result.Name + "\",";
			json += "\"ph\":\"X\",";
			json += "\"pid\":0,";
			json += "\"tid\":" + result.ThreadID + ",";
			json += "\"ts\":" + result.Start.ToString(nfi);
			json += "}";

			if (currentSession != null)
			{
                streamWriter.Write(json);
                streamWriter.Flush();
                outputStream.Flush();
            }
		}

        private static void WriteHeader()
        {
            streamWriter.Write("{\"otherData\": {},\"traceEvents\":[{}");
            streamWriter.Flush();
            outputStream.Flush();
        }

        private static void WriteFooter()
        {
            streamWriter.Write("]}");
            streamWriter.Flush();
            outputStream.Flush();
        }

        private static void InternalEndSession()
        {
            if (currentSession != null)
            {
                WriteFooter();
                streamWriter.Flush();
                outputStream.Flush();
                streamWriter.Close();

                streamWriter.Dispose();
                outputStream.Dispose();

                currentSession = null;
            }
        }
        #endregion
    }
}
