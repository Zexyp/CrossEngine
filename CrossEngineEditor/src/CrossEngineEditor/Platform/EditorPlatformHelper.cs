#define NATIVEFILEDIALOGSHARP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CrossEngine.Logging;


#if NATIVEFILEDIALOGSHARP
using NativeFileDialogSharp;
#else
using ShellFileDialogs;
#endif

namespace CrossEngineEditor.Platform
{
    static class EditorPlatformHelper
    {
        static Logger _log = new Logger("editor-platform");
        public static string FileSaveDialog()
        {
            _log.Trace("opening file save dialog");
#if NATIVEFILEDIALOGSHARP
            var result = Dialog.FileSave();
            Debug.Assert(!result.IsError);
            return result.Path;
#else
            return ShellFileDialogs.FileSaveDialog.ShowDialog(0, null, null, null, null);
#endif
        }

        public static string FileOpenDialog()
        {
            _log.Trace("opening file open dialog");
#if NATIVEFILEDIALOGSHARP
            var result = Dialog.FileOpen();
            Debug.Assert(!result.IsError);
            return result.Path;
#else
            return ShellFileDialogs.FileOpenDialog.ShowSingleSelectDialog(0, null, null, null, null, null);
#endif
        }
    }
}
