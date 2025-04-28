#if WINDOWS || LINUX
#define PASTEL
#endif

using System;

using System.Diagnostics;
using System.Drawing;
using System.IO;

#if PASTEL
using Pastel;
#endif

#if WASM
using CrossEngine.Platform.Wasm;
#endif

namespace CrossEngine.Logging
{
    // TODO: backgroud colors

    public enum LogLevel : int
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
        Off = 6,
        Stfu = Off
    }

    public static class Log
    {
        public static readonly Logger Default = new Logger("default");

        public static LogLevel GlobalLevel;
        // https://no-color.org/
        public static bool EnableColors = Environment.GetEnvironmentVariable("NO_COLOR") == null && !Console.IsOutputRedirected;

        public static TextWriter WriterOut = Console.Out;
        public static TextWriter WriterError = Console.Error;

        //private static Mutex mutex = new Mutex();
        
        public static void Init(LogLevel? level = null, bool? enableColors = null)
        {
            GlobalLevel = level ?? GlobalLevel;
            EnableColors = enableColors ?? EnableColors;

            Default.Trace("configured log");
            if (!EnableColors)
                Default.Trace("colors disabled");
        }

        public static void Print(LogLevel level, string message, uint? color = null)
        {
            if (GlobalLevel > level)
                return;
            
            //mutex.WaitOne();

#if WINDOWS || LINUX

#if !PASTEL
#warning Custom colors not supported
            ConsoleColor? consloeColor = Log.EnableColors ? (color != null ? (ConsoleColor)color : level switch
            {
                LogLevel.Trace => ConsoleColor.DarkGray,
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Warn => ConsoleColor.DarkYellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => (ConsoleColor)color
            }) : null;
            if (consloeColor != null)
                Console.ForegroundColor = consloeColor.Value;
#else
            color = Log.EnableColors ? (color != null ? color : level switch
            {
                LogLevel.Trace => 0x7F7F7F,
                LogLevel.Debug => 0x357EC7,
                LogLevel.Info => 0x1FB311,
                LogLevel.Warn => 0xFFC20E,
                LogLevel.Error => 0xF36F2C,
                LogLevel.Fatal => 0xCA3431,
                _ => null
            }) : null;
            if (color != null)
                message = message.Pastel(Color.FromArgb(unchecked((int)color)));
#endif

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                case LogLevel.Warn:
                    WriterOut.WriteLineAsync(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    WriterError.WriteLineAsync(message);
                    break;
                default:
                    WriterOut.WriteLineAsync(message);
                    break;
            }

#if !PASTEL
            if (consloeColor != null)
                Console.ResetColor();
#endif

#elif WASM
            static string ToHex(uint color)
            {
                var r = (byte)color;
                var g = (byte)(color >> 8);
                var b = (byte)(color >> 16);
                var a = (byte)(color >> 24);
                return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
            }

            if (color != null)
                if (color.Value < 0x01000000)
                    color += 0xff000000;
            string style = Log.EnableColors ? (color != null ? ToHex(color.Value) : level switch
            {
                LogLevel.Trace => "gray",
                LogLevel.Debug => "turquoise",
                _ => null
            }) : null;
            if (style != null)
            {
                style = $"color: {style};";
                message = "%c" + message;
            }
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    if (style == null) Interop.Console.Debug(message);
                    else Interop.Console.Debug(message, style);
                    break;
                case LogLevel.Info:
                    if (style == null) Interop.Console.Info(message);
                    else Interop.Console.Info(message, style);
                    break;
                case LogLevel.Warn:
                    if (style == null) Interop.Console.Warn(message);
                    else Interop.Console.Warn(message, style);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    if (style == null) Interop.Console.Error(message);
                    else Interop.Console.Error(message, style);
                    break;
                default:
                    if (style == null) Interop.Console.Log(message);
                    else Interop.Console.Log(message, style);
                    break;
            }

#else
#error Ooof, there is no logging backend
#endif

            //mutex.ReleaseMutex();
        }
    }

    public class Logger
    {
        // %t - time
        // %n - name
        // %l - level
        // %m - message
        public string Pattern = "[%t][%n][%l]: %m";
        public string DateTimeFormat = "HH:mm:ss";

        public string Name = "";
        public LogLevel Level;
        public uint? Color; // ARGB (0xAARRGGBB)

        public Logger(string name, LogLevel level = LogLevel.Trace)
        {
            this.Name = name;
            this.Level = level;
        }

        protected string FillPattern(LogLevel level, string message)
        {
            return Pattern
                .Replace("%t", DateTime.Now.ToString(DateTimeFormat))
                .Replace("%n", Name)
                .Replace("%l", level.ToString())
                .Replace("%m", message);
        }

#nullable enable
        [Conditional("DEBUG")]
        public virtual void Trace(string format, params object?[] args)
        {
            if (this.Level > LogLevel.Trace) return;
            Log.Print(LogLevel.Trace,
                FillPattern(LogLevel.Trace, (args?.Length != 0 ? String.Format(format, args) : format)),
                Color);
        }

        [Conditional("DEBUG")]
        public virtual void Debug(string format, params object?[] args)
        {
            if (this.Level > LogLevel.Debug) return;
            Log.Print(LogLevel.Debug,
                FillPattern(LogLevel.Debug, (args?.Length != 0 ? String.Format(format, args) : format)),
                Color);
        }

        public virtual void Info(string format, params object?[] args)
        {
            if (this.Level > LogLevel.Info) return;
            Log.Print(LogLevel.Info,
                FillPattern(LogLevel.Info, (args?.Length != 0 ? String.Format(format, args) : format)),
                Color);
        }

        public virtual void Warn(string format, params object?[] args)
        {
            if (this.Level > LogLevel.Warn) return;
            Log.Print(LogLevel.Warn,
                FillPattern(LogLevel.Warn, (args?.Length != 0 ? String.Format(format, args) : format)),
                Color);
        }

        public virtual void Error(string format, params object?[] args)
        {
            if (this.Level > LogLevel.Error) return;
            Log.Print(LogLevel.Error,
                FillPattern(LogLevel.Error, (args?.Length != 0 ? String.Format(format, args) : format)),
                Color);
        }

        public virtual void Fatal(string format, params object?[] args)
        {
            if (this.Level > LogLevel.Fatal) return;
            Log.Print(LogLevel.Fatal,
                FillPattern(LogLevel.Fatal, (args?.Length != 0 ? String.Format(format, args) : format)),
                Color);
        }
#nullable restore
    }
}
