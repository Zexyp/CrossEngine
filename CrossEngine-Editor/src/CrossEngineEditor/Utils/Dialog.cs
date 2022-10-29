using System;
using System.Collections.Generic;

using ShellFileDialogs;

namespace CrossEngineEditor.Utils
{
    // Not using [STAThread] can make COM behave unexpectedly.
    // " If the attribute is not present, the application uses the multithreaded apartment model, which is not supported for Windows Forms."
    public static class Dialog
    {
        public static class Filters
        {
            public static readonly Filter AllFiles = new Filter("All Files", "*");
            public static readonly Filter JsonFile = new Filter("JSON File", "json");
            public static readonly Filter IniFile = new Filter("INI File", "ini");
            public static readonly Filter ImageFiles = new Filter("All Image Files", "png", "jpg", "jpeg", "bmp", "gif", "tif", "tiff", "exif");
        }

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
            string? defaultFileName = null,
            params Filter[] filters)
        {
            if (filters.Length == 0)
                filters = null;

            path = FileOpenDialog.ShowSingleSelectDialog(IntPtr.Zero, title, initialDirectory, defaultFileName, filters, null);
            return path != null;
        }

        public static bool FileSave(out string path,
            string? title = null,
            string? initialDirectory = null,
            string? defaultFileName = null,
            params Filter[] filters)
        {
            if (filters.Length == 0)
                filters = null;

            path = FileSaveDialog.ShowDialog(IntPtr.Zero, title, initialDirectory, defaultFileName, filters);
            return path != null;
        }
    }
}
