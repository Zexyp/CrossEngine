using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS
using ShellFileDialogs;
#elif LINUX
using System.Diagnostics;
using NativeFileDialogSharp;
#endif

namespace CrossEngineEditor.Platform
{
    static class EditorPlatformHelper
    {
        public static string FileSaveDialog()
        {
#if WINDOWS
            return ShellFileDialogs.FileSaveDialog.ShowDialog(0, null, null, null, null);
#elif LINUX
            var result = Dialog.FileSave();
            Debug.Assert(!result.IsError);
            return result.Path;
#endif
        }

        public static string FileOpenDialog()
        {
#if WINDOWS
            return ShellFileDialogs.FileOpenDialog.ShowSingleSelectDialog(0, null, null, null, null, null);
#elif LINUX
            var result = Dialog.FileOpen();
            Debug.Assert(!result.IsError);
            return result.Path;
#endif
        }
    }
}
