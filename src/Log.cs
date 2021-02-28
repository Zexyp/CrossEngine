using System;
using static OpenGL.GL;

using System.Runtime.InteropServices;
using System.Text;

using static OpenGL.Extensions.Debug;

public static class Log
{
    public static bool? Init()
    {
        OpenGL.Extensions.Debug.Import(GLFW.Glfw.GetProcAddress);
        
        return null;
    }

    static public void Communication(string text = "", bool time = false)
    {
        if (text == "")
        {
            text = "---";
        }
        if (time)
            text = "[" + DateTime.Now.TimeOfDay.ToString().Split('.')[0] + "] " + text;

        ColoredConsole.WriteLine("[COMMU]: " + text, ConsoleColor.DarkCyan);
    }

    static public void Debug(string text)
    {
        ColoredConsole.WriteLine("[DEBUG]: " + text, ConsoleColor.DarkGray);
    }

    static public void Info(string text)
    {
        ColoredConsole.WriteLine("[INFO]: " + text, ConsoleColor.Gray);
    }

    static public void Warn(string text)
    {
        ColoredConsole.WriteLine("[WARN]: " + text, ConsoleColor.DarkYellow);
    }

    static public void Error(string text)
    {
        ColoredConsole.WriteLine("[ERROR]: " + text, ConsoleColor.Red);
    }

    static public void GLError(string text)
    {
        ColoredConsole.WriteLine("[OpenGL ERROR]: " + text, ConsoleColor.Red);
    }

    #region GL Logging

    static int level = 0;

    static private void GLMessage(int source, int type, uint id, int severity, int length, IntPtr message, IntPtr userParam)
    {
        byte[] textArray = new byte[length];
        Marshal.Copy(message, textArray, 0, length);
        string text = Encoding.ASCII.GetString(textArray);
        
        switch (severity)
        {
            case GL_DEBUG_SEVERITY_HIGH:
                {
                    if (level < 4) ColoredConsole.WriteLine("[OpenGL-high]: " + text, ConsoleColor.Red);
                }
                break;
            case GL_DEBUG_SEVERITY_MEDIUM:
                {
                    if (level < 3) ColoredConsole.WriteLine("[OpenGL-medium]: " + text, ConsoleColor.DarkYellow);
                }
                break;
            case GL_DEBUG_SEVERITY_LOW:
                {
                    if (level < 2) ColoredConsole.WriteLine("[OpenGL-low]: " + text, ConsoleColor.Green);
                }
                break;
            case GL_DEBUG_SEVERITY_NOTIFICATION:
                {
                    if (level < 1) ColoredConsole.WriteLine("[OpenGL-notification]: " + text, ConsoleColor.Gray);
                }
                break;
            default:
                {
                    ColoredConsole.WriteLine("[OpenGL]: " + text, ConsoleColor.Gray);
                }
                break;
        }
    }

    public static unsafe void EnableGLDebugging(int level = 0)
    {
        Log.level = level;
        glDebugMessageCallback(GLMessage, null);
        glEnable(GL_DEBUG_OUTPUT);
        glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
    }
    #endregion

    //####################################################################################################
    //####################################################################################################
    //####################################################################################################

    private static class ColoredConsole
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
}


