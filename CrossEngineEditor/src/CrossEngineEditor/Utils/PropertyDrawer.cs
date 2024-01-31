using System;
using ImGuiNET;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Reflection;
using System.Diagnostics;

using CrossEngine.Utils.Editor;
using CrossEngine.Utils;
using CrossEngine.Assets;
using CrossEngine;
using CrossEngine.Logging;
using CrossEngineEditor.Utils.Reflection;
using Silk.NET.Core.Native;

namespace CrossEngineEditor.Utils
{
    public static class PropertyDrawer
    {
        [Flags]
        public enum EditResult
        {
            None = 0,
            Changed = 1 << 0,
            DoneEditing = 1 << 1,
            Ended = 1 << 2,
            Started = 1 << 3,
        }

        delegate EditResult EditorValueRepresentationFunction(EditorValueAttribute attribute, string name, Type type, ref object value);

        delegate (bool, object) EditorValueFunction(string name, object value);
        delegate (bool, object) EditorRangeFunction(IRangeValue range, string name, object value);
        delegate (bool, object) EditorDragFunction(ISteppedRangeValue range, string name, object value);
        delegate (bool, object) EditorSliderFunction(IRangeValue range, string name, object value);
        delegate (bool, object) EditorColorFunction(EditorColorAttribute attribute, string name, object value);

        private static void PrintInvalidUI(string name)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            ImGui.Text($"{name} (invalid UI)");
            ImGui.PopStyleColor();
        }

        private static EditResult ExecuteFromDict(IDictionary dict, Type type, string name, ref object value, Attribute attribute = null)
        {
            if (dict.Contains(value.GetType()))
            {
                var result = ((bool Success, object Value))(attribute == null ? ((Delegate)dict[type]).DynamicInvoke(name, value) :
                                                                                ((Delegate)dict[type]).DynamicInvoke(attribute, name, value));

                value = result.Value;

                EditResult editResult = 0;
                editResult |= ImGui.IsItemDeactivatedAfterEdit() ? EditResult.DoneEditing : 0;
                editResult |= ImGui.IsItemActivated() ? EditResult.Started : 0;
                editResult |= ImGui.IsItemDeactivated() ? EditResult.Ended : 0;
                editResult |= result.Success ? EditResult.Changed : 0;
                return editResult;
            }
            else
            {
                PrintInvalidUI(name);
                return 0;
            }
        }

