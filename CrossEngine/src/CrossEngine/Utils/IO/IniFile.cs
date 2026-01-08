using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CrossEngine.Serialization;

namespace CrossEngine.Utils.IO
{
    public class IniFile
    {
        // peepee poopoo
        internal static readonly NumberFormatInfo nfi = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberDecimalDigits = 3 };
        const string WriteSeparator = " = ";

        public struct IniFileSection
        {
            private const string VectorSeparator = ",";
            
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
            public Vector4 ReadVec4(string key)
            {
                var parts = _variables[key].Split(VectorSeparator);
                return new Vector4(
                    float.Parse(parts[0].Trim(), IniFile.nfi),
                    float.Parse(parts[1].Trim(), IniFile.nfi),
                    float.Parse(parts[2].Trim(), IniFile.nfi),
                    float.Parse(parts[3].Trim(), IniFile.nfi));
            }
            public Vector3 ReadVec3(string key)
            {
                var parts = _variables[key].Split(VectorSeparator);
                return new Vector3(
                    float.Parse(parts[0].Trim(), IniFile.nfi),
                    float.Parse(parts[1].Trim(), IniFile.nfi),
                    float.Parse(parts[2].Trim(), IniFile.nfi));
            }
            public Vector2 ReadVec2(string key)
            {
                var parts = _variables[key].Split(VectorSeparator);
                return new Vector2(
                    float.Parse(parts[0].Trim(), IniFile.nfi),
                    float.Parse(parts[1].Trim(), IniFile.nfi));
            }

            public System.Drawing.Color ReadColor(string key)
            {
                return default;
            }
            #endregion

            #region TryRead
            private bool TryReadWrapper<T>(string key, out T value, Func<string, T> reader)
            {
                value = default;
                var result = _variables.ContainsKey(key);
                if (result) value = reader(key);
                return result;
            }
            
            public bool TryReadString(string key, out string value) => TryReadWrapper(key, out value, ReadString);
            public bool TryReadInt32(string key, out int value) => TryReadWrapper(key, out value, ReadInt32);
            public bool TryReadSingle(string key, out float value) => TryReadWrapper(key, out value, ReadSingle);
            public bool TryReadBoolean(string key, out bool value) => TryReadWrapper(key, out value, ReadBoolean);
            public bool TryReadVec4(string key, out Vector4 value) => TryReadWrapper(key, out value, ReadVec4);
            public bool TryReadVec3(string key, out Vector3 value) => TryReadWrapper(key, out value, ReadVec3);
            public bool TryReadVec2(string key, out Vector2 value) => TryReadWrapper(key, out value, ReadVec2);
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
            public void Write(string key, Vector4 value) => Write(key, $"{value.X.ToString(nfi)}{VectorSeparator}{value.Y.ToString(nfi)}{VectorSeparator}{value.Z.ToString(nfi)}{VectorSeparator}{value.W.ToString(nfi)}");
            public void Write(string key, Vector3 value) => Write(key, $"{value.X.ToString(nfi)}{VectorSeparator}{value.Y.ToString(nfi)}{VectorSeparator}{value.Z.ToString(nfi)}");
            public void Write(string key, Vector2 value) => Write(key, $"{value.X.ToString(nfi)}{VectorSeparator}{value.Y.ToString(nfi)}");
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

    public class IniSerializationInfo : SerializationInfo
    {
        IniFile.IniFileSection _section;

        public static IniSerializationInfo FromSection(IniFile.IniFileSection section)
        {
            return new IniSerializationInfo() { _section = section };
        }

        private IniSerializationInfo() { }
        
        public override void AddValue(string name, object value)
        {
            switch (value)
            {
                case string s: _section.Write(name, s); break;
                case int i: _section.Write(name, i); break;
                case float f: _section.Write(name, f); break;
                case bool b: _section.Write(name, b); break;
                case Vector4 v4: _section.Write(name, v4); break;
                case Vector3 v3: _section.Write(name, v3); break;
                case Vector2 v2: _section.Write(name, v2); break;
                default: throw new NotImplementedException();
            }
        }

        public override object GetValue(string name, Type typeOfValue)
        {
            switch (typeOfValue)
            {
                case Type t when t == typeof(string): return _section.ReadString(name);
                case Type t when t == typeof(int): return _section.ReadInt32(name);
                case Type t when t == typeof(float): return _section.ReadSingle(name);
                case Type t when t == typeof(bool): return _section.ReadBoolean(name);
                case Type t when t == typeof(Vector4): return _section.ReadVec4(name);
                case Type t when t == typeof(Vector3): return _section.ReadVec3(name);
                case Type t when t == typeof(Vector2): return _section.ReadVec2(name);
                default: throw new NotImplementedException();
            }
        }

        public override bool TryGetValue(string name, Type typeOfValue, out object value)
        {
            switch (typeOfValue)
            {
                case Type t when t == typeof(string): return WrapOutTry(out value, (out string v) => _section.TryReadString(name, out v));
                case Type t when t == typeof(int): return WrapOutTry(out value, (out int v) => _section.TryReadInt32(name, out v));
                case Type t when t == typeof(float): return WrapOutTry(out value, (out float v) => _section.TryReadSingle(name, out v));
                case Type t when t == typeof(bool): return WrapOutTry(out value, (out bool v) => _section.TryReadBoolean(name, out v));
                case Type t when t == typeof(Vector4): return WrapOutTry(out value, (out Vector4 v) => _section.TryReadVec4(name, out v));
                case Type t when t == typeof(Vector3): return WrapOutTry(out value, (out Vector3 v) => _section.TryReadVec3(name, out v));
                case Type t when t == typeof(Vector2): return WrapOutTry(out value, (out Vector2 v) => _section.TryReadVec2(name, out v));
                default: throw new NotImplementedException();
            }
        }
    }
}
