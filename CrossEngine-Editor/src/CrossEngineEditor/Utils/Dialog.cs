using System;
using System.Collections.Generic;

using ShellFileDialogs;

namespace CrossEngineEditor.Utils
{
    // Not using [STAThread] can make COM behave unexpectedly.
    // " If the attribute is not present, the application uses the multithreaded apartment model, which is not supported for Windows Forms."
    public static class Dialog
    {
        public static bool FolderBrowser(out string path,
            string? title = null,
            string? initialDirectory = null)
        {
            path = FolderBrowserDialog.ShowDialog(IntPtr.Zero, title, initialDirectory);
            return path != null;
        }

        public static bool FileOpen(out string path,
            string? title = null,
            string? initialDirectory = null,
            string? defaultFileName = null)
        {
            path = FileOpenDialog.ShowSingleSelectDialog(IntPtr.Zero, title, initialDirectory, defaultFileName, null, null);
            return path != null;
        }

        public static bool FileSave(out string path,
            string? title = null,
            string? initialDirectory = null,
            string? defaultFileName = null)
        {
            path = FileSaveDialog.ShowDialog(IntPtr.Zero, title, initialDirectory, defaultFileName, null);
            return path != null;
        }
    }
}
