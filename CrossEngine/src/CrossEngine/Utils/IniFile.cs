using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrossEngine.Utils
{
    public class IniFile
    {
        // peepee poopoo
        static readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };
        const string WriteSeparator = " = ";

        public struct IniFileSection
        {
            internal Dictionary<string, string> _variables = new Dictionary<string, string>();

            public IniFileSection()
            {
            }

            #region Read
            public string ReadString(string key) => _variables[key];
            public int ReadInt32(string key) => int.Parse(_variables[key]);
            public float ReadSingle(string key) => float.Parse(_variables[key], nfi);
            public bool ReadBoolean(string key)
            {
                switch (_variables[key])
                {
                    case "yes":
                    case "on":
                    case "true":
                    case "True":
                    case "1":
                        return true;
                    case "no":
                    case "off":
                    case "false":
                    case "False":
                    case "0":
                        return false;
                    default:
                        throw new FormatException();
                        break;
                }
            }
            #endregion

            #region TryRead
            public bool TryReadString(string key, out string value)
            {
                value = default;
                var result = _variables.ContainsKey(key);
                if (result) value = ReadString(key);
                return result;
            }
            public bool TryReadInt32(string key, out int value)
            {
                value = default;
                var result = _variables.ContainsKey(key);
                if (result) value = ReadInt32(key);
                return result;
            }
            public bool TryReadSingle(string key, out float value)
            {
                value = default;
                var result = _variables.ContainsKey(key);
                if (result) value = ReadSingle(key);
                return result;
            }
            public bool TryReadBoolean(string key, out bool value)
            {
                value = default;
                var result = _variables.ContainsKey(key);
                if (result) value = ReadBoolean(key);
                return result;
            }
            #endregion
            
            #region ReadOrDefault
            public string ReadStringOrDefault(string key, string defaultValue) => TryReadString(key, out var value) ? value : defaultValue;
            public int ReadInt32OrDefault(string key, int defaultValue) => TryReadInt32(key, out var value) ? value : defaultValue;
            public float ReadSingleOrDefault(string key, float defaultValue) => TryReadSingle(key, out var value) ? value : defaultValue;
            public bool ReadBooleanOrDefault(string key, bool defaultValue) => TryReadBoolean(key, out var value) ? value : defaultValue;
            #endregion
            
            #region Write

            public void Write(string key, string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new FormatException();
                _variables[key] = value;
            }
            public void Write(string key, int value) => Write(key, value.ToString());
            public void Write(string key, float value) => Write(key, value.ToString(nfi));
            public void Write(string key, bool value) => Write(key, value.ToString());
            #endregion
        }

        Dictionary<string, IniFileSection> _sections = new Dictionary<string, IniFileSection>();

        public IniFileSection this[string section]
        {
            get
            {
                if (!_sections.ContainsKey(section))
                    _sections.Add(section, new IniFileSection());
                return _sections[section];
            }
        }

        public static IniFile Load(Stream stream)
        {
            var ini = new IniFile();
            using (var reader = new StreamReader(stream))
            {
                string currentSection = null;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    switch (line)
                    {
                        case string s when s.StartsWith(";") || s.StartsWith("#") || s.StartsWith("/"):
                            continue;
                        case string s when s.StartsWith("[") && s.EndsWith("]"):
                            currentSection = line.TrimStart('[').TrimEnd(']');
                            continue;
                        default:
                            var match = Regex.Match(line, @"^\s*(\S+)\s*[:=]\s*(.*)\s*$");
                            var key = match.Groups[1].Value.Trim(' ');
                            var value = match.Groups[2].Value.Trim(' ').Trim('"');
                            ini[currentSection].Write(key, value);
                            break;
                    }
                }
            }
            return ini;
        }

        public static void Dump(IniFile ini, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                foreach (var pair in ini._sections)
                {
                    if (pair.Key != null) writer.WriteLine($"[{pair.Key}]");
                    foreach (var variable in pair.Value._variables)
                    {
                        var key = variable.Key;
                        var value = variable.Value;
                        if (value.StartsWith(" ") || value.EndsWith(" "))
                            value = $"\"{value}\"";
                        
                        Debug.Assert(!string.IsNullOrWhiteSpace(key));
                        
                        writer.WriteLine($"{key}{WriteSeparator}{value}");
                    }
                }
            }
        }
    }
}
