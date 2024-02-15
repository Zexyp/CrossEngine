using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using ImGuiNET;

using CrossEngine.Logging;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngineEditor.Utils.Reflection;
using System.Xml.Linq;
using Silk.NET.Core.Native;
using CrossEngine.Assets;

namespace CrossEngineEditor.Utils
{
    public static class InspectDrawer
    {
        [Flags]
        public enum EditResult
        {
            None = 0,
            Changed = 1 << 0,
            DoneEditing = 1 << 1,
            Ended = 1 << 2,
            Started = 1 << 3,
            Full = Started | Ended | Changed | DoneEditing,
        }

        public delegate void EditResultHandler(MemberInfo member, object target, EditResult result);

        public static EditResult DrawMember(MemberInfo memberInfo, object target, Action<Exception> errorCallback = null)
        {
            ArgumentNullException.ThrowIfNull(memberInfo);

            var attribs = memberInfo.GetCustomAttributes<EditorValueAttribute>(true);
            
            Debug.Assert(memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property, "Member must be field or property."); // why would anybody do this
            Debug.Assert(attribs.Count(a => a.Kind == EditorAttributeType.Edit) <= 1);

            var result = EditResult.None;
            try
            {
                var type = memberInfo.GetUnderlyingType();
                var value = memberInfo.GetFieldOrPropertyValue(target);
                foreach (var attrib in attribs.OrderByDescending(a => a.Kind))
                {
                    var attribType = attrib.GetType();
                    if (attribType.IsGenericType)
                        attribType = attribType.GetGenericTypeDefinition();

                    if (!AttributeHandlers.ContainsKey(attribType))
                        throw new NotImplementedException();

                    result = AttributeHandlers[attribType].Invoke(attrib, type, memberInfo.Name, ref value);

                    if ((result & EditResult.Changed) != 0)
                        memberInfo.SetFieldOrPropertyValue(target, value);

                    //if (result != EditResult.None)
                    //    Console.WriteLine(result);
                }
                return result;
            }
            catch (Exception ex)
            {
                PrintInvalidUI($"{memberInfo.GetUnderlyingType().FullName}.{memberInfo.Name}");

                Log.Default.Warn($"while drawing ui a wild exception appears:\n{ex}");

                errorCallback?.Invoke(ex);

                return EditResult.None;
            }
        }

        public static void Inspect(object target, Type type = null, EditResultHandler editResultHandler = null)
        {
            Debug.Assert(target != null);

            type = type ?? target.GetType();
            var membs = type.GetMembers();
            for (int mi = 0; mi < membs.Length; mi++)
            {
                var memb = membs[mi];
                if (Attribute.IsDefined(memb, typeof(EditorValueAttribute), true))
                {
                    var result = InspectDrawer.DrawMember(memb, target);
                    editResultHandler?.Invoke(memb, target, result);
                }
            }
        }

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

        private static void PrintInvalidUI(string name)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            ImGui.Text($"Broken: {name}");
            ImGui.PopStyleColor();
        }

        delegate bool EditorValueFunction(EditorValueAttribute attrib, string name, ref object value);
        delegate bool EditorRangeFunction(EditorRangeAttribute range, string name, ref object value);
        delegate bool EditorDragFunction(EditorDragAttribute range, string name, ref object value);
        delegate bool EditorSliderFunction(EditorSliderAttribute range, string name, ref object value);
        delegate bool EditorColorFunction(EditorColorAttribute attrib, string name, ref object value);
        private delegate EditResult EditorValueRepresentationFunction(EditorValueAttribute attribute, Type type, string name, ref object value);

