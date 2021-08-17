using System;
using static OpenGL.GL;

using System.Runtime.InteropServices;
using System.Text;

using static OpenGL.Extensions.Debug;

namespace CrossEngine.Logging
{
    public static class Log
    {
        public static Logger App { get; private set; }
        internal static Logger Core { get; private set; }
        private static Logger GLLog;

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
                        GLLog.Error("[high]: " + text, ConsoleColor.Red);
                    }
                    break;
                case GL_DEBUG_SEVERITY_MEDIUM:
                    {
                        GLLog.Warn("[medium]: " + text, ConsoleColor.DarkYellow);
                    }
                    break;
                case GL_DEBUG_SEVERITY_LOW:
                    {
                        GLLog.Info("[low]: " + text, ConsoleColor.Green);
                    }
                    break;
                case GL_DEBUG_SEVERITY_NOTIFICATION:
                    {
                        GLLog.Info("[notification]: " + text, ConsoleColor.Gray);
                    }
                    break;
                default:
                    {
                        GLLog.Info(": " + text, ConsoleColor.Gray);
                    }
                    break;
            }
        }

        public static unsafe void EnableGLDebugging(Logger.Level level = Logger.Level.Trace)
        {
            OpenGL.Extensions.Debug.Import(GLFW.Glfw.GetProcAddress);

            GLLog.level = level;
            glDebugMessageCallback(GLMessage, null);
            glEnable(GL_DEBUG_OUTPUT);
            glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);

            Log.GLLog.Trace(": gl debuging enabled");
        }
        #endregion
    }

    public class Logger
    {
        public enum Level : int
        {
            Trace = 0,
            Debug = 1,
            Info = 2,
            Warn = 3,
            Error = 4,
            Fatal = 5,
            Off = 6
        }

        public string Pattern = "[%t][%n][%l]: ";
        // %t - time
        // %n - name
        // %l - level

        string name = "";
        public Level level;

        public Logger(string name = "undefined", Level level = Level.Trace)
        {
            this.name = name;
            this.level = level;
        }

        private string ConvertPattern(Level level)
        {
            return Pattern
                .Replace("%t", DateTime.Now.TimeOfDay.Hours.ToString("00") + ":" + DateTime.Now.TimeOfDay.Minutes.ToString("00") + ":" + DateTime.Now.TimeOfDay.Seconds.ToString("00"))
                .Replace("%n", name)
                .Replace("%l", level.ToString());
        }

#pragma warning disable CS8632
        private void Print(Level level, string format, params object?[]? arg)
        {
            if (this.level > level) return;

            switch (level)
            {
                case Level.Trace:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case Level.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case Level.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case Level.Warn:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case Level.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Level.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }

            if (arg.Length > 0) Console.WriteLine(ConvertPattern(level) + format, arg);
            else Console.WriteLine(ConvertPattern(level) + format);
            Console.ResetColor();
        }

        public void Trace(string format, params object?[]? arg)
        {
#if DEBUG
            Print(Level.Trace, format, arg);
#endif
        }

        public void Debug(string format, params object?[]? arg)
        {
#if DEBUG
            Print(Level.Debug, format, arg);
#endif
        }

        public void Info(string format, params object?[]? arg)
        {
            Print(Level.Info, format, arg);
        }

        public void Warn(string format, params object?[]? arg)
        {
            Print(Level.Warn, format, arg);
        }

        public void Error(string format, params object?[]? arg)
        {
            Print(Level.Error, format, arg);
        }

        public void Fatal(string format, params object?[]? arg)
        {
            Print(Level.Fatal, format, arg);
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
