using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Utils.Editor;

using CrossEngineEditor.Utils;
using CrossEngineEditor.Modals;
using CrossEngineEditor.Operations;

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
                                var comp = SpawnComponent(_coreComponents[i]);
                                AddComponent(Context.ActiveEntity, comp);
                            }
                        }
                        
                        ImGui.Separator();

                        //var comps = AssemblyLoader.GetSubclassesOf(typeof(Component));
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

        private const string DragNDropIdentifier = nameof(Component);

        private void RemoveComponent(Entity entity, Component component)
        {
            var op = new ComponentRemoveOperation(entity, component, entity.GetComponentIndex(component));
            Context.Operations?.Push(op);

            entity.RemoveComponent(component);
        }

        private void AddComponent(Entity entity, Component component)
        {
            var op = new ComponentAddOperation(entity, component);
            Context.Operations?.Push(op);

            entity.AddComponent(component);
        }

        private void ShiftComponent(Entity entity, Component component, int index)
        {
            var op = new ComponentShiftOperation(entity, component, entity.GetComponentIndex(component), index);
            Context.Operations?.Push(op);

            entity.ShiftComponent(component, index);
        }

        private void EnableComponent(Component component, bool enabled)
        {
            var op = new ComponentEnabledChangeOperation(component, component.Enabled, enabled);
            Context.Operations?.Push(op);

            component.Enabled = enabled;
        }

        private void DrawComponents(Entity contextEntity)
        {
            // TODO: fix
            //bool entityEnabled = context.Enabled;
            //if (ImGui.Checkbox("Enabled", ref entityEnabled)) 
            //    context.Enabled = entityEnabled;

            var components = contextEntity.Components;
            for (int compi = 0; compi < components.Count; compi++)
            {
                Component component = components[compi];
                Type componentType = component.GetType();
                bool componentStay = true;

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
                        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left)) resetTarget = true;
                    }

                    if (ImGui.BeginDragDropTarget())
                    {
                        var payload = ImGui.AcceptDragDropPayload(DragNDropIdentifier);
                        if (payload.NativePtr != null && selectedDragNDropComponent != null)
                        {
                            int tci = contextEntity.GetComponentIndex(targetedComponent);
                            if (tci > contextEntity.GetComponentIndex(selectedDragNDropComponent)) tci--;
                            contextEntity.ShiftComponent(selectedDragNDropComponent, tci);

                            selectedDragNDropComponent = null;
                            targetedComponent = null;
                        }
                        ImGui.EndDragDropTarget();
                    }

                    if (resetTarget) targetedComponent = null;
                }

                bool collapsingHeader = ImGui.CollapsingHeader(componentType.Name, ref componentStay);

                // item shifting drag
                {
                    if (ImGui.BeginDragDropSource())
                    {
                        ImGui.Text("Component: " + componentType.Name);
                        selectedDragNDropComponent = component;
                        ImGui.SetDragDropPayload(DragNDropIdentifier, IntPtr.Zero, 0);
                        ImGui.EndDragDropSource();
                    }

                    if (ImGui.BeginDragDropTarget())
                    {
                        targetedComponent = component;
                        ImGui.EndDragDropTarget();
                    }
                }

                // draw header augmentation
                {
                    var style = ImGui.GetStyle();
                    float collapsingHeaderButtonOffset = ((ImGui.GetTextLineHeight() + style.FramePadding.Y * 2) + 1);
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 1 + style.FramePadding.X + style.FramePadding.Y));
                    bool enabled = component.Enabled;
                    if (ImGui.Checkbox("##enabled", ref enabled))
                        EnableComponent(component, enabled);
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 2 + style.FramePadding.X + style.FramePadding.Y));
                    if (ImGui.ArrowButton("up", ImGuiDir.Up))
                        if (compi > 0) ShiftComponent(contextEntity, component, compi - 1);
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 3 + style.FramePadding.X + style.FramePadding.Y));
                    if (ImGui.ArrowButton("down", ImGuiDir.Down))
                        if (compi < components.Count) ShiftComponent(contextEntity, component, compi + 1);
                }

                if (collapsingHeader)
                {
                    ImGuiUtils.BeginGroupFrame();

                    // draw component members
                    MemberInfo[] membs = componentType.GetMembers().
                        Where(m => m.IsDefined(typeof(EditorValueAttribute), true)).
                        ToArray();

                    // defined to save memory allocation for each member
                    void UIError(MemberInfo mi, Exception ex, Component component)
                    {
                        // close header
                        ImGui.GetStateStorage().SetInt(ImGui.GetID(componentType.Name), 0);
                        
                        // notify
                        EditorLayer.Instance.PushModal(
                            new ActionModal($"UI threw an exception at '{mi.DeclaringType.Name}.{mi.Name}':\n{ex.Message}",
                                "UI Error", ActionModal.ButtonFlags.OK)
                            { Color = ActionModal.TextColor.Error });
                    }

                    for (int mi = 0; mi < membs.Length; mi++)
                    {
                        var mem = membs[mi];
                        if (mem.MemberType == MemberTypes.Field || mem.MemberType == MemberTypes.Property)
                            PropertyDrawer.DrawEditorValue(mem, component,
                                (ex) => UIError(mem, ex, component),
                                Context.Operations
                                );
                    }

                    //ImGui.Separator();

                    ImGuiUtils.EndGroupFrame();
                }

                //if (valcol) ImGui.PopStyleColor(2);

                ImGui.PopID();

                if (!componentStay)
                {
                    RemoveComponent(contextEntity, component);
                }
            }
        }
    }
}
