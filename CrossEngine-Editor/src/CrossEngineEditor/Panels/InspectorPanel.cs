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
using CrossEngine.Assets;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    class InspectorPanel : EditorPanel
    {
        //static readonly List<Type> ComponentTypes = new List<Type>()
        //{
        //    typeof(TransformComponent),
        //    typeof(TagComponent),
        //    typeof(SpriteRendererComponent),
        //    typeof(CameraComponent),
        //    typeof(RigidBodyComponent),
        //    typeof(Box2DColliderComponent),
        //};

        public InspectorPanel() : base("Inspector")
        {
            WindowFlags = ImGuiWindowFlags.MenuBar;
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Edit", Context.ActiveEntity != null))
                {
                    if (ImGui.BeginMenu("Add Component"))
                    {
                        for (int i = 0; i < EditorLayer.Instance.CoreComponentTypes.Length; i++)
                        {
                            if (ImGui.MenuItem(EditorLayer.Instance.CoreComponentTypes[i].Name))
                            {
                                Context.ActiveEntity.AddComponent<Component>((Component)Activator.CreateInstance(EditorLayer.Instance.CoreComponentTypes[i]));
                            }
                        }
                        ImGui.Separator();
                        for (int i = 0; i < EditorLayer.Instance.ComponentTypeRegistry.Count; i++)
                        {
                            if (ImGui.MenuItem(EditorLayer.Instance.ComponentTypeRegistry[i].Name))
                            {
                                Context.ActiveEntity.AddComponent<Component>((Component)Activator.CreateInstance(EditorLayer.Instance.ComponentTypeRegistry[i]));
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Remove Component"))
                    {
                        var components = Context.ActiveEntity.Components;
                        for (int i = 0; i < components.Count; i++)
                        {
                            if (ImGui.MenuItem(components[i].GetType().Name))
                            {
                                Context.ActiveEntity.RemoveComponent(components[i]);
                            }
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (Context.ActiveEntity != null) DrawComponents(Context.ActiveEntity);
        }

        private Component selectedDragNDropComponent = null;
        private Component targetedComponent = null;

        private void DrawComponents(Entity context)
        {
            // TODO: fix
            bool entityEnabled = context.Enabled;
            if (ImGui.Checkbox("Enabled", ref entityEnabled)) 
                context.Enabled = entityEnabled;

            var components = context.Components;

            for (int compi = 0; compi < components.Count; compi++)
            {
                Component component = components[compi];

                Type componentType = component.GetType();

                bool stay = true;

                ImGui.PushID(componentType.Name + compi);

                bool valcol = !component.Valid;
                if (valcol)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.412f, 0.118f, 0.118f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.729f, 0.208f, 0.208f, 1.0f));
                }

                // item shifting drop
                unsafe
                {
                    // smoll fix
                    bool resetTarget = false;
                    if (targetedComponent == component)
                    {
                        ImGui.Button("##target", new Vector2(ImGui.GetColumnWidth(), 2.5f));
                        if (ImGui.IsItemHovered()) resetTarget = true;
                    }

                    if (ImGui.BeginDragDropTarget())
                    {
                        var payload = ImGui.AcceptDragDropPayload("_COMPONENT");
                        if (payload.NativePtr != null && selectedDragNDropComponent != null)
                        {
                            int tci = Context.ActiveEntity.GetComponentIndex(targetedComponent);
                            if (tci > Context.ActiveEntity.GetComponentIndex(selectedDragNDropComponent)) tci--;
                            Context.ActiveEntity.ShiftComponent(selectedDragNDropComponent, tci);

                            selectedDragNDropComponent = null;
                            targetedComponent = null;
                        }
                        ImGui.EndDragDropTarget();
                    }

                    if (resetTarget) targetedComponent = null;
                }

                bool collapsingHeader = ImGui.CollapsingHeader(componentType.Name, ref stay);

                // item shifting drag
                {
                    if (ImGui.BeginDragDropSource())
                    {
                        ImGui.Text("Component: " + componentType.Name);
                        selectedDragNDropComponent = component;
                        ImGui.SetDragDropPayload("_COMPONENT", IntPtr.Zero, 0);
                        ImGui.EndDragDropSource();
                    }

                    if (ImGui.BeginDragDropTarget())
                    {
                        targetedComponent = component;
                        ImGui.EndDragDropTarget();
                    }
                }

                // contex menu
                {
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Move Up"))
                        {
                            context.ShiftComponent(component, Math.Max(0, compi - 1));
                        }
                        if (ImGui.MenuItem("Move Down"))
                        {
                            context.ShiftComponent(component, Math.Min(components.Count - 1, compi + 1));
                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Remove"))
                        {
                            Context.ActiveEntity.RemoveComponent(component);
                        }

                        ImGui.EndPopup();
                    }
                }

                var style = ImGui.GetStyle();
                float collapsingHeaderButtonOffset = ((ImGui.GetTextLineHeight() + style.FramePadding.Y * 2) + 1);
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 1 + style.FramePadding.X + style.FramePadding.Y));
                bool enabled = component.Enabled;
                if (ImGui.Checkbox("##enabled", ref enabled))
                    component.Enabled = enabled;
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 2 + style.FramePadding.X + style.FramePadding.Y));
                if (ImGui.ArrowButton("up", ImGuiDir.Up))
                    context.ShiftComponent(component, Math.Max(0, compi - 1));
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 3 + style.FramePadding.X + style.FramePadding.Y));
                if (ImGui.ArrowButton("down", ImGuiDir.Down))
                    context.ShiftComponent(component, Math.Min(components.Count - 1, compi + 1));


                if (collapsingHeader)
                {
                    ImGuiUtils.BeginGroupFrame();

                    MemberInfo[] membs = componentType.GetMembers();

                    for (int mi = 0; mi < membs.Length; mi++)
                    {
                        var mem = membs[mi];
                        switch (mem.MemberType)
                        {
                            case MemberTypes.Field:    PropertyDrawer.DrawEditorValue((FieldInfo)mem, component); break;
                            case MemberTypes.Property: PropertyDrawer.DrawEditorValue((PropertyInfo)mem, component); break;
                        }
                    }

                    //ImGui.Separator();

                    ImGuiUtils.EndGroupFrame(0xff363636);
                }

                if (valcol) ImGui.PopStyleColor(2);

                ImGui.PopID();

                if (!stay)
                {
                    Context.ActiveEntity.RemoveComponent(component);
                }
            }
        }
    }
}
