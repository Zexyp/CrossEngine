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

using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.Gui;
using CrossEngineEditor.UndoRedo;
using CrossEngineEditor.Operations;
using CrossEngine.Serialization;

namespace CrossEngineEditor.Utils
{
    static class PropertyDrawerUtilExtensions
    {
        public static void SetFieldOrPropertyValue(this MemberInfo info, object? obj, object? value)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)info).SetValue(obj, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)info).SetValue(obj, value);
                    break;
                default: throw new InvalidOperationException();
            }
        }

        public static object? GetFieldOrPropertyValue(this MemberInfo info, object? obj)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)info).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo)info).GetValue(obj);
                default: throw new InvalidOperationException();
            }
        }
    }

    public static class PropertyDrawer
    {
        /*
        class PropertyDrawerSerializationInfo : SerializationInfo
        {
            private Dictionary<string, object> _data = new Dictionary<string, object>();
            private Stack<int> _prefixStack = new Stack<int>();
            private string _prefix = "";

            public PropertyDrawerSerializationInfo() : base(OperationState.Undefined)
            {

            }

            public void MarkRead() => State = OperationState.Read;
            public void MarkWrite() => State = OperationState.Write;
            public void PushID(int id)
            {
                _prefixStack.Push(id);
                RebuildPrefix();
            }
            public void PopID()
            {
                _prefixStack.Pop();
                RebuildPrefix();
            }

            public override void AddValue(string name, object value)
            {
                if (State != OperationState.Write) throw new InvalidOperationException();
                _data.Add(_prefix + name, value);
            }

            public override object GetValue(string name, Type typeOfValue)
            {
                if (State != OperationState.Read) throw new InvalidOperationException();
                return _data[_prefix + name];
            }

            public override bool TryGetValue(string name, Type typeOfValue, out object value)
            {
                if (State != OperationState.Read) throw new InvalidOperationException();
                value = null;
                if (!_data.ContainsKey(_prefix + name))
                    return false;
                value = _data[_prefix + name];
                return true;
            }

            private unsafe void RebuildPrefix()
            {
                _prefix = "";
                foreach (var item in _prefixStack)
                {
                    _prefix += item;
                }
            }
        }
        */

        [Flags]
        enum EditResult
        {
            None = 0,
            Changed = 1 << 0,
            DoneEditing = 1 << 1,
            Ended = 1 << 2,
            Started = 1 << 3,
        }

        delegate EditResult EditorValueRepresentationFunction(EditorValueAttribute attribute, string name, ref object value);

        delegate (bool, object) EditorValueFunction(string name, object value);
        delegate (bool, object) EditorRangeFunction(IValueRange range, string name, object value);
        delegate (bool, object) EditorDragFunction(ISteppedValueRange range, string name, object value);
        delegate (bool, object) EditorSliderFunction(IValueRange range, string name, object value);
        delegate (bool, object) EditorColorFunction(EditorColorAttribute attribute, string name, object value);

        private static void PrintInvalidUI(string name)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            ImGui.Text($"{name} (invalid UI)");
            ImGui.PopStyleColor();
        }

        private static EditResult AssembleResult()
        {
            EditResult editResult = 0;
            editResult |= ImGui.IsItemDeactivatedAfterEdit() ? EditResult.DoneEditing : 0;
            editResult |= ImGui.IsItemActivated() ? EditResult.Started : 0;
            editResult |= ImGui.IsItemDeactivated() ? EditResult.Ended : 0;
            return editResult;
        }

        private static EditResult ExecuteFromDict(IDictionary dict, string name, ref object value, Attribute attribute = null)
        {
            if (dict.Contains(value.GetType()))
            {
                var result = ((bool Success, object Value))(attribute == null ? ((Delegate)dict[value.GetType()]).DynamicInvoke(name, value) :
                                                                                ((Delegate)dict[value.GetType()]).DynamicInvoke(attribute, name, value));
                
                value = result.Value;

                EditResult editResult = AssembleResult();
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
            { typeof(int), (IValueRange range, string name, object value) => {
                int v = (int)value;
                bool success;
                success = ImGui.InputInt(name, ref v);
                if (success) value = Math.Clamp(v, (int)range.Min, (int)range.Max);
                return (success, value);
            } },
            { typeof(float), (IValueRange range, string name, object value) => {
                float v = (float)value;
                bool success;
                success = ImGui.InputFloat(name, ref v);
                if (success) value = Math.Clamp(v, range.Min, range.Max);
                return (success, value);
            } },
            { typeof(Vector2), (IValueRange range, string name, object value) => {
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.InputFloat2(name, ref v);
                if (success) value = Vector2.Clamp(v, new Vector2(range.Min), new Vector2(range.Max));
                return (success, value);
            } },
            { typeof(Vector3), (IValueRange range, string name, object value) => {
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.InputFloat3(name, ref v);
                if (success) value = Vector3.Clamp(v, new Vector3(range.Min), new Vector3(range.Max));
                return (success, value);
            } },
            { typeof(Vector4), (IValueRange range, string name, object value) => {
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.InputFloat4(name, ref v);
                if (success) value = Vector4.Clamp(v, new Vector4(range.Min), new Vector4(range.Max));
                return (success, value);
            } },
        };
        static readonly Dictionary<Type, EditorSliderFunction> SimpleSliderHandlers = new Dictionary<Type, EditorSliderFunction>()
        {
            { typeof(int), (IValueRange range, string name, object value) => {
                int v = (int)value;
                bool success;
                success = ImGui.SliderInt(name, ref v, (int)range.Min, (int)range.Max);
                if (success) value = Math.Clamp(v, (int)range.Min, (int)range.Max);
                return (success, value);
            } },
            { typeof(float), (IValueRange range, string name, object value) => {
                float v = (float)value;
                bool success;
                success = ImGui.SliderFloat(name, ref v, range.Min, range.Max);
                if (success) value = Math.Clamp(v, range.Min, range.Max);
                return (success, value);
            } },
            { typeof(Vector2), (IValueRange range, string name, object value) => {
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.SliderFloat2(name, ref v, range.Min, range.Max);
                if (success) value = Vector2.Clamp(v, new Vector2(range.Min), new Vector2(range.Max));
                return (success, value);
            } },
            { typeof(Vector3), (IValueRange range, string name, object value) => {
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.SliderFloat3(name, ref v, range.Min, range.Max);
                if (success) value = Vector3.Clamp(v, new Vector3(range.Min), new Vector3(range.Max));
                return (success, value);
            } },
            { typeof(Vector4), (IValueRange range, string name, object value) => {
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.SliderFloat4(name, ref v, range.Min, range.Max);
                if (success) value = Vector4.Clamp(v, new Vector4(range.Min), new Vector4(range.Max));
                return (success, value);
            } },
        };
        static readonly Dictionary<Type, EditorDragFunction> SimpleDragHandlers = new Dictionary<Type, EditorDragFunction>()
        {
            { typeof(uint), (ISteppedValueRange range, string name, object value) => {
                int v = (int)(uint)value;
                bool success;
                success = ImGui.DragInt(name, ref v, range.Step, (int)Math.Max(range.Min, 0), (int)range.Max);
                if (success) value = (uint)Math.Clamp(v, range.Min, range.Max);
                return (success, value);
            } },
            { typeof(int), (ISteppedValueRange range, string name, object value) => {
                int v = (int)value;
                bool success;
                success = ImGui.DragInt(name, ref v, range.Step, (int)range.Min, (int)range.Max);
                if (success) value = Math.Clamp(v, (int)range.Min, (int)range.Max);
                return (success, value);
            } },
            { typeof(float), (ISteppedValueRange range, string name, object value) => {
                float v = (float)value;
                bool success;
                success = ImGui.DragFloat(name, ref v, range.Step, range.Min, range.Max);
                if (success) value = Math.Clamp(v, range.Min, range.Max);
                return (success, value);
            } },
            { typeof(Vector2), (ISteppedValueRange range, string name, object value) => {
                Vector2 v = (Vector2)value;
                bool success;
                success = ImGui.DragFloat2(name, ref v, range.Step, range.Min, range.Max);
                if (success) value = Vector2.Clamp(v, new Vector2(range.Min), new Vector2(range.Max));
                return (success, value);
            } },
            { typeof(Vector3), (ISteppedValueRange range, string name, object value) => {
                Vector3 v = (Vector3)value;
                bool success;
                success = ImGui.DragFloat3(name, ref v, range.Step, range.Min, range.Max);
                if (success) value = Vector3.Clamp(v, new Vector3(range.Min), new Vector3(range.Max));
                return (success, value);
            } },
            { typeof(Vector4), (ISteppedValueRange range, string name, object value) => {
                Vector4 v = (Vector4)value;
                bool success;
                success = ImGui.DragFloat4(name, ref v, range.Step, range.Max, range.Min);
                if (success) value = Vector4.Clamp(v, new Vector4(range.Min), new Vector4(range.Max));
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
            { typeof(EditorHintAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                throw new NotImplementedException();
            } },

            { typeof(EditorDisplayAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                ImGui.Text($"{(name != null ? $"{name}: " : "")}{value.ToString()}");
                return EditResult.None;
            } },

            { typeof(EditorValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                return ExecuteFromDict(SimpleValueHandlers, name, ref value);
            } },

            { typeof(EditorRangeAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                return ExecuteFromDict(SimpleRangeHandlers, name, ref value, attribute);
            } },
            { typeof(EditorSliderAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                return ExecuteFromDict(SimpleSliderHandlers, name, ref value, attribute);
            } },
            { typeof(EditorDragAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                return ExecuteFromDict(SimpleDragHandlers, name, ref value, attribute);
            } },

            { typeof(EditorColorAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                return ExecuteFromDict(SimpleColorHandlers, name, ref value, attribute);
            } },

            { typeof(EditorSectionAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                ImGui.Text(name);
                ImGui.SameLine();
                ImGuiUtils.SmartSeparator(3);
                return EditResult.None;
            } },

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
                return success ? EditResult.Started | EditResult.Ended | EditResult.DoneEditing | EditResult.Changed : EditResult.None;
            } },

            { typeof(EditorGradientAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                ImGui.Text(name);
                if (value is Gradient<Vector4>) ImGradient.Manipulate((Gradient<Vector4>)value);
                else if (value is Gradient<Vector3>) ImGradient.Manipulate((Gradient<Vector3>)value);
                else if (value is Gradient<Vector2>) ImGradient.Manipulate((Gradient<Vector2>)value);
                else if (value is Gradient<float>) ImGradient.Manipulate((Gradient<float>)value);
                else
                {
                    Debug.Assert(false);
                    PrintInvalidUI(name);
                }
                if (ImGui.IsItemActivated())
                    Console.WriteLine("ac");
                if (ImGui.IsItemDeactivated())
                    Console.WriteLine("deac");
                if (ImGui.IsItemDeactivatedAfterEdit())
                    Console.WriteLine("edi dun");
                return EditResult.None;
            } },
            /*
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
            */

            #region Primitives
            { typeof(EditorBooleanValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorBooleanValueAttribute)attribute;
                bool v = (bool)value;
                bool success = ImGui.Checkbox(name, ref v);
                if (success) value = v;
                return AssembleResult() | (success ? EditResult.Changed : EditResult.None);
            } },
            { typeof(EditorStringAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorStringAttribute)attribute;
                byte[] v = new byte[cattribt.MaxLength];
                Encoding.UTF8.GetBytes((string)value).CopyTo(v, 0);
                bool success = ImGui.InputText(name, v, cattribt.MaxLength);
                if (success) value = Encoding.UTF8.GetString(v).TrimEnd('\0');
                return AssembleResult() | (success ? EditResult.Changed : EditResult.None);
            } },
            #endregion

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
        };

        //static ValueChangedOperation _inprogress = null;
        // the order of drawing matters so this should fix it but i think this will break it
        static Queue<MemberValueChangeOperation> _inprogops = new Queue<MemberValueChangeOperation>();

        // TODO: this mess can't handle null value :(
        public static void DrawEditorValue(MemberInfo memberInfo, object target, Action<Exception> errorCallback = null, IOperationHistory history = null)
        {
            Debug.Assert(memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property, "Member is not field or property.");

            if (memberInfo == null)
            {
                PrintInvalidUI("null");
                return;
            }

            try
            {
                foreach (var attrib in memberInfo.GetCustomAttributes<EditorValueAttribute>(true).
                    OrderByDescending(a => a.Kind))
                {
                    Type attribType = attrib.GetType();
                    if (AttributeHandlers.ContainsKey(attribType))
                    {
                        object value = memberInfo.GetFieldOrPropertyValue(target);
                        object prevvalue = value;
                        var result = AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : memberInfo.Name, ref value);
                        if ((result & EditResult.Changed) != 0)
                            memberInfo.SetFieldOrPropertyValue(target, value);

                        if (history != null)
                        {
                            /*
                            if ((result & EditResult.DoneEditing) != 0)
                            {
                                Debug.Assert(_inprogress != null);
                                Debug.Assert(_inprogress.Member == memberInfo);
                                Debug.Assert(_inprogress.Target == target);

                                _inprogress.NextValue = value;
                                history.Push(_inprogress);
                            }
                            if ((result & EditResult.Ended) != 0)
                            {
                                Debug.Assert(_inprogress != null);
                                _inprogress = null;
                            }
                            if ((result & EditResult.Started) != 0)
                            {
                                Debug.Assert(_inprogress == null);

                                _inprogress = new ValueChangedOperation();
                                _inprogress.Member = memberInfo;
                                _inprogress.Target = target;
                                _inprogress.PreviousValue = value;
                            }
                            */
                            Debug.Assert(_inprogops.Count <= 2);
                            
                            if ((result & EditResult.DoneEditing) != 0)
                            {
                                var op = _inprogops.Peek();
                                op.NextValue = value;
                                history.Push(op);
                            }
                            if ((result & EditResult.Ended) != 0)
                            {
                                _inprogops.Dequeue();
                            }
                            if ((result & EditResult.Started) != 0)
                            {
                                var op = new MemberValueChangeOperation();
                                op.Member = memberInfo;
                                op.Target = target;
                                op.PreviousValue = prevvalue;

                                _inprogops.Enqueue(op);
                            }
                        }
                    }
                    else
                    {
                        PrintInvalidUI((attrib.Name != null) ? attrib.Name : memberInfo.Name);
                    }
                }
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Application.Log.Warn($"while drawing ui a wild exception appears:\n{ex}");
                errorCallback?.Invoke(ex);
            }
        }
    }
}
