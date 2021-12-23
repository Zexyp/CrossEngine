using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;

namespace CrossEngineEditor.Utils
{
    class ImGuiStyleConfig
    {
        static class ImGuiStyleConfigSections
        {
            public static readonly string Colors = "style.colors";
            public static readonly string Sizes = "style.sizes";
        }

        // akward workaround
        private unsafe delegate bool RefReader(Func<string, string> reader, PropertyInfo propinfo, void* setTo, out int offset);
        private static readonly Dictionary<Type, Action<Action<string, string>, PropertyInfo, ImGuiStylePtr>> refWriters = new Dictionary<Type, Action<Action<string, string>, PropertyInfo, ImGuiStylePtr>>()
        {
            { typeof(float).MakeByRefType(), (writer, propinfo, style) => {
                float val = (float)propinfo.GetValue(style);
                writer(propinfo.Name, val.ToString());
            }},
            { typeof(Vector2).MakeByRefType(), (writer, propinfo, style) => {
                Vector2 val = (Vector2)propinfo.GetValue(style);
                writer(propinfo.Name, $"{val.X};{val.Y}");
            }},
            { typeof(ImGuiDir).MakeByRefType(), (writer, propinfo, style) => {
                ImGuiDir val = (ImGuiDir)propinfo.GetValue(style);
                writer(propinfo.Name, val.ToString());
            }},
            { typeof(bool).MakeByRefType(), (writer, propinfo, style) => {
                bool val = (bool)propinfo.GetValue(style);
                writer(propinfo.Name, val.ToString());
            }},
        };
        private unsafe static readonly Dictionary<Type, RefReader> refReaders = new Dictionary<Type, RefReader>()
        {
            { typeof(float).MakeByRefType(), (Func<string, string> reader, PropertyInfo propinfo, void* ptr, out int offset) => {
                offset = sizeof(float);

                string stringValue = reader(propinfo.Name);

                if (String.IsNullOrEmpty(stringValue)) return true;

                if (float.TryParse(stringValue, out float val))
                {
                    *(float*)ptr = val;
                    return true;
                }
                return false;
            }},
            { typeof(Vector2).MakeByRefType(), (Func<string, string> reader, PropertyInfo propinfo, void* ptr, out int offset) => {
                offset = sizeof(Vector2);

                string stringValue = reader(propinfo.Name);

                if (String.IsNullOrEmpty(stringValue)) return true;

                string[] sepVals = stringValue.Split(";");

                if (sepVals.Length < 2)
                {
                    return false;
                }

                bool valid = true;

                Vector2 val = new Vector2();

                valid = valid && float.TryParse(sepVals[0], out val.X);
                valid = valid && float.TryParse(sepVals[1], out val.Y);

                if (valid) *(Vector2*)ptr = val;
                return valid;
            }},
            { typeof(ImGuiDir).MakeByRefType(), (Func<string, string> reader, PropertyInfo propinfo, void* ptr, out int offset) => {
                offset = sizeof(ImGuiDir);

                string stringValue = reader(propinfo.Name);

                if (String.IsNullOrEmpty(stringValue)) return true;

                if (Enum.TryParse(stringValue, out ImGuiDir val))
                {
                    *(ImGuiDir*)ptr = val;
                    return true;
                }
                return false;
            }},
            { typeof(bool).MakeByRefType(), (Func<string, string> reader, PropertyInfo propinfo, void* ptr, out int offset) => {
                offset = sizeof(bool);

                string stringValue = reader(propinfo.Name);

                if (String.IsNullOrEmpty(stringValue)) return true;

                if (bool.TryParse(stringValue, out bool val))
                {
                    *(bool*)ptr = val;
                    return true;
                }
                return false;
            }},
        };

        public static void Save(IniConfig config)
        {
            // colors
            var style = ImGui.GetStyle();
            {
                for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                {
                    Vector4 col = style.Colors[i];
                    string name = ImGui.GetStyleColorName((ImGuiCol)i);
                    config.Write(ImGuiStyleConfigSections.Colors, name, $"{col.X};{col.Y};{col.Z};{col.W}");
                }
            }
            // sizes
            {
                Action<string, string> writer = (key, value) => config.Write(ImGuiStyleConfigSections.Sizes, key, value);
                foreach (var propinfo in style.GetType().GetProperties().Where(propinfo => propinfo.PropertyType.IsByRef))
                {
                    if (refWriters.ContainsKey(propinfo.PropertyType))
                    {
                        refWriters[propinfo.PropertyType](writer, propinfo, style);
                    }
                }
            }
        }

        public static bool Load(IniConfig config)
        {
            bool success = true;

            var style = ImGui.GetStyle();
            // colors
            {
                bool valid = true;

                for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                {
                    string name = ImGui.GetStyleColorName((ImGuiCol)i);
                    string stringValue = config.Read(ImGuiStyleConfigSections.Colors, name);

                    if (String.IsNullOrEmpty(stringValue)) continue;

                    string[] sepVals = stringValue.Split(";");

                    if (sepVals.Length < 4)
                    {
                        valid = false;
                        continue;
                    }

                    Vector4 col = new Vector4();

                    valid = valid && float.TryParse(sepVals[0], out col.X);
                    valid = valid && float.TryParse(sepVals[1], out col.Y);
                    valid = valid && float.TryParse(sepVals[2], out col.Z);
                    valid = valid && float.TryParse(sepVals[3], out col.W);

                    if (valid) style.Colors[i] = col;
                }

                if (!valid) EditorApplication.Log.Trace($"invalid config section '{ImGuiStyleConfigSections.Colors}'");

                success = success && valid;
            }
            // sizes
            unsafe
            {
                bool valid = true;
                Func<string, string> reader = (key) => config.Read(ImGuiStyleConfigSections.Sizes, key);
                byte* baseAddress = (byte*)style.NativePtr;
                foreach (var propinfo in style.GetType().GetProperties().Where(propinfo => propinfo.PropertyType.IsByRef))
                {
                    if (refReaders.ContainsKey(propinfo.PropertyType))
                    {
                        int offset = 0;
                        valid = valid && refReaders[propinfo.PropertyType](reader, propinfo, baseAddress, out offset);
                        baseAddress += offset;
                    }
                }

                if (!valid) EditorApplication.Log.Trace($"invalid config section '{ImGuiStyleConfigSections.Sizes}'");

                success = success && valid;
            }

            return success;
        }
    }
}
