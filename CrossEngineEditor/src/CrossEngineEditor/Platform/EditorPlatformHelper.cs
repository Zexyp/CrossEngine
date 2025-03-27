using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS
using ShellFileDialogs;
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
#error Nope
#endif
        }

        public static string FileOpenDialog()
        {
#if WINDOWS
            return ShellFileDialogs.FileOpenDialog.ShowSingleSelectDialog(0, null, null, null, null, null);
#elif LINUX
#error Nope
#endif
        }
    }
}
