using System;
using System.Collections.Generic;

using ShellFileDialogs;

namespace CrossEngineEditor.Utils
{
    // Not using [STAThread] can make COM behave unexpectedly.
    // " If the attribute is not present, the application uses the multithreaded apartment model, which is not supported for Windows Forms."
    public static class Dialog
    {
        public static bool FolderBrowser(out string path)
        {
            path = FolderBrowserDialog.ShowDialog(IntPtr.Zero, null, null);
            return path != null;
        }

        public static bool FileOpen(out string path)
        {
            path = FileOpenDialog.ShowSingleSelectDialog(IntPtr.Zero, null, null, null, null, null);
            return path != null;
        }

        public static bool FileSave(out string path)
        {
            path = FileSaveDialog.ShowDialog(IntPtr.Zero, null, null, null, null);
            return path != null;
        }
    }
}
