using System;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace CrossEngine.Logging
{
    public enum LogLevel : int
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
        Off = 6
    }

    public static class Log
    {
        public static Logger App { get; private set; } // for final application
        internal static Logger Core { get; private set; } // for engine core

        private static Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private static LogLevel _globalLevel = LogLevel.Trace;

        private static Mutex mutex = new Mutex();

        public static LogLevel GlobalLevel
        {
            get { return _globalLevel; }
            set
            {
                _globalLevel = value;
            }
        }

        static bool initialized = false;
        public static void Init()
        {
            if (initialized)
                return;
            initialized = true;

            Core = new Logger("CORE");

            App = new Logger("APP");

            Log.Core.Trace("log initialized");
        }

        public static Logger GetLogger(string name)
        {
            if (!_loggers.ContainsKey(name))
                _loggers.Add(name, new Logger(name));
            return _loggers[name];
        }

        internal static void Print(LogLevel level, string message)
        {
            mutex.WaitOne();

            if (_globalLevel <= level)
            {
                switch (level)
                {
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warn:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                }

                Console.WriteLine(message);
                Console.ResetColor();
            }
            
            mutex.ReleaseMutex();
        }
    }

    public class Logger
    {
        public string Pattern = "[%t][%n][%l]: ";
        // %t - time
        // %n - name
        // %l - level
        public string DateTimeFormat = "HH:mm:ss";

        public readonly string Name = "";
        public LogLevel LogLevel;

        public Logger(string name, LogLevel level = LogLevel.Trace)
        {
            this.Name = name;
            this.LogLevel = level;
        }

        protected string FillPattern(LogLevel level)
        {
            return Pattern
                .Replace("%t", DateTime.Now.ToString(DateTimeFormat))
                .Replace("%n", Name)
                .Replace("%l", level.ToString());
        }

#nullable enable
        [Conditional("DEBUG")]
        public virtual void Trace(string format, params object?[] args)
        {
            if (this.LogLevel > LogLevel.Trace) return;
            Log.Print(LogLevel.Trace,
                FillPattern(LogLevel.Trace) +
                (args?.Length != 0 ? String.Format(format, args) : format));
        }

        [Conditional("DEBUG")]
        public virtual void Debug(string format, params object?[] args)
        {
            if (this.LogLevel > LogLevel.Debug) return;
            Log.Print(LogLevel.Debug,
                FillPattern(LogLevel.Debug) +
                (args?.Length != 0 ? String.Format(format, args) : format));
        }

        public virtual void Info(string format, params object?[] args)
        {
            if (this.LogLevel > LogLevel.Info) return;
            Log.Print(LogLevel.Info,
                FillPattern(LogLevel.Info) +
                (args?.Length != 0 ? String.Format(format, args) : format));
        }

        public virtual void Warn(string format, params object?[] args)
        {
            if (this.LogLevel > LogLevel.Warn) return;
            Log.Print(LogLevel.Warn,
                FillPattern(LogLevel.Warn) +
                (args?.Length != 0 ? String.Format(format, args) : format));
        }

        public virtual void Error(string format, params object?[] args)
        {
            if (this.LogLevel > LogLevel.Error) return;
            Log.Print(LogLevel.Error,
                FillPattern(LogLevel.Error) +
                (args?.Length != 0 ? String.Format(format, args) : format));
        }

        public virtual void Fatal(string format, params object?[] args)
        {
            if (this.LogLevel > LogLevel.Fatal) return;
            Log.Print(LogLevel.Fatal,
                FillPattern(LogLevel.Fatal) +
                (args?.Length != 0 ? String.Format(format, args) : format));
        }
#nullable restore
    }
}