        // should be simple - IsItemDeactivatedAfterEdit will be invoked
        private static readonly Dictionary<Type, EditorValueFunction> SimpleValueHandlers = new Dictionary<Type, EditorValueFunction>()
        {
            { typeof(bool), (EditorValueAttribute attrib, string name, ref object value) => {
                bool v = (bool)value;
                bool success;
                success = ImGui.Checkbox(name, ref v);
                if (success) value = v;
                return success;
            } },

            { typeof(int), (EditorValueAttribute attrib, string name, ref object value) => {
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(IntVec2), (EditorValueAttribute attrib, string name, ref object value) => {
                IntVec2 v = (IntVec2)value;
                bool success;
                success = ImGui.InputInt2(name, ref v.X);
                if (success) value = v;
                return success;
            } },
            { typeof(IntVec3), (EditorValueAttribute attrib, string name, ref object value) => {
                IntVec3 v = (IntVec3)value;
                bool success;
                success = ImGui.InputInt2(name, ref v.X);
                if (success) value = v;
                return success;
            } },
            { typeof(IntVec4), (EditorValueAttribute attrib, string name, ref object value) => {
                IntVec4 v = (IntVec4)value;
                bool success;
                success = ImGui.InputInt2(name, ref v.X);
                if (success) value = v;
                return success;
            } },

            { typeof(float), (EditorValueAttribute attrib, string name, ref object value) => {
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(double), (EditorValueAttribute attrib, string name, ref object value) => {
                double v = (double)value;
                bool success;
                success = ImGui.InputDouble(name, ref v);
                if (success) value = v;
                return success;
            } },

            { typeof(Vector2), (EditorValueAttribute attrib, string name, ref object value) => {
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector3), (EditorValueAttribute attrib, string name, ref object value) => {
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector4), (EditorValueAttribute attrib, string name, ref object value) => {
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = v;
                return success;
            } },
        };
        private static readonly Dictionary<Type, EditorRangeFunction> RangeValueHandlers = new Dictionary<Type, EditorRangeFunction>()
        {
            { typeof(int), (EditorRangeAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return success;
            } },
            { typeof(float), (EditorRangeAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return success;
            } },
            { typeof(Vector2), (EditorRangeAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = Vector2.Clamp(v, new Vector2(trange.Min), new Vector2(trange.Max));
                return success;
            } },
            { typeof(Vector3), (EditorRangeAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = Vector3.Clamp(v, new Vector3(trange.Min), new Vector3(trange.Max));
                return success;
            } },
            { typeof(Vector4), (EditorRangeAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = Vector4.Clamp(v, new Vector4(trange.Min), new Vector4(trange.Max));
                return success;
            } },
        };
        private static readonly Dictionary<Type, EditorSliderFunction> SliderValueHandlers = new Dictionary<Type, EditorSliderFunction>()
        {
            { typeof(int), (EditorSliderAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.SliderInt(name, ref v, (int)trange.Min, (int)trange.Max);
                if (success) value = Math.Clamp(v, (int)trange.Min, (int)trange.Max);
                return success;
            } },
            { typeof(float), (EditorSliderAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.SliderFloat(name, ref v, trange.Min, trange.Max);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return success;
            } },
            { typeof(Vector2), (EditorSliderAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.SliderFloat2(name, ref v, trange.Min, trange.Max);
                if (success) value = Vector2.Clamp(v, new Vector2(trange.Min), new Vector2(trange.Max));
                return success;
            } },
            { typeof(Vector3), (EditorSliderAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.SliderFloat3(name, ref v, trange.Min, trange.Max);
                if (success) value = Vector3.Clamp(v, new Vector3(trange.Min), new Vector3(trange.Max));
                return success;
            } },
            { typeof(Vector4), (EditorSliderAttribute range, string name, ref object value) => {
                var trange = (IRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.SliderFloat4(name, ref v, trange.Min, trange.Max);
                if (success) value = Vector4.Clamp(v, new Vector4(trange.Min), new Vector4(trange.Max));
                return success;
            } },
        };
        private static readonly Dictionary<Type, EditorDragFunction> DragValueHandlers = new Dictionary<Type, EditorDragFunction>()
        {
            { typeof(uint), (EditorDragAttribute range, string name, ref object value) => {
                var trange = (ISteppedRangeValue<uint>)range;
                int v = (int)(uint)value;
                bool success;
                success = ImGui.DragInt(name, ref v, trange.Step, (int)Math.Max(trange.Min, 0), (int)trange.Max);
                if (success) value = (uint)Math.Clamp(v, trange.Min, trange.Max);
                return success;
            } },
            { typeof(int), (EditorDragAttribute range, string name, ref object value) => {
                var trange = (ISteppedRangeValue<int>)range;
                int v = (int)value;
                bool success;
                success = ImGui.DragInt(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return success;
            } },
            { typeof(float), (EditorDragAttribute range, string name, ref object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                float v = (float)value;
                bool success;
                success = ImGui.DragFloat(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Math.Clamp(v, trange.Min, trange.Max);
                return success;
            } },
            { typeof(Vector2), (EditorDragAttribute range, string name, ref object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.DragFloat2(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Vector2.Clamp(v, new Vector2(trange.Min), new Vector2(trange.Max));
                return success;
            } },
            { typeof(Vector3), (EditorDragAttribute range, string name, ref object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.DragFloat3(name, ref v, trange.Step, trange.Min, trange.Max);
                if (success) value = Vector3.Clamp(v, new Vector3(trange.Min), new Vector3(trange.Max));
                return success;
            } },
            { typeof(Vector4), (EditorDragAttribute range, string name, ref object value) => {
                var trange = (ISteppedRangeValue<float>)range;
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.DragFloat4(name, ref v, trange.Step, trange.Max, trange.Min);
                if (success) value = Vector4.Clamp(v, new Vector4(trange.Min), new Vector4(trange.Max));
                return success;
            } },
        };
        private static readonly Dictionary<Type, EditorColorFunction> ColorValueHandlers = new Dictionary<Type, EditorColorFunction>()
        {
            { typeof(Vector3), (EditorColorAttribute attribute, string name, ref object value) => {
                Vector3 v = (Vector3)value;
                bool success = ImGui.ColorEdit3(name, ref v, attribute.HDR ? ImGuiColorEditFlags.HDR : 0);
                if (success) value = v;
                return success;
            } },
            { typeof(Vector4), (EditorColorAttribute attribute, string name, ref object value) => {
                Vector4 v = (Vector4)value;
                bool success = ImGui.ColorEdit4(name, ref v, attribute.HDR ? ImGuiColorEditFlags.HDR : 0);
                if (success) value = v;
                return success;
            } },
        };

        static readonly Dictionary<Type, EditorValueRepresentationFunction> AttributeHandlers = new Dictionary<Type, EditorValueRepresentationFunction>()
        {
            { typeof(EditorHintAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                throw new NotImplementedException();
            } },

            { typeof(EditorDisplayAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                ImGui.Text($"{(name != null ? $"{name}: " : "")}{value}");
                return EditResult.None;
            } },

            { typeof(EditorValueAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                return ExecuteFromDict(SimpleValueHandlers, attribute, type, name, ref value);
            } },

            { typeof(EditorRangeAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                return ExecuteFromDict(RangeValueHandlers, attribute, type, name, ref value);
            } },
            { typeof(EditorSliderAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                return ExecuteFromDict(SliderValueHandlers, attribute, type, name, ref value);
            } },
            { typeof(EditorDragAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                return ExecuteFromDict(DragValueHandlers, attribute, type, name, ref value);
            } },

            { typeof(EditorColorAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                return ExecuteFromDict(ColorValueHandlers, attribute, type, name, ref value);
            } },

            { typeof(EditorSectionAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                ImGui.SeparatorText(name);
                return EditResult.None;
            } },

            { typeof(EditorStringAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                var cattrib = (EditorStringAttribute)attribute;
                var style = ImGui.GetStyle();
                var v = (string)value;
                float square = ImGui.GetTextLineHeight() + style.FramePadding.Y * 2;
                
                bool pushedColor = v == null;
                if (pushedColor) ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000ff);

                var t = (v != null) ? v : "<null>";
                byte[] buffer = new byte[cattrib.MaxLength + 1];
                Encoding.Default.GetBytes(t, buffer);

                var result = ImGui.InputText(name, buffer, cattrib.MaxLength + 1);
                var er = EvalResult(result);
                
                if (result)
                {
                    // tractooooor
                    //  ,-,---,
                    //  |_|___|  Y
                    //  |/``\ |--'-q  _
                    // {( () ) {(===t||
                    //   \__/````\_/
                    //
                    // i hate my code

                    var buffCont = Encoding.Default.GetString(buffer).TrimEnd('\0');
                    var newVal = "";
                    for (int i = 0; i < buffCont.Length && buffCont[i] != '\0'; i++)
                    {
                        newVal += buffCont[i];
                    }
                    value = newVal;
                }

                if (pushedColor) ImGui.PopStyleColor();

                return er;
            } },

            { typeof(EditorEnumAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                EditResult er = EditResult.None;

                if (ImGui.BeginCombo(name, value.ToString()))
                {
                    var values = Enum.GetValues(value.GetType());
                    for (int i = 0; i < values.Length; i++)
                    {
                        var ev = values.GetValue(i);
                        bool isSelected = Enum.Equals(ev, value);
                        if (ImGui.Selectable(ev.ToString(), isSelected))
                        {
                            value = ev;
                            er = EditResult.Full;
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }

                return er;
            } },

            { typeof(EditorGuidAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                object strVal = value.ToString();
                
                EditResult er = AttributeHandlers[typeof(EditorStringAttribute)].Invoke(attribute, typeof(string), name, ref strVal);

                if (((er & EditResult.Changed) != 0) && Guid.TryParse((string)strVal, out var guid))
                {
                    value = guid;
                }
                else
                {
                    er &= ~EditResult.Changed;
                }

                return er;
            } },

            { typeof(EditorAssetAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                EditResult er = EditResult.None;
                var v = (Asset)value;

                if (ImGui.BeginCombo(name, v?.Id.ToString()))
                {
                    var coll = AssetManager.Current?.GetCollection(type);

                    if (coll != null)
                        foreach (Asset item in coll)
                        {
                            bool isSelected = item == v;
                            if (ImGui.Selectable(item.Id.ToString(), isSelected))
                            {
                                value = item;
                                er = EditResult.Full;
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                    ImGui.EndCombo();
                }

                return er;
            } },

            { typeof(EditorNullableAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
                ImGui.SameLine();

                if (ImGui.Button("×") && value != null)
                {
                    value = null;
                    return EditResult.Full;
                }

                return EditResult.None;
            } },
            //{ typeof(EditorPathAttribute), (EditorValueAttribute attribute, Type type, string name, ref object value) => {
            //    ImGui.SameLine();
            //
            //    if (ImGui.Button("..."))
            //    {
            //        var newVal = ShellFileDialogs.FileOpenDialog.ShowSingleSelectDialog(0, null, null, null, null, null);
            //        if (newVal != null)
            //        {
            //            value = newVal;
            //            return EditResult.Full;
            //        }
            //    }
            //
            //    return EditResult.None;
            //} }
        };

        private static EditResult ExecuteFromDict(IDictionary dict, EditorValueAttribute attrib, Type type, string name, ref object value)
        {
            if (!dict.Contains(type))
                throw new NotImplementedException();

            object[] prms = new[] { attrib, name, value };
            var result = (bool)((Delegate)dict[type]).DynamicInvoke(prms);
            value = prms[2];

            var editResult = EvalResult(result);

            return editResult;
        }

        private static EditResult EvalResult(bool edited)
        {
            EditResult editResult = 0;
            editResult |= ImGui.IsItemDeactivatedAfterEdit() ? EditResult.DoneEditing : 0;
            editResult |= ImGui.IsItemActivated() ? EditResult.Started : 0;
            editResult |= ImGui.IsItemDeactivated() ? EditResult.Ended : 0;
            editResult |= edited ? EditResult.Changed : 0;
            return editResult;
        }
    }
}
