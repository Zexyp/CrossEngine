using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CrossEngineEditor.Utils
{
    // black box content provided by:
    // https://stackoverflow.com/questions/9088227/using-getopenfilename-instead-of-openfiledialog

    // Copyright
    // Microsoft Corporation
    // All rights reserved

    // OpenFileDlg.cs

    

    /*
    typedef struct tagOFN { 
      DWORD         lStructSize; 
      HWND          hwndOwner; 
      HINSTANCE     hInstance; 
      LPCTSTR       lpstrFilter; 
      LPTSTR        lpstrCustomFilter; 
      DWORD         nMaxCustFilter; 
      DWORD         nFilterIndex; 
      LPTSTR        lpstrFile; 
      DWORD         nMaxFile; 
      LPTSTR        lpstrFileTitle; 
      DWORD         nMaxFileTitle; 
      LPCTSTR       lpstrInitialDir; 
      LPCTSTR       lpstrTitle; 
      DWORD         Flags; 
      WORD          nFileOffset; 
      WORD          nFileExtension; 
      LPCTSTR       lpstrDefExt; 
      LPARAM        lCustData; 
      LPOFNHOOKPROC lpfnHook; 
      LPCTSTR       lpTemplateName; 
    #if (_WIN32_WINNT >= 0x0500)
      void *        pvReserved;
      DWORD         dwReserved;
      DWORD         FlagsEx;
    #endif // (_WIN32_WINNT >= 0x0500)
    } OPENFILENAME, *LPOPENFILENAME; 
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;

        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;

        public String file = null;
        public int maxFile = 0;

        public String fileTitle = null;
        public int maxFileTitle = 0;

        public String initialDir = null;

        public String title = null;

        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;

        public String defExt = null;

        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;

        public String templateName = null;

        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    internal class LibWrap
    {
        //BOOL GetOpenFileName(LPOPENFILENAME lpofn);

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        public const int OFN_NOCHANGEDIR = 0x00000008;
        public const int OFN_OVERWRITEPROMPT = 0x00000002;
    }

    public class FileDialog
    {
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

        public static bool Open(out string filepath, 
            string initialDir = null,
            string title = null,
            string filter = null,
            string defExt = null)
        {
            OpenFileName ofn = new OpenFileName();

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
            ofn.flags |= LibWrap.OFN_NOCHANGEDIR; // what kind person decided that it's good idea to change execution directory when calling this mess (._. )
            // !!!

            bool success = LibWrap.GetOpenFileName(ofn);

            // sussy info
            //Console.WriteLine("Selected file with full path: {0}", ofn.file);

            //Console.WriteLine("Selected file name: {0}", ofn.fileTitle);
            //Console.WriteLine("Offset from file name: {0}", ofn.fileOffset);
            //Console.WriteLine("Offset from file extension: {0}", ofn.fileExtension);

            filepath = ofn.file;
            return success;
        }

        public static bool Save(out string filepath,
            string initialDir = null,
            string name = null,
            string title = null,
            string filter = null,
            string defExt = null)
        {
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            ofn.filter = (filter != null) ? filter : "All Files (*.*)\0*.*\0";

            var pls = new char[256];
            ((name != null) ? name : "file").ToCharArray().CopyTo((Span<char>)pls);
            ofn.file = new String(pls);
            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.initialDir = (initialDir != null) ? initialDir : "";
            ofn.title = (title != null) ? title : "Save";
            ofn.defExt = (defExt != null) ? defExt : "";

            ofn.flags |= LibWrap.OFN_OVERWRITEPROMPT;

            bool success = LibWrap.GetSaveFileName(ofn);

            // sussy info
            //Console.WriteLine("Selected file with full path: {0}", ofn.file);

            //Console.WriteLine("Selected file name: {0}", ofn.fileTitle);
            //Console.WriteLine("Offset from file name: {0}", ofn.fileOffset);
            //Console.WriteLine("Offset from file extension: {0}", ofn.fileExtension);

            filepath = ofn.file;
            return success;
        }
    }
}
