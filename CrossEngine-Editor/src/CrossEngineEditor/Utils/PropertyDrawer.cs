using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Reflection;

using CrossEngine.Utils.Editor;
using CrossEngine.Assets;

namespace CrossEngineEditor.Utils
{
    public class PropertyDrawer
    {
        delegate bool EditorValueRepresentationFunction(EditorValueAttribute attribute, string name, ref object value);
        static readonly Dictionary<Type, EditorValueRepresentationFunction> AttributeHandlers = new Dictionary<Type, EditorValueRepresentationFunction>
        {
            #region Primitives
            #region Number
            { typeof(EditorInt32ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorInt32ValueAttribute)attribute;
                int v = (int)value;
                bool success;
                switch (cattribt.NumberInputType)
                {
                    case NumberInputTypeRepresentation.Drag:
                        {
                            success = ImGui.DragInt(name, ref v, cattribt.Step, (int)cattribt.Min, (int)cattribt.Max);
                        }
                        break;
                    case NumberInputTypeRepresentation.Input:
                        {
                            success = ImGui.InputInt(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputTypeRepresentation.Slider:
                        {
                            success = ImGui.SliderInt(cattribt.Name, ref v, (int)cattribt.Min, (int)cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputTypeRepresentation) + " value.");
                }
                if (success) value = v;
                return success;
            } },
            { typeof(EditorSingleValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorSingleValueAttribute)attribute;
                float v = (float)value;
                bool success;
                switch (cattribt.NumberInputType)
                {
                    case NumberInputTypeRepresentation.Drag:
                        {
                            success = ImGui.DragFloat(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputTypeRepresentation.Input:
                        {
                            success = ImGui.InputFloat(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputTypeRepresentation.Slider:
                        {
                            success = ImGui.SliderFloat(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputTypeRepresentation) + " value.");
                }
                if (success) value = v;
                return success;
            } },
            { typeof(EditorVector2ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorVector2ValueAttribute)attribute;
                Vector2 v = (Vector2)value;
                bool success;
                switch (cattribt.NumberInputType)
                {
                    case NumberInputTypeRepresentation.Drag:
                        {
                            success = ImGui.DragFloat2(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputTypeRepresentation.Input:
                        {
                            success = ImGui.InputFloat2(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputTypeRepresentation.Slider:
                        {
                            success = ImGui.SliderFloat2(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputTypeRepresentation) + " value.");
                }
                if (success) value = v;
                return success;
            } },
            { typeof(EditorVector3ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorVector3ValueAttribute)attribute;
                Vector3 v = (Vector3)value;
                bool success;
                switch (cattribt.NumberInputType)
                {
                    case NumberInputTypeRepresentation.Drag:
                        {
                            success = ImGui.DragFloat3(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputTypeRepresentation.Input:
                        {
                            success = ImGui.InputFloat3(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputTypeRepresentation.Slider:
                        {
                            success = ImGui.SliderFloat3(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputTypeRepresentation) + " value.");
                }
                if (success) value = v;
                return success;
            } },
            { typeof(EditorVector4ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorVector4ValueAttribute)attribute;
                Vector4 v = (Vector4)value;
                bool success;
                switch (cattribt.NumberInputType)
                {
                    case NumberInputTypeRepresentation.Drag:
                        {
                            success = ImGui.DragFloat4(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputTypeRepresentation.Input:
                        {
                            success = ImGui.InputFloat4(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputTypeRepresentation.Slider:
                        {
                            success = ImGui.SliderFloat4(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputTypeRepresentation) + " value.");
                }
                if (success) value = v;
                return success;
            } },
            #endregion
            #region Color
            { typeof(EditorColor3ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorColor3ValueAttribute)attribute;
                Vector3 v = (Vector3)value;
                bool success = ImGui.ColorEdit3(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(EditorColor4ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorColor4ValueAttribute)attribute;
                Vector4 v = (Vector4)value;
                bool success = ImGui.ColorEdit4(name, ref v);
                if (success) value = v;
                return success;
            } },
            #endregion
            { typeof(EditorBooleanValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorBooleanValueAttribute)attribute;
                bool v = (bool)value;
                bool success = ImGui.Checkbox(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(EditorStringValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorStringValueAttribute)attribute;
                byte[] v = new byte[cattribt.MaxLength];
                Encoding.UTF8.GetBytes((string)value).CopyTo(v, 0);
                bool success = ImGui.InputText(name, v, cattribt.MaxLength);
                if (success) value = Encoding.UTF8.GetString(v).TrimEnd('\0');
                return success;
            } },
            #endregion

            { typeof(EditorEnumValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorEnumValueAttribute)attribute;
                bool success;
                int v = (int)value;
                string[] items = Enum.GetValues(value.GetType()).OfType<object>().Select(o => o.ToString()).ToArray();
                ImGui.PushID(name);
                success = ImGui.Combo(name, ref v, items, items.Length);
                ImGui.PopID();
                if (success) value = v;
                return success;
            } },

            { typeof(EditorAssetValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorAssetValueAttribute)attribute;
                bool success = false;

                var scene = EditorLayer.Instance.Context.Scene;
                Asset v = null;
                AssetCollection assets = null;
                IReadOnlyCollection<Asset> assetValues = null;
                int ci = 0;
                if (scene != null)
                {
                    v = (Asset)value;
                    assets = (AssetCollection)typeof(AssetPool).GetMethod(nameof(AssetPool.GetCollection)).MakeGenericMethod(cattribt.Type).Invoke(scene.AssetPool, null);
                    assetValues = assets.GetAll();
                    ci = (assetValues != null) ? assetValues.ToList().IndexOf(v) : 0;
                }

                ImGui.PushID(name);
                if (ImGui.BeginCombo("", (v != null) ? v.Name : ""))
                {
                    if (assetValues != null) foreach (var item in assetValues)
                    {
                        bool sel = item == v;
                        if (ImGui.Selectable(item.Name, sel))
                        {
                            success = true;
                            v = item;
                        }
                        if (sel)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
                ImGui.PopID();
                ImGui.SameLine();
                if (ImGui.Button("×"))
                {
                    v = null;
                    success = true;
                }
                ImGui.SameLine();
                ImGui.Text(name);

                if (success) value = v;
                return success;
            } },
        };

        public static void DrawEditorValue(FieldInfo fieldInfo, object target)
        {
            EditorValueAttribute attrib = fieldInfo.GetCustomAttribute<EditorValueAttribute>(true);
            if (attrib != null)
            {
                Type attribType = attrib.GetType();
                if (AttributeHandlers.ContainsKey(attribType))
                {
                    object value = fieldInfo.GetValue(target);
                    if (AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : fieldInfo.Name, ref value))
                        fieldInfo.SetValue(target, value);
                }
                else ImGui.Text(fieldInfo.Name);
            }
        }

        public static void DrawEditorValue(PropertyInfo propertyInfo, object target)
        {
            EditorValueAttribute attrib = propertyInfo.GetCustomAttribute<EditorValueAttribute>(true);
            if (attrib != null)
            {
                Type attribType = attrib.GetType();
                if (AttributeHandlers.ContainsKey(attribType))
                {
                    object value = propertyInfo.GetValue(target);
                    if (AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : propertyInfo.Name, ref value))
                        propertyInfo.SetValue(target, value);
                }
                else ImGui.Text(propertyInfo.Name);
            }
        }
    }
}