        // should be simple - IsItemDeactivatedAfterEdit will be invoked
        static readonly Dictionary<Type, EditorValueFunction> SimpleValueHandlers = new Dictionary<Type, EditorValueFunction>()
        {
            { typeof(bool), (string name, object value) => {
                bool v = (bool)value;
                bool success;
                success = ImGui.Checkbox(name, ref v);
                if (success) value = v;
                return (success, value);
            } },
            { typeof(int), (string name, object value) => {
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = v;
                return (success, value);
            } },
            { typeof(float), (string name, object value) => {
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = v;
                return (success, value);
            } },
            { typeof(Vector2), (string name, object value) => {
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = v;
                return (success, value);
            } },
            { typeof(Vector3), (string name, object value) => {
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = v;
                return (success, value);
            } },
            { typeof(Vector4), (string name, object value) => {
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = v;
                return (success, value);
            } },
        };
        static readonly Dictionary<Type, EditorRangeFunction> SimpleRangeHandlers = new Dictionary<Type, EditorRangeFunction>()
        {
            { typeof(int), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return (success, value);
            } },
            { typeof(float), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return (success, value);
            } },
            { typeof(Vector2), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = Vector2.Clamp(v, new Vector2(trange.Min), new Vector2(trange.Max));
                return (success, value);
            } },
            { typeof(Vector3), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = Vector3.Clamp(v, new Vector3(trange.Min), new Vector3(trange.Max));
                return (success, value);
            } },
            { typeof(Vector4), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = Vector4.Clamp(v, new Vector4(trange.Min), new Vector4(trange.Max));
                return (success, value);
            } },
        };
        static readonly Dictionary<Type, EditorSliderFunction> SimpleSliderHandlers = new Dictionary<Type, EditorSliderFunction>()
        {
            { typeof(int), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.SliderInt(name, ref v, (int)trange.Min, (int)trange.Max);
                if (success) value = Math.Clamp(v, (int)trange.Min, (int)trange.Max);
                return (success, value);
            } },
            { typeof(float), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.SliderFloat(name, ref v, trange.Min, trange.Max);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return (success, value);
            } },
            { typeof(Vector2), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.SliderFloat2(name, ref v, trange.Min, trange.Max);
                if (success) value = Vector2.Clamp(v, new Vector2(trange.Min), new Vector2(trange.Max));
                return (success, value);
            } },
            { typeof(Vector3), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.SliderFloat3(name, ref v, trange.Min, trange.Max);
                if (success) value = Vector3.Clamp(v, new Vector3(trange.Min), new Vector3(trange.Max));
                return (success, value);
            } },
            { typeof(Vector4), (IRangeValue range, string name, object value) => {
                var trange = (IRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.SliderFloat4(name, ref v, trange.Min, trange.Max);
                if (success) value = Vector4.Clamp(v, new Vector4(trange.Min), new Vector4(trange.Max));
                return (success, value);
            } },
        };
        static readonly Dictionary<Type, EditorDragFunction> SimpleDragHandlers = new Dictionary<Type, EditorDragFunction>()
        {
            { typeof(uint), (ISteppedRangeValue range, string name, object value) => {
                var trange = (ISteppedRangeValue<uint>)range;
                int v = (int)(uint)value;
                bool success;
                success = ImGui.DragInt(name, ref v, trange.Step, (int)Math.Max(trange.Min, 0), (int)trange.Max);
                if (success) value = (uint)Math.Clamp(v, trange.Min, trange.Max);
                return (success, value);
            } },
            { typeof(int), (ISteppedRangeValue range, string name, object value) => {
                var trange = (ISteppedRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.DragInt(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return (success, value);
            } },
            { typeof(float), (ISteppedRangeValue range, string name, object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.DragFloat(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return (success, value);
            } },
            { typeof(Vector2), (ISteppedRangeValue range, string name, object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.DragFloat2(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Vector2.Clamp(v, new Vector2(trange.Min), new Vector2(trange.Max));
                return (success, value);
            } },
            { typeof(Vector3), (ISteppedRangeValue range, string name, object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.DragFloat3(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Vector3.Clamp(v, new Vector3(trange.Min), new Vector3(trange.Max));
                return (success, value);
            } },
            { typeof(Vector4), (ISteppedRangeValue range, string name, object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.DragFloat4(name, ref v, trange.Step, trange.Max, trange.Min);
                if (success) value = Vector4.Clamp(v, new Vector4(trange.Min), new Vector4(trange.Max));
                return (success, value);
            } },
        };
        static readonly Dictionary<Type, EditorColorFunction> SimpleColorHandlers = new Dictionary<Type, EditorColorFunction>()
        {
            { typeof(Vector3), (EditorColorAttribute attribute, string name, object value) => {
                Vector3 v = (Vector3)value;
                bool success = ImGui.ColorEdit3(name, ref v, attribute.HDR ? ImGuiColorEditFlags.HDR : 0);
                if (success) value = v;
                return (success, value);
            } },
            { typeof(Vector4), (EditorColorAttribute attribute, string name, object value) => {
                Vector4 v = (Vector4)value;
                bool success = ImGui.ColorEdit4(name, ref v, attribute.HDR ? ImGuiColorEditFlags.HDR : 0);
                if (success) value = v;
                return (success, value);
            } },
        };

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        static readonly Dictionary<Type, EditorValueRepresentationFunction> AttributeHandlers = new Dictionary<Type, EditorValueRepresentationFunction>()
        {
            { typeof(EditorHintAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                throw new NotImplementedException();
            } },

            { typeof(EditorDisplayAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                ImGui.Text($"{(name != null ? $"{name}: " : "")}{value}");
                return EditResult.None;
            } },

            { typeof(EditorValueAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                return ExecuteFromDict(SimpleValueHandlers, type, name, ref value);
            } },

            { typeof(EditorRangeAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                return ExecuteFromDict(SimpleRangeHandlers, type, name, ref value, attribute);
            } },
            { typeof(EditorSliderAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                return ExecuteFromDict(SimpleSliderHandlers, type, name, ref value, attribute);
            } },
            { typeof(EditorDragAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                return ExecuteFromDict(SimpleDragHandlers, type, name, ref value, attribute);
            } },

            { typeof(EditorColorAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                return ExecuteFromDict(SimpleColorHandlers, type, name, ref value, attribute);
            } },

            { typeof(EditorSectionAttribute), (EditorValueAttribute attribute, string name, Type type, ref object value) => {
                ImGui.Text(name);
                ImGui.SameLine();
                ImGui.Separator();
                return EditResult.None;
            } },
            /*
            { typeof(EditorEnumAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorEnumAttribute)attribute;
                bool success = false;
                int v = (int)value;
                (int Value, string Name)[] items = Enum.GetValues(value.GetType()).OfType<object>().Select(o => ((int)o, o.ToString())).ToArray();
                
                ImGui.PushID(name);
                if (ImGui.BeginCombo(name, value.ToString()))
                {
                    foreach (var item in items)
                    {
                        bool sel = item.Value == v;
                        if (ImGui.Selectable(item.Name, sel))
                        {
                            success = true;
                            v = item.Value;
                        }
                        if (sel)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
                ImGui.PopID();
                
                if (success) value = v;
                return success;
            } },

            { typeof(EditorGradientAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                ImGui.Text(name);
                if (value is Gradient<Vector4>) ImGradient.Manipulate((Gradient<Vector4>)value);
                else if (value is Gradient<Vector3>) ImGradient.Manipulate((Gradient<Vector3>)value);
                else if (value is Gradient<Vector2>) ImGradient.Manipulate((Gradient<Vector2>)value);
                else if (value is Gradient<float>) ImGradient.Manipulate((Gradient<float>)value);
                else
                {
                    PrintInvalidUI(name);
                }
                return false;
            } },

            { typeof(EditorAssetAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorAssetAttribute)attribute;
                bool success = false;

                var registry = EditorLayer.Instance.Context.Scene?.AssetRegistry;
                AssetInfo v = (AssetInfo)value;
                IAssetCollection assets = registry?.GetCollection(cattribt.AssetType);

                ImGui.PushID(name);
                if (ImGui.BeginCombo("", v != null ? v.RelativePath : "null"))
                {
                    if (assets != null) foreach (var item in assets)
                    {
                        bool sel = item == v;
                        if (ImGui.Selectable(item.RelativePath, sel))
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
                if (ImGuiUtils.SquareButton("×"))
                {
                    v = null;
                    success = true;
                }
                ImGui.SameLine();
                ImGui.Text(name);

                if (success) value = v;
                return success;
            } },

            #region Primitives
            { typeof(EditorBooleanValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorBooleanValueAttribute)attribute;
                bool v = (bool)value;
                bool success = ImGui.Checkbox(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(EditorStringAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorStringAttribute)attribute;
                byte[] v = new byte[cattribt.MaxLength];
                Encoding.UTF8.GetBytes((string)value).CopyTo(v, 0);
                bool success = ImGui.InputText(name, v, cattribt.MaxLength);
                if (success) value = Encoding.UTF8.GetString(v).TrimEnd('\0');
                return success;
            } },
            #endregion
            */
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
            */
            /*
            { typeof(EditorInnerDrawAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorInnerDrawAttribute)attribute;
                if (name != null)
                    ImGui.Text(name);
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
                return EditResult.None;
            } },
            */
        };

        private static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default: throw new Exception();
            }
        }

        // TODO: this mess can't handle null value :(
        public static EditResult DrawEditorValue(MemberInfo memberInfo, object target, Action<Exception> errorCallback = null)
        {
            Debug.Assert(memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property, "Member is not field or property."); // why would anybody do this

            ArgumentNullException.ThrowIfNull(memberInfo);

            var result = EditResult.None;

            var attribs = memberInfo.GetCustomAttributes<EditorValueAttribute>(true);
            // trololo
            Debug.Assert(attribs.Where(a => a.Kind == EditorAttributeType.Edit).Count() <= 1);

            try
            {
                foreach (var attrib in attribs.
                    OrderByDescending(a => a.Kind))
                {
                    Type attribType = attrib.GetType();
                    if (attribType.IsGenericType)
                        attribType = attribType.GetGenericTypeDefinition();

                    string name = (attrib.Name != null) ? attrib.Name : memberInfo.Name;
                    if (!AttributeHandlers.ContainsKey(attribType))
                    {
                        PrintInvalidUI(name);
                    }

                    object value = memberInfo.GetFieldOrPropertyValue(target);

                    result = AttributeHandlers[attribType].Invoke(attrib, name, memberInfo.GetUnderlyingType(), ref value);
                    
                    if ((result & EditResult.Changed) != 0)
                        memberInfo.SetFieldOrPropertyValue(target, value);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex is NotImplementedException)
                    throw;

                Log.Default.Warn($"while drawing ui a wild exception appears:\n{ex}");
                errorCallback?.Invoke(ex);

                return EditResult.None;
            }
        }
    }
}