using System;
using static OpenGL.GL;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Threading;

using static OpenGL.Extensions.Debug;

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
        public static Logger App { get; private set; }
        internal static Logger Core { get; private set; }
        private static Logger GLLog;

        private static Dictionary<string, Logger> loggers = new Dictionary<string, Logger>();
        private static Mutex mutex = new Mutex();

        static bool initialized = false;
        public static void Init()
        {
            if (initialized)
                return;
            initialized = true;

            Core = new Logger("CORE");

            App = new Logger("APP");

            GLLog = new Logger("OpenGL");
            GLLog.Pattern = "[%t][%n]";

            Log.Core.Trace("log initialized");
        }

        public static Logger GetLogger(string name)
        {
            if (!loggers.ContainsKey(name))
                loggers.Add(name, new Logger(name));
            return loggers[name];
        }

        internal static void Print(LogLevel level, string format, params object?[]? arg)
        {
            mutex.WaitOne();
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

            if (arg.Length > 0) Console.WriteLine(format, arg);
            else Console.WriteLine(format);
            Console.ResetColor();
            mutex.ReleaseMutex();
        }

        //static public void Communication(string text = "", bool time = false)
        //{
        //    if (text == "")
        //    {
        //        text = "---";
        //    }
        //    if (time)
        //        text = "[" + DateTime.Now.TimeOfDay.ToString().Split('.')[0] + "] " + text;
        //
        //    ColoredConsole.WriteLine("[COMMU]: " + text, ConsoleColor.DarkCyan);
        //}

        #region GL Logging
        static public void GLError(string text)
        {
            GLLog.Error(text);
        }

        static private void GLMessage(int source, int type, uint id, int severity, int length, IntPtr message, IntPtr userParam)
        {
            byte[] textArray = new byte[length];
            Marshal.Copy(message, textArray, 0, length);
            string text = Encoding.ASCII.GetString(textArray);

            switch (severity)
            {
                case GL_DEBUG_SEVERITY_HIGH:
                    {
                        GLLog.Error("[high]: " + text);
                    }
                    break;
                case GL_DEBUG_SEVERITY_MEDIUM:
                    {
                        GLLog.Warn("[medium]: " + text);
                    }
                    break;
                case GL_DEBUG_SEVERITY_LOW:
                    {
                        GLLog.Info("[low]: " + text);
                    }
                    break;
                case GL_DEBUG_SEVERITY_NOTIFICATION:
                    {
                        GLLog.Info("[notification]: " + text);
                    }
                    break;
                default:
                    {
                        GLLog.Info(": " + text);
                    }
                    break;
            }
        }

        public static unsafe void EnableGLDebugging(LogLevel level = LogLevel.Trace)
        {
            OpenGL.Extensions.Debug.Import(GLFW.Glfw.GetProcAddress);

            GLLog.LogLevel = level;
            glDebugMessageCallback(GLMessage, null);
            glEnable(GL_DEBUG_OUTPUT);
            glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);

            Log.GLLog.Trace(": gl debuging enabled");
        }
        #endregion
    }

    public class Logger
    {
        public string Pattern = "[%t][%n][%l]: ";
        // %t - time
        // %n - name
        // %l - level

        string name = "";
        public LogLevel LogLevel;

        public Logger(string name = "undefined", LogLevel level = LogLevel.Trace)
        {
            this.name = name;
            this.LogLevel = level;
        }

        private string ConvertPattern(LogLevel level)
        {
            return Pattern
                .Replace("%t", DateTime.Now.TimeOfDay.Hours.ToString("00") + ":" + DateTime.Now.TimeOfDay.Minutes.ToString("00") + ":" + DateTime.Now.TimeOfDay.Seconds.ToString("00"))
                .Replace("%n", name)
                .Replace("%l", level.ToString());
        }

#pragma warning disable CS8632
        [Conditional("DEBUG")]
        public void Trace(string format, params object?[]? arg)
        {
            if (this.LogLevel > LogLevel.Trace) return;
            Log.Print(LogLevel.Trace, ConvertPattern(LogLevel.Trace) + format, arg);
        }

        [Conditional("DEBUG")]
        public void Debug(string format, params object?[]? arg)
        {
            if (this.LogLevel > LogLevel.Debug) return;
            Log.Print(LogLevel.Debug, ConvertPattern(LogLevel.Debug) + format, arg);
        }

        public void Info(string format, params object?[]? arg)
        {
            if (this.LogLevel > LogLevel.Info) return;
            Log.Print(LogLevel.Info, ConvertPattern(LogLevel.Info) + format, arg);
        }

        public void Warn(string format, params object?[]? arg)
        {
            if (this.LogLevel > LogLevel.Warn) return;
            Log.Print(LogLevel.Warn, ConvertPattern(LogLevel.Warn) + format, arg);
        }

        public void Error(string format, params object?[]? arg)
        {
            if (this.LogLevel > LogLevel.Error) return;
            Log.Print(LogLevel.Error, ConvertPattern(LogLevel.Error) + format, arg);
        }

        public void Fatal(string format, params object?[]? arg)
        {
            if (this.LogLevel > LogLevel.Fatal) return;
            Log.Print(LogLevel.Fatal, ConvertPattern(LogLevel.Fatal) + format, arg);
        }
#pragma warning restore CS8632
    }
}

/*
//####################################################################################################
//####################################################################################################
//####################################################################################################
static class ColoredConsole
{
    static public void Write(string text, ConsoleColor? fg = null, ConsoleColor? bg = null)
    {
        if (fg != null) Console.ForegroundColor = (ConsoleColor)fg;
        if (bg != null) Console.BackgroundColor = (ConsoleColor)bg;
        Console.Write(text);
        Console.ResetColor();
    }

    static public void WriteLine(string text, ConsoleColor? fg = null, ConsoleColor? bg = null)
    {
        if (fg != null) Console.ForegroundColor = (ConsoleColor)fg;
        if (bg != null) Console.BackgroundColor = (ConsoleColor)bg;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
*/
