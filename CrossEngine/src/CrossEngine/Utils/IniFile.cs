using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace CrossEngine.Utils
{
    public class IniFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int WritePrivateProfileString(string section, string key, string value, string filepath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filepath);

        public string Path;
        public int LineCapacity = 255;

        public IniFile(string path, bool isFileName = false)
        {
            if (!isFileName) path += ".ini";
            Path = new FileInfo(path).FullName;
        }

        public string Read(string section, string key)
        {
            var retVal = new StringBuilder(LineCapacity);
            GetPrivateProfileString(section, key, "", retVal, LineCapacity, Path);
            return retVal.ToString();
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }

        public void DeleteKey(string section, string key)
        {
            Write(section, key, null);
        }

        public void DeleteSection(string section)
        {
            Write(section, null, null);
        }
    }
}
