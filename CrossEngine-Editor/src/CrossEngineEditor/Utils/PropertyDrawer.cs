﻿using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Reflection;

using CrossEngine.Utils.Editor;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Utils
{
    public class PropertyDrawer
    {
        delegate bool EditorValueRepresentationFunction(EditorValueAttribute attribute, string name, ref object value);
        delegate bool EditorValueFunction(string name, ref object value);
        delegate bool EditorRangeFunction(IRangeValue range, string name, ref object value);
        delegate bool EditorDragFunction(ISteppedRangeValue range, string name, ref object value);
        delegate bool EditorSliderFunction(IRangeValue range, string name, ref object value);

        private static void PrintInvalidUI(string name)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            ImGui.Text($"{name} invalid UI");
            ImGui.PopStyleColor();
        }

        static readonly Dictionary<Type, EditorValueFunction> ValueHandlers = new Dictionary<Type, EditorValueFunction>()
        {
            { typeof(bool), (string name, ref object value) => {
                bool v = (bool)value;
                bool success;
                success = ImGui.Checkbox(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(int), (string name, ref object value) => {
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(float), (string name, ref object value) => {
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector2), (string name, ref object value) => {
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector3), (string name, ref object value) => {
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector4), (string name, ref object value) => {
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = v;
                return success;
            } },
        };
        static readonly Dictionary<Type, EditorRangeFunction> RangeHandlers = new Dictionary<Type, EditorRangeFunction>()
        {
            { typeof(int), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = Math.Clamp(v, crange.Min, crange.Max);
                return success;
            } },
            { typeof(float), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector2), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = Vector2.Clamp(v, new Vector2(crange.Min), new Vector2(crange.Max));
                return success;
            } },
            { typeof(Vector3), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = Vector3.Clamp(v, new Vector3(crange.Min), new Vector3(crange.Max));
                return success;
            } },
            { typeof(Vector4), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = Vector4.Clamp(v, new Vector4(crange.Min), new Vector4(crange.Max));
                return success;
            } },
        };
        static readonly Dictionary<Type, EditorSliderFunction> SliderHandlers = new Dictionary<Type, EditorSliderFunction>()
        {
            { typeof(int), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.SliderInt(name, ref v, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(float), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.SliderFloat(name, ref v, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector2), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.SliderFloat2(name, ref v, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector3), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.SliderFloat3(name, ref v, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector4), (IRangeValue range, string name, ref object value) => {
                var crange = (IRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.SliderFloat4(name, ref v, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
        };
        static readonly Dictionary<Type, EditorDragFunction> DragHandlers = new Dictionary<Type, EditorDragFunction>()
        {
            { typeof(int), (ISteppedRangeValue range, string name, ref object value) => {
                var crange = (ISteppedRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.DragInt(name, ref v, crange.Step, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(float), (ISteppedRangeValue range, string name, ref object value) => {
                var crange = (ISteppedRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.DragFloat(name, ref v, crange.Step, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector2), (ISteppedRangeValue range, string name, ref object value) => {
                var crange = (ISteppedRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.DragFloat2(name, ref v, crange.Step, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector3), (ISteppedRangeValue range, string name, ref object value) => {
                var crange = (ISteppedRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.DragFloat3(name, ref v, crange.Step, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector4), (ISteppedRangeValue range, string name, ref object value) => {
                var crange = (ISteppedRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.DragFloat4(name, ref v, crange.Step, crange.Min, crange.Max);
                if (success) value = v;
                return success;
            } },
        };



        static readonly Dictionary<Type, EditorValueRepresentationFunction> AttributeHandlers = new Dictionary<Type, EditorValueRepresentationFunction>()
        {
            { typeof(EditorValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                if (ValueHandlers.ContainsKey(value.GetType()))
                    return ValueHandlers[value.GetType()](name, ref value);
                else
                {
                    PrintInvalidUI(name);
                    return false;
                }
            } },
            { typeof(EditorRangeAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorRangeAttribute)attribute;
                if (RangeHandlers.ContainsKey(value.GetType()))
                    return RangeHandlers[value.GetType()](cattribt, name, ref value);
                else
                {
                    PrintInvalidUI(name);
                    return false;
                }
            } },
            { typeof(EditorSliderAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorSliderAttribute)attribute;
                if (SliderHandlers.ContainsKey(value.GetType()))
                    return SliderHandlers[value.GetType()](cattribt, name, ref value);
                else
                {
                    PrintInvalidUI(name);
                    return false;
                }
            } },
            { typeof(EditorDragAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorDragAttribute)attribute;
                if (DragHandlers.ContainsKey(value.GetType()))
                    return DragHandlers[value.GetType()](cattribt, name, ref value);
                else
                {
                    PrintInvalidUI(name);
                    return false;
                }
            } },
            { typeof(EditorSectionAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                ImGui.Text(attribute.Name);
                ImGui.SameLine();
                ImGuiUtils.SmartSeparator(3);
                return false;
            } },

            #region Primitives
            #region Number
            { typeof(EditorInt32ValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorInt32ValueAttribute)attribute;
                int v = (int)value;
                bool success;
                switch (cattribt.NumberInputType)
                {
                    case NumberInputType.Drag:
                        {
                            success = ImGui.DragInt(name, ref v, cattribt.Step, (int)cattribt.Min, (int)cattribt.Max);
                        }
                        break;
                    case NumberInputType.Input:
                        {
                            success = ImGui.InputInt(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputType.Slider:
                        {
                            success = ImGui.SliderInt(cattribt.Name, ref v, (int)cattribt.Min, (int)cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputType) + " value.");
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
                    case NumberInputType.Drag:
                        {
                            success = ImGui.DragFloat(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputType.Input:
                        {
                            success = ImGui.InputFloat(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputType.Slider:
                        {
                            success = ImGui.SliderFloat(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputType) + " value.");
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
                    case NumberInputType.Drag:
                        {
                            success = ImGui.DragFloat2(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputType.Input:
                        {
                            success = ImGui.InputFloat2(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputType.Slider:
                        {
                            success = ImGui.SliderFloat2(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputType) + " value.");
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
                    case NumberInputType.Drag:
                        {
                            success = ImGui.DragFloat3(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputType.Input:
                        {
                            success = ImGui.InputFloat3(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputType.Slider:
                        {
                            success = ImGui.SliderFloat3(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputType) + " value.");
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
                    case NumberInputType.Drag:
                        {
                            success = ImGui.DragFloat4(name, ref v, cattribt.Step, cattribt.Min, cattribt.Max);
                        }
                        break;
                    case NumberInputType.Input:
                        {
                            success = ImGui.InputFloat4(cattribt.Name, ref v);
                        }
                        break;
                    case NumberInputType.Slider:
                        {
                            success = ImGui.SliderFloat4(cattribt.Name, ref v, cattribt.Min, cattribt.Max);
                        }
                        break;
                    default: throw new ArgumentException("Invalid " + nameof(NumberInputType) + " value.");
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
            /*
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
                IAssetCollection assets = null;
                IReadOnlyCollection<Asset> assetValues = null;
                int ci = 0;
                if (scene != null)
                {
                    if (value is IAssetHandle)
                        v = ((IAssetHandle)value).Asset;
                    else
                        v = (Asset)value;

                    assets = (IAssetCollection)typeof(AssetPool).GetMethod(nameof(AssetPool.GetCollection)).MakeGenericMethod(cattribt.Type).Invoke(scene.AssetPool, null);
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

                if (success)
                {
                    if (value is IAssetHandle)
                        ((IAssetHandle)value).Asset = v;
                    else
                        value = v;
                }
                return success;
            } },
            */

            { typeof(EditorInnerValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorInnerValueAttribute)attribute;
                Type valtype = value.GetType();
                MemberInfo[] membs = valtype.GetMembers();
                ImGuiUtils.BeginGroupFrame();
                for (int mi = 0; mi < membs.Length; mi++)
                {
                    var mem = membs[mi];
                    switch (mem.MemberType)
                    {
                        case MemberTypes.Field:    PropertyDrawer.DrawEditorValue((FieldInfo)mem, value); break;
                        case MemberTypes.Property: PropertyDrawer.DrawEditorValue((PropertyInfo)mem, value); break;
                    }
                }
                ImGuiUtils.EndGroupFrame();
                return false;
            } },
        };

        public static void DrawEditorValue(FieldInfo fieldInfo, object target)
        {
            foreach (var attrib in fieldInfo.GetCustomAttributes<EditorValueAttribute>(true).
                OrderByDescending(a => a.Type))
            {
                Type attribType = attrib.GetType();
                if (AttributeHandlers.ContainsKey(attribType))
                {
                    object value = fieldInfo.GetValue(target);
                    if (AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : fieldInfo.Name, ref value))
                        fieldInfo.SetValue(target, value);
                }
                else
                {
                    PrintInvalidUI((attrib.Name != null) ? attrib.Name : fieldInfo.Name);
                }
            }
        }

        public static void DrawEditorValue(PropertyInfo propertyInfo, object target)
        {
            foreach (var attrib in propertyInfo.GetCustomAttributes<EditorValueAttribute>(true).
                OrderByDescending(a => a.Type))
            {
                Type attribType = attrib.GetType();
                if (AttributeHandlers.ContainsKey(attribType))
                {
                    object value = propertyInfo.GetValue(target);
                    if (AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : propertyInfo.Name, ref value))
                        propertyInfo.SetValue(target, value);
                }
                else
                {
                    PrintInvalidUI((attrib.Name != null) ? attrib.Name : propertyInfo.Name);
                }
            }
        }

        public static void DrawEditorValue(MemberInfo memberInfo, object target)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field: DrawEditorValue((FieldInfo)memberInfo, target); break;
                case MemberTypes.Property: DrawEditorValue((PropertyInfo)memberInfo, target); break;
                default: throw new InvalidOperationException();
            }
        }
    }
}
