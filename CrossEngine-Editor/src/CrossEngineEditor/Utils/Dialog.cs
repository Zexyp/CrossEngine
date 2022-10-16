using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace CrossEngineEditor.Utils
{
    public class Dialog
    {
        public class FolderPicker
        {
            public virtual string ResultPath { get; protected set; }
            public virtual string ResultName { get; protected set; }
            public virtual string InputPath { get; set; }
            public virtual bool ForceFileSystem { get; set; }
            public virtual string Title { get; set; }
            public virtual string OkButtonLabel { get; set; }
            public virtual string FileNameLabel { get; set; }

            protected virtual int SetOptions(int options)
            {
                if (ForceFileSystem)
                {
                    options |= (int)FOS.FOS_FORCEFILESYSTEM;
                }
                return options;
            }

            // for all .NET
            public virtual bool? ShowDialog(IntPtr owner, bool throwOnError = false)
            {
                var dialog = (IFileOpenDialog)new FileOpenDialog();
                if (!string.IsNullOrEmpty(InputPath))
                {
                    if (CheckHr(SHCreateItemFromParsingName(InputPath, null, typeof(IShellItem).GUID, out var item), throwOnError) != 0)
                        return null;

                    dialog.SetFolder(item);
                }

                var options = FOS.FOS_PICKFOLDERS;
                options = (FOS)SetOptions((int)options);
                dialog.SetOptions(options);

                if (Title != null)
                {
                    dialog.SetTitle(Title);
                }

                if (OkButtonLabel != null)
                {
                    dialog.SetOkButtonLabel(OkButtonLabel);
                }

                if (FileNameLabel != null)
                {
                    dialog.SetFileName(FileNameLabel);
                }

                if (owner == IntPtr.Zero)
                {
                    owner = Process.GetCurrentProcess().MainWindowHandle;
                    if (owner == IntPtr.Zero)
                    {
                        owner = GetDesktopWindow();
                    }
                }

                var hr = dialog.Show(owner);
                if (hr == ERROR_CANCELLED)
                    return null;

                if (CheckHr(hr, throwOnError) != 0)
                    return null;

                if (CheckHr(dialog.GetResult(out var result), throwOnError) != 0)
                    return null;

                if (CheckHr(result.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out var path), throwOnError) != 0)
                    return null;

                ResultPath = path;

                if (CheckHr(result.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out path), false) == 0)
                {
                    ResultName = path;
                }
                return true;
            }

            private static int CheckHr(int hr, bool throwOnError)
            {
                if (hr != 0)
                {
                    if (throwOnError)
                        Marshal.ThrowExceptionForHR(hr);
                }
                return hr;
            }

            [DllImport("shell32")]
            private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

            [DllImport("user32")]
            private static extern IntPtr GetDesktopWindow();

#pragma warning disable IDE1006 // Naming Styles
            private const int ERROR_CANCELLED = unchecked((int)0x800704C7);
#pragma warning restore IDE1006 // Naming Styles

            [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
            private class FileOpenDialog
            {
            }

            [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IFileOpenDialog
            {
                [PreserveSig] int Show(IntPtr parent); // IModalWindow
                [PreserveSig] int SetFileTypes();  // not fully defined
                [PreserveSig] int SetFileTypeIndex(int iFileType);
                [PreserveSig] int GetFileTypeIndex(out int piFileType);
                [PreserveSig] int Advise(); // not fully defined
                [PreserveSig] int Unadvise();
                [PreserveSig] int SetOptions(FOS fos);
                [PreserveSig] int GetOptions(out FOS pfos);
                [PreserveSig] int SetDefaultFolder(IShellItem psi);
                [PreserveSig] int SetFolder(IShellItem psi);
                [PreserveSig] int GetFolder(out IShellItem ppsi);
                [PreserveSig] int GetCurrentSelection(out IShellItem ppsi);
                [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
                [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
                [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
                [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
                [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
                [PreserveSig] int GetResult(out IShellItem ppsi);
                [PreserveSig] int AddPlace(IShellItem psi, int alignment);
                [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
                [PreserveSig] int Close(int hr);
                [PreserveSig] int SetClientGuid();  // not fully defined
                [PreserveSig] int ClearClientData();
                [PreserveSig] int SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);
                [PreserveSig] int GetResults([MarshalAs(UnmanagedType.IUnknown)] out object ppenum);
                [PreserveSig] int GetSelectedItems([MarshalAs(UnmanagedType.IUnknown)] out object ppsai);
            }

            [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IShellItem
            {
                [PreserveSig] int BindToHandler(); // not fully defined
                [PreserveSig] int GetParent(); // not fully defined
                [PreserveSig] int GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
                [PreserveSig] int GetAttributes();  // not fully defined
                [PreserveSig] int Compare();  // not fully defined
            }

#pragma warning disable CA1712 // Do not prefix enum values with type name
            private enum SIGDN : uint
            {
                SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
                SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
                SIGDN_FILESYSPATH = 0x80058000,
                SIGDN_NORMALDISPLAY = 0,
                SIGDN_PARENTRELATIVE = 0x80080001,
                SIGDN_PARENTRELATIVEEDITING = 0x80031001,
                SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
                SIGDN_PARENTRELATIVEPARSING = 0x80018001,
                SIGDN_URL = 0x80068000
            }

            [Flags]
            private enum FOS
            {
                FOS_OVERWRITEPROMPT = 0x2,
                FOS_STRICTFILETYPES = 0x4,
                FOS_NOCHANGEDIR = 0x8,
                FOS_PICKFOLDERS = 0x20,
                FOS_FORCEFILESYSTEM = 0x40,
                FOS_ALLNONSTORAGEITEMS = 0x80,
                FOS_NOVALIDATE = 0x100,
                FOS_ALLOWMULTISELECT = 0x200,
                FOS_PATHMUSTEXIST = 0x800,
                FOS_FILEMUSTEXIST = 0x1000,
                FOS_CREATEPROMPT = 0x2000,
                FOS_SHAREAWARE = 0x4000,
                FOS_NOREADONLYRETURN = 0x8000,
                FOS_NOTESTFILECREATE = 0x10000,
                FOS_HIDEMRUPLACES = 0x20000,
                FOS_HIDEPINNEDPLACES = 0x40000,
                FOS_NODEREFERENCELINKS = 0x100000,
                FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
                FOS_DONTADDTORECENT = 0x2000000,
                FOS_FORCESHOWHIDDEN = 0x10000000,
                FOS_DEFAULTNOMINIMODE = 0x20000000,
                FOS_FORCEPREVIEWPANEON = 0x40000000,
                FOS_SUPPORTSTREAMABLEITEMS = unchecked((int)0x80000000)
            }
#pragma warning restore CA1712 // Do not prefix enum values with type name
        }

        // black box content provided by:
        // https://stackoverflow.com/questions/9088227/using-getopenfilename-instead-of-openfiledialog

        internal class Native
        {
            public const int MAX_PATH = 260,
                MAX_UNICODESTRING_LEN = short.MaxValue,
                CSIDL_DESKTOP = 0x0000,
                BFFM_SETSELECTIONA = 0x400 + 102,
                BFFM_SETSELECTIONW = 0x400 + 103,
                BFFM_INITIALIZED = 1,
                BFFM_SELCHANGED = 2,
                BFFM_ENABLEOK = 0x400 + 101;
            public static int BFFM_SETSELECTION => Marshal.SystemDefaultCharSize == 1 ? BFFM_SETSELECTIONA : BFFM_SETSELECTIONW;

            [Flags]
            public enum BrowseInfos
            {
                NewDialogStyle = 0x0040,   // Use the new dialog layout with the ability to resize
                HideNewFolderButton = 0x0200    // Don't display the 'New Folder' button
            }

            public delegate int BrowseCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class OPENFILENAME_I
            {
                public int lStructSize = Marshal.SizeOf(typeof(OPENFILENAME_I)); //ndirect.DllLib.sizeOf(this);
                public IntPtr hwndOwner;
                public IntPtr hInstance;
                public string lpstrFilter;   // use embedded nulls to separate filters
                public IntPtr lpstrCustomFilter = IntPtr.Zero;
                public int nMaxCustFilter = 0;
                public int nFilterIndex;
                public IntPtr lpstrFile;
                public int nMaxFile = MAX_PATH;
                public IntPtr lpstrFileTitle = IntPtr.Zero;
                public int nMaxFileTitle = MAX_PATH;
                public string lpstrInitialDir;
                public string lpstrTitle;
                public int Flags;
                public short nFileOffset = 0;
                public short nFileExtension = 0;
                public string lpstrDefExt;
                public IntPtr lCustData = IntPtr.Zero;
                public /*WndProc*/ IntPtr lpfnHook;
                public string lpTemplateName = null;
                public IntPtr pvReserved = IntPtr.Zero;
                public int dwReserved = 0;
                public int FlagsEx;
            }
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class BROWSEINFO
            {
                public IntPtr hwndOwner; //HWND hwndOwner; // HWND of the owner for the dialog
                public IntPtr pidlRoot; //LPCITEMIDLIST pidlRoot; // Root ITEMIDLIST

                // For interop purposes, send over a buffer of MAX_PATH size. 
                public IntPtr pszDisplayName; //LPWSTR pszDisplayName; // Return display name of item selected.

                public string lpszTitle; //LPCWSTR lpszTitle; // text to go in the banner over the tree.
                public int ulFlags; //UINT ulFlags; // Flags that control the return stuff
                public BrowseCallbackProc lpfn; //BFFCALLBACK lpfn; // Call back pointer
                public IntPtr lParam; //LPARAM lParam; // extra info that's passed back in callbacks
                public int iImage; //int iImage; // output var: where to return the Image index.
            }

            [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OPENFILENAME_I ofn);
            [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool GetSaveFileName([In, Out] OPENFILENAME_I ofn);
            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SHBrowseForFolder([In] BROWSEINFO lpbi);
            [DllImport("shell32.dll")]
            public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);
            [DllImport("ole32.dll", SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = true)]
            internal extern static void CoTaskMemFree(IntPtr pv);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);
            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            private static extern bool SHGetPathFromIDListEx(IntPtr pidl, IntPtr pszPath, int cchPath, int flags);
            public static bool SHGetPathFromIDListLongPath(IntPtr pidl, ref IntPtr pszPath)
            {
                int noOfTimes = 1;
                // This is how size was allocated in the calling method.
                int bufferSize = Native.MAX_PATH * Marshal.SystemDefaultCharSize;
                int length = Native.MAX_PATH;
                bool result = false;

                // SHGetPathFromIDListEx returns false in case of insufficient buffer.
                // This method does not distinguish between insufficient memory and an error. Until we get a proper solution,
                // this logic would work. In the worst case scenario, loop exits when length reaches unicode string length.
                while ((result = SHGetPathFromIDListEx(pidl, pszPath, length, 0)) == false
                        && length < Native.MAX_UNICODESTRING_LEN)
                {
                    string path = Marshal.PtrToStringAuto(pszPath);

                    if (path.Length != 0 && path.Length < length)
                        break;

                    noOfTimes += 2; //520 chars capacity increase in each iteration.
                    length = noOfTimes * length >= Native.MAX_UNICODESTRING_LEN
                        ? Native.MAX_UNICODESTRING_LEN : noOfTimes * length;
                    pszPath = Marshal.ReAllocHGlobal(pszPath, (IntPtr)((length + 1) * Marshal.SystemDefaultCharSize));
                }

                return result;
            }

            public const int OFN_NOCHANGEDIR = 0x00000008;
            public const int OFN_OVERWRITEPROMPT = 0x00000002;
        }

        public static class Filters
        {
            public const string AllFiles = "All Files (*.*)\0*.*\0";
            public const string JsonFile = "JSON File (*.json)\0*.json\0";
            public const string IniFile = "INI File (*.ini)\0*.ini\0";
            public const string ImageFiles = "All Image Files (*.bmp; *.jpg; *.jpeg; *.png; *.tif; *.tiff)\0*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff\0" +
                "PNG (*.png)\0*.png\0" +
                "JPG (*.jpg; *.jpeg)\0*.jpg;*.jpeg\0" +
                "BMP (*.bmp)\0*.bmp\0" +
                "GIF (*.gif)\0*.gif\0" +
                "TIFF (*.tif; *.tiff)\0*.tif;*.tiff\0" +
                "EXIF (*.exif)\0*.exif\0";
        }

        public static bool OpenFile(out string filepath, 
            string initialDir = null,
            string title = null,
            string filter = null,
            string defExt = null)
        {
            throw new NotImplementedException();

            /*
            Native.OPENFILENAME_I ofn = new Native.OPENFILENAME_I();

            ofn.structSize = Marshal.SizeOf(ofn);

            ofn.filter = (filter != null) ? filter : "All Files (*.*)\0*.*\0";

            ofn.file = new String(new char[256]);
            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.initialDir = (initialDir != null) ? initialDir : "";
            ofn.title = (title != null) ? title : "Open";
            ofn.defExt = (defExt != null) ? defExt : "";

            // !!!
            ofn.flags |= Native.OFN_NOCHANGEDIR; // what kind person decided that it's good idea to change execution directory when calling this mess (._. )
            // !!!

            bool success = Native.GetOpenFileName(ofn);

            // sussy info
            //Console.WriteLine("Selected file with full path: {0}", ofn.file);

            //Console.WriteLine("Selected file name: {0}", ofn.fileTitle);
            //Console.WriteLine("Offset from file name: {0}", ofn.fileOffset);
            //Console.WriteLine("Offset from file extension: {0}", ofn.fileExtension);

            filepath = ofn.file;
            return success;
            */
        }

        public static bool SaveFile(out string filepath,
            string initialDir = null,
            string name = null,
            string title = null,
            string filter = null,
            string defExt = null)
        {
            throw new NotImplementedException();

            /*
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            ofn.filter = (filter != null) ? filter : Filters.AllFiles;

            var pls = new char[256];
            ((name != null) ? name : "file").ToCharArray().CopyTo((Span<char>)pls);
            ofn.file = new String(pls);
            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.initialDir = (initialDir != null) ? initialDir : "";
            ofn.title = (title != null) ? title : "Save";
            ofn.defExt = (defExt != null) ? defExt : "";

            ofn.flags |= Native.OFN_OVERWRITEPROMPT;

            bool success = Native.GetSaveFileName(ofn);

            // sussy info
            //Console.WriteLine("Selected file with full path: {0}", ofn.file);

            //Console.WriteLine("Selected file name: {0}", ofn.fileTitle);
            //Console.WriteLine("Offset from file name: {0}", ofn.fileOffset);
            //Console.WriteLine("Offset from file extension: {0}", ofn.fileExtension);

            filepath = ofn.file;
            return success;
            */
        }


        // code taken from .NET Framework by accident ;(
        public static bool BrowseFolderSimple(out string path, IntPtr? hWndOwnerParam = null, string descriptionText = "Já se asi poseru", bool showNewFolderButton = true)
        {
            IntPtr hWndOwner = hWndOwnerParam ?? IntPtr.Zero;
            IntPtr pidlRoot = IntPtr.Zero;
            bool returnValue = false;
            string selectedPath = String.Empty;

            Environment.SpecialFolder rootFolder = System.Environment.SpecialFolder.Desktop;

            Native.SHGetSpecialFolderLocation(hWndOwner, (int)rootFolder, ref pidlRoot);
            if (pidlRoot == IntPtr.Zero)
            {
                Native.SHGetSpecialFolderLocation(hWndOwner, Native.CSIDL_DESKTOP, ref pidlRoot);
                if (pidlRoot == IntPtr.Zero)
                {
                    throw new InvalidOperationException("do i rly care");
                }
            }

            int mergedOptions = unchecked((int)(long)Native.BrowseInfos.NewDialogStyle);
            if (!showNewFolderButton)
            {
                mergedOptions += unchecked((int)(long)Native.BrowseInfos.HideNewFolderButton);
            }

            IntPtr pidlRet = IntPtr.Zero;
            IntPtr pszDisplayName = IntPtr.Zero;
            IntPtr pszSelectedPath = IntPtr.Zero;
            Native.BrowseCallbackProc callback;

            try
            {
                // Construct a BROWSEINFO
                Native.BROWSEINFO bi = new Native.BROWSEINFO();

                pszDisplayName = Marshal.AllocHGlobal(Native.MAX_PATH * Marshal.SystemDefaultCharSize);
                pszSelectedPath = Marshal.AllocHGlobal((Native.MAX_PATH + 1) * Marshal.SystemDefaultCharSize);
                int FolderBrowserDialog_BrowseCallbackProc(IntPtr hwnd,
                                                           int msg,
                                                           IntPtr lParam,
                                                           IntPtr lpData)
                {
                    switch (msg)
                    {
                        case Native.BFFM_INITIALIZED:
                            // Indicates the browse dialog box has finished initializing. The lpData value is zero. 
                            if (selectedPath.Length != 0)
                            {
                                // Try to select the folder specified by selectedPath
                                Native.SendMessage(new HandleRef(null, hwnd), (int)Native.BFFM_SETSELECTION, 1, selectedPath);
                            }
                            break;
                        case Native.BFFM_SELCHANGED:
                            // Indicates the selection has changed. The lpData parameter points to the item identifier list for the newly selected item. 
                            IntPtr selectedPidl = lParam;
                            if (selectedPidl != IntPtr.Zero)
                            {
                                IntPtr pszSelectedPath = Marshal.AllocHGlobal((Native.MAX_PATH + 1) * Marshal.SystemDefaultCharSize);
                                // Try to retrieve the path from the IDList
                                bool isFileSystemFolder = Native.SHGetPathFromIDListLongPath(selectedPidl, ref pszSelectedPath);
                                Marshal.FreeHGlobal(pszSelectedPath);
                                Native.SendMessage(new HandleRef(null, hwnd), (int)Native.BFFM_ENABLEOK, 0, isFileSystemFolder ? 1 : 0);
                            }
                            break;
                    }
                    return 0;
                }
                callback = new Native.BrowseCallbackProc(FolderBrowserDialog_BrowseCallbackProc);

                bi.pidlRoot = pidlRoot;
                bi.hwndOwner = hWndOwner;
                bi.pszDisplayName = pszDisplayName;
                bi.lpszTitle = descriptionText;
                bi.ulFlags = mergedOptions;
                bi.lpfn = callback;
                bi.lParam = IntPtr.Zero;
                bi.iImage = 0;

                // And show the dialog
                pidlRet = Native.SHBrowseForFolder(bi);

                if (pidlRet != IntPtr.Zero)
                {
                    // Then retrieve the path from the IDList
                    Native.SHGetPathFromIDListLongPath(pidlRet, ref pszSelectedPath);

                    //// set the flag to True before selectedPath is set to
                    //// assure security check and avoid bogus race condition
                    //selectedPathNeedsCheck = true;

                    // Convert to a string
                    selectedPath = Marshal.PtrToStringAuto(pszSelectedPath);

                    returnValue = true;
                }
            }
            finally
            {
                Native.CoTaskMemFree(pidlRoot);
                if (pidlRet != IntPtr.Zero)
                {
                    Native.CoTaskMemFree(pidlRet);
                }

                // Then free all the stuff we've allocated or the SH API gave us
                if (pszSelectedPath != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszSelectedPath);
                }
                if (pszDisplayName != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszDisplayName);
                }

                callback = null;
            }

            path = selectedPath;
            return returnValue;
        }
    }
}
