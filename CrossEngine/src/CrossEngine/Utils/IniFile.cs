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
        [DllImport("kernel32")]
        static extern int WritePrivateProfileString(string section, string key, string value, string filepath);
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filepath);

        public string Path;
        public int LineCapacity = 255;

        public IniFile(string path)
        {
            Path = new FileInfo(path).FullName;
        }

        public string this[string key]
        {
            get => ReadValue(key);
            set => WriteValue(key, value);
        }

        public string? ReadSectionValue(string section, string key)
        {
            var retVal = new StringBuilder(LineCapacity);
            GetPrivateProfileString(section, key, "", retVal, LineCapacity, Path);
            return retVal.ToString();
        }

        public void WriteSectionValue(string section, string key, string? value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }

        public void WriteValue(string key, string value)
        {
            WriteSectionValue(null, key, value);
        }

        public string? ReadValue(string key)
        {
            return ReadSectionValue(null, key);
        }
    }
}
