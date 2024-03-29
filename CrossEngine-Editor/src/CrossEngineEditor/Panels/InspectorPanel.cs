﻿using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Utils.Editor;
using CrossEngine.Assemblies;

using CrossEngineEditor.Utils;
using CrossEngineEditor.Modals;

namespace CrossEngineEditor.Panels
{
    public class InspectorPanel : EditorPanel
    {
        private static readonly Type[] _coreComponents = Assembly.GetAssembly(typeof(Component)).ExportedTypes.
                Where(type => type.IsSubclassOf(typeof(Component)) && !type.IsAbstract).
                ToArray();

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
                        Component SpawnComponent(Type type)
                        {
                            return (Component)Activator.CreateInstance(type);
                        }

                        for (int i = 0; i < _coreComponents.Length; i++)
                        {
                            if (ImGui.MenuItem(_coreComponents[i].Name))
                            {
                                Context.ActiveEntity.AddComponent(SpawnComponent(_coreComponents[i]));
                            }
                        }
                        
                        ImGui.Separator();

                        var comps = AssemblyLoader.GetSubclassesOf(typeof(Component));
                        for (int i = 0; i < comps.Length; i++)
                        {
                            if (ImGui.MenuItem(comps[i].Name))
                            {
                                Context.ActiveEntity.AddComponent(SpawnComponent(comps[i]));
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
            //bool entityEnabled = context.Enabled;
            //if (ImGui.Checkbox("Enabled", ref entityEnabled)) 
            //    context.Enabled = entityEnabled;

            var components = context.Components;

            for (int compi = 0; compi < components.Count; compi++)
            {
                Component component = components[compi];

                Type componentType = component.GetType();

                bool stay = true;

                ImGui.PushID(component.GetHashCode());

                // validity indication
                //bool valcol = !component.Valid;
                //if (valcol)
                //{
                //    ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.412f, 0.118f, 0.118f, 1.0f));
                //    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.729f, 0.208f, 0.208f, 1.0f));
                //}

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
                            int tci = context.GetComponentIndex(targetedComponent);
                            if (tci > context.GetComponentIndex(selectedDragNDropComponent)) tci--;
                            context.ShiftComponent(selectedDragNDropComponent, tci);

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
                            context.RemoveComponent(component);
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

                void UIError(MemberInfo mi, Exception ex, Component component)
                {
                    ImGui.GetStateStorage().SetInt(ImGui.GetID(componentType.Name), 0);
                    EditorLayer.Instance.PushModal(
                        new ActionModal($"UI threw an exception at '{mi.DeclaringType.Name}.{mi.Name}':\n{ex.Message}",
                            "UI Error", ActionModal.ButtonFlags.OK)
                        { Color = ActionModal.TextColor.Error });
                }

                if (collapsingHeader)
                {
                    ImGuiUtils.BeginGroupFrame();

                    MemberInfo[] membs = componentType.GetMembers().
                        Where(m => m.IsDefined(typeof(EditorValueAttribute), true)).
                        ToArray();

                    for (int mi = 0; mi < membs.Length; mi++)
                    {
                        var mem = membs[mi];
                        if (mem.MemberType == MemberTypes.Field || mem.MemberType == MemberTypes.Property)
                            PropertyDrawer.DrawEditorValue(mem, component,
                                (ex) => UIError(mem, ex, component)
                                );
                    }

                    //ImGui.Separator();

                    ImGuiUtils.EndGroupFrame();
                }

                //if (valcol) ImGui.PopStyleColor(2);

                ImGui.PopID();

                if (!stay)
                {
                    context.RemoveComponent(component);
                }
            }
        }
    }
}
