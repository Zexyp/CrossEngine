using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;
using CrossEngine.Entities;
using CrossEngine.Entities.Components;
using CrossEngine.Utils.Editor;

namespace CrossEngineEditor
{
    class InspectorPanel : EditorPanel
    {
        delegate bool EditorValueRepresentationFunction(EditorValueAttribute attribute, string name, ref object value);
        static readonly Dictionary<Type, EditorValueRepresentationFunction> AttributeHandlers = new Dictionary<Type, EditorValueRepresentationFunction>
        {
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
                if (success) value = Encoding.UTF8.GetString(v);
                return success;
            } },
            { typeof(EditorEnumValueAttribute), (EditorValueAttribute attribute, string name, ref object value) => {
                var cattribt = (EditorEnumValueAttribute)attribute;
                bool success;
                int v = (int)value;
                string[] items = Enum.GetValues(value.GetType()).OfType<object>().Select(o => o.ToString()).ToArray();
                success = ImGui.Combo(name, ref v, items, items.Length);
                if (success) value = v;
                return success;
            } },
        };

        List<Type> componentTypes = new List<Type>()
        {
            typeof(TransformComponent),
            typeof(TagComponent),
            typeof(SpriteRendererComponent),
            typeof(CameraComponent),
            typeof(RigidBodyComponent),
            typeof(Box2DColliderComponent),
        };

        public InspectorPanel() : base("Inspector")
        {
            WindowFlags = ImGuiWindowFlags.MenuBar;
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Edit"))
                {
                    if (EditorLayer.Instance.SelectedEntity != null)
                    {
                        if (ImGui.BeginMenu("Add Component"))
                        {
                            for (int i = 0; i < componentTypes.Count; i++)
                            {
                                if (ImGui.MenuItem(componentTypes[i].Name))
                                {
                                    EditorLayer.Instance.SelectedEntity.AddComponent((Component)componentTypes[i].GetConstructor(System.Type.EmptyTypes).Invoke(null));
                                }
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Remove Component"))
                        {
                            for (int i = 0; i < EditorLayer.Instance.SelectedEntity.Components.Count; i++)
                            {
                                if (ImGui.MenuItem(EditorLayer.Instance.SelectedEntity.Components[i].GetType().Name))
                                {
                                    EditorLayer.Instance.SelectedEntity.RemoveComponent(EditorLayer.Instance.SelectedEntity.Components[i]);
                                }
                            }
                            ImGui.EndMenu();
                        }
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (EditorLayer.Instance.SelectedEntity != null) DrawComponents(EditorLayer.Instance.SelectedEntity);
        }

        void DrawComponents(Entity context)
        {
            // TODO: fix
            bool entityEnabled = context.Enabled;
            if (ImGui.Checkbox("Enabled", ref entityEnabled))
                context.Enabled = entityEnabled;

            for (int compi = 0; compi < context.Components.Count; compi++)
            {
                Component component = context.Components[compi];

                Type componentType = component.GetType();

                bool stay = true;

                ImGui.PushID(componentType.Name + compi);

                bool valcol = !component.Valid;
                if (valcol)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.412f, 0.118f, 0.118f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.729f, 0.208f, 0.208f, 1.0f));
                }

                bool collapsingHeader = ImGui.CollapsingHeader(componentType.Name, ref stay);
                float collapsingHeaderButtonOffset = ((ImGui.GetTextLineHeight() + ImGui.GetStyle().CellPadding.Y * 2) + 3);
                ImGui.SameLine(ImGui.GetWindowWidth() - (collapsingHeaderButtonOffset * 2 + ImGui.GetStyle().CellPadding.X));
                bool enabled = component.Enabled;
                if (ImGui.Checkbox("", ref enabled))
                    component.Enabled = enabled;
                ImGui.SameLine(ImGui.GetWindowWidth() - (collapsingHeaderButtonOffset * 3 + ImGui.GetStyle().CellPadding.X));
                if (ImGui.ArrowButton("up", ImGuiDir.Up))
                    context.ShiftComponent(component, Math.Max(0, compi - 1));
                ImGui.SameLine(ImGui.GetWindowWidth() - (collapsingHeaderButtonOffset * 4 + ImGui.GetStyle().CellPadding.X));
                if (ImGui.ArrowButton("down", ImGuiDir.Down))
                    context.ShiftComponent(component, Math.Min(context.Components.Count - 1, compi + 1));

                if (collapsingHeader)
                {
                    

                    // maybe swap this for member info
                    FieldInfo[] fields = componentType.GetFields();
                    PropertyInfo[] props = componentType.GetProperties();

                    for (int fi = 0; fi < fields.Length; fi++)
                    {
                        EditorValueAttribute attrib = fields[fi].GetCustomAttribute<EditorValueAttribute>(true);
                        if (attrib != null)
                        {
                            Type attribType = attrib.GetType();
                            if (AttributeHandlers.ContainsKey(attribType))
                            {
                                object value = fields[fi].GetValue(component);
                                if (AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : fields[fi].Name, ref value))
                                    fields[fi].SetValue(component, value);
                            }
                            else ImGui.Text(fields[fi].Name);
                        }
                    }
                    for (int pi = 0; pi < props.Length; pi++)
                    {
                        EditorValueAttribute attrib = props[pi].GetCustomAttribute<EditorValueAttribute>(true);
                        if (attrib != null)
                        {
                            Type attribType = attrib.GetType();
                            if (AttributeHandlers.ContainsKey(attribType))
                            {
                                object value = props[pi].GetValue(component);
                                if (AttributeHandlers[attribType](attrib, (attrib.Name != null) ? attrib.Name : props[pi].Name, ref value))
                                    props[pi].SetValue(component, value);
                            }
                            else ImGui.Text(props[0].Name);
                        }
                    }
                    ImGui.Separator();
                }

                if (valcol) ImGui.PopStyleColor(2);

                ImGui.PopID();

                if (!stay)
                {
                    EditorLayer.Instance.SelectedEntity.RemoveComponent(component);
                }
            }
        }
    }
}
