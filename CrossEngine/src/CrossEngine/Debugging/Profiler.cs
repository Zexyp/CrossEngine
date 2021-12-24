using System;

using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

using CrossEngine.Logging;

namespace CrossEngine.Profiling
{
    public class Profiler
    {
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
        }

        static Mutex mutex = new Mutex();
        static FileStream outputStream;
        static StreamWriter streamWriter;
        static ProfilerSession? currentSession = null;
        static readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };
        static DateTime appStarted = DateTime.Now;

        #region Public Methods
        [Conditional("PROFILING")]
        public static void BeginSession(string name, string filepath)
        {
            mutex.WaitOne();

            Log.Init();

            profileResultsStack = new Stack<ProfileResult>();

            if (currentSession != null)
            {
                Log.Core.Error("[profiler] session was already created");
                return;
            }

            try
            {
                outputStream = File.Open(filepath, FileMode.Create);
            }
            catch (IOException)
            {
                Log.Core.Error("[profiler] file cannot be created");
                return;
            }

            streamWriter = new StreamWriter(outputStream);

            currentSession = new ProfilerSession(name);
            WriteHeader();

            Log.Core.Info("[profiler] profiling session started");

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void EndSession()
        {
            mutex.WaitOne();

            InternalEndSession();
            Log.Core.Info("[profiler] profiling session ended");

            mutex.ReleaseMutex();
        }

        static Stack<ProfileResult> profileResultsStack;

        [Conditional("PROFILING")]
        public static void BeginScope(string name)
        {
            if (currentSession == null)
            {
                Log.Core.Error($"[profiler] no session to profile scope '{name}'");
                return;
            }

            ProfileResult result = new ProfileResult(name, Thread.CurrentThread.ManagedThreadId, (float)(DateTime.Now - appStarted).Ticks / TimeSpan.TicksPerMillisecond);

            mutex.WaitOne();

            profileResultsStack.Push(result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void BeginScope()
        {
            string name = new StackTrace().GetFrame(1).GetMethod().Name;

            if (currentSession == null)
            {
                Log.Core.Error($"[profiler] no session to profile scope '{name}'");
                return;
            }

            ProfileResult result = new ProfileResult(name, Thread.CurrentThread.ManagedThreadId, (float)(DateTime.Now - appStarted).Ticks / TimeSpan.TicksPerMillisecond);

            mutex.WaitOne();

            profileResultsStack.Push(result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void EndScope()
        {
            if (currentSession == null)
            {
                Log.Core.Error("[profiler] no session to end a scope");
                return;
            }

            mutex.WaitOne();

            ProfileResult result = profileResultsStack.Pop();
            result.ElapsedTime = ((float)(DateTime.Now - appStarted).Ticks / TimeSpan.TicksPerMillisecond) - result.Start;
            WriteProfile(ref result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void Function(string name)
        {
            if (currentSession == null)
            {
                Log.Core.Error($"[profiler] no session to profile function '{name}'");
                return;
            }

            mutex.WaitOne();

            ProfileResult result = new ProfileResult(name, Thread.CurrentThread.ManagedThreadId, (float)(DateTime.Now - appStarted).Ticks / TimeSpan.TicksPerMillisecond);
            WriteProfile(ref result);

            mutex.ReleaseMutex();
        }

        [Conditional("PROFILING")]
        public static void Function()
        {
            string name = new StackTrace().GetFrame(1).GetMethod().Name;

            if (currentSession == null)
            {
                Log.Core.Error($"[profiler] no session to profile function '{name}'");
                return;
            }

            mutex.WaitOne();

            ProfileResult result = new ProfileResult(name, Thread.CurrentThread.ManagedThreadId, (float)(DateTime.Now - appStarted).Ticks / TimeSpan.TicksPerMillisecond);
            WriteProfile(ref result);

            mutex.ReleaseMutex();
        }
        #endregion

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
    }
}
