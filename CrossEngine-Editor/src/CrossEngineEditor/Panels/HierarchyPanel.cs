using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine.Logging;
using CrossEngine.ECS;
using CrossEngine.Utils;
using CrossEngine.Components;

namespace CrossEngineEditor.Panels
{
    public class HierarchyPanel : EditorPanel
    {
        private const string DragDropIdentifier = "_TREENODE<ENTITY>";

        public HierarchyPanel() : base("Outliner")
        {
            WindowFlags = ImGuiWindowFlags.MenuBar;
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Edit", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Add Entity"))
                    {
                        if (Context != null)
                            Context.Scene.CreateEntity();
                    }
                    if (ImGui.MenuItem("Remove Entity", Context.ActiveEntity != null))
                    {
                        Context.Scene.DestroyEntity(Context.ActiveEntity);
                        Context.ActiveEntity = null;
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (Context.Scene != null)
            {
                var ents = Context.Scene.HierarchyRoot;
                for (int i = 0; i < ents.Count; i++)
                {
                    DrawEntityNode(ents[i], i.ToString());
                }
            }
        }

        Entity selectedDragNDropNode = null;
        Entity targetedNode = null;

        void DrawEntityNode(Entity node, string id, int i = 0)
        {
            ImGui.PushID(node.Id);
            // drag drop
            unsafe
            {
                // smoll fix
                bool resetTarget = false;
                // draw drop target
                if (targetedNode == node && targetedNode != null)
                {
                    ImGui.Button("##target", new Vector2(ImGui.GetColumnWidth(), 2.5f));
            
                    if (ImGui.IsItemHovered()) resetTarget = true;
                }
            
                // dropped next to
                if (ImGui.BeginDragDropTarget())
                {
                    var payload = ImGui.AcceptDragDropPayload(DragDropIdentifier);
                    if (payload.NativePtr != null && selectedDragNDropNode != null)
                    {
                        EditorApplication.Log.Trace("dropped " +
                            $"{((selectedDragNDropNode != null) ? selectedDragNDropNode.Id : "null")} before " +
                            $"{((targetedNode != null && node != null) ? node.Id : "null")}");
            
                        if (!targetedNode.IsParentedBy(selectedDragNDropNode))
                        {
                            if (selectedDragNDropNode.Parent != targetedNode.Parent)
                                selectedDragNDropNode.Parent = targetedNode.Parent;
            
                            int tnei = Context.Scene.GetEntityIndex(targetedNode);
                            if (tnei > Context.Scene.GetEntityIndex(selectedDragNDropNode)) tnei--;
                            Context.Scene.ShiftEntity(selectedDragNDropNode, tnei);
            
                            if (targetedNode.Parent != null)
                            {
                                int tnci = targetedNode.Parent.GetChildIndex(targetedNode);
                                if (tnci > targetedNode.Parent.GetChildIndex(selectedDragNDropNode)) tnci--;
                                targetedNode.Parent.ShiftChild(selectedDragNDropNode, tnci);
                            }
                            else
                            {
                                int tnci = Context.Scene.GetRootEntityIndex(targetedNode);
                                if (tnci > Context.Scene.GetRootEntityIndex(selectedDragNDropNode)) tnci--;
                                Context.Scene.ShiftRootEntity(selectedDragNDropNode, tnci);
                            }
                        }
                        else EditorApplication.Log.Trace($"drop failed (cyclic tree prevented)");
            
                        selectedDragNDropNode = null;
                        targetedNode = null;
                    }
                    ImGui.EndDragDropTarget();
                }
            
                if (resetTarget) targetedNode = null;
            }

            ImGuiTreeNodeFlags flags = ((node == Context.ActiveEntity) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) |
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                       ImGuiTreeNodeFlags.DefaultOpen |
                                       ((node.Children.Count <= 0) ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);
            
            if (node == Context.ActiveEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));

            string label = node.TryGetComponent(out TagComponent tagcomp) ? tagcomp.Tag : $"Entity (UID: {node.Id})";
            
            bool opened = ImGui.TreeNodeEx(id + i.ToString(), flags, label);
            
            if (node == Context.ActiveEntity) ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
                Context.ActiveEntity = node;

            // drag drop
            unsafe
            {
                if (ImGui.BeginDragDropSource())
                {
                    ImGui.Text("Entity: " + label);
                    selectedDragNDropNode = node;
                    ImGui.SetDragDropPayload(DragDropIdentifier, IntPtr.Zero, 0);
                    ImGui.EndDragDropSource();
                }
            
                // dropped on to
                if (ImGui.BeginDragDropTarget())
                {
                    targetedNode = node;
            
                    var payload = ImGui.AcceptDragDropPayload(DragDropIdentifier);
                    if (payload.NativePtr != null)
                    {
                        EditorApplication.Log.Trace("dropped " +
                            $"{((selectedDragNDropNode != null) ? selectedDragNDropNode.Id : "null")} onto " +
                            $"{((node != null) ? node.Id : "null")}");
            
                        if (!node.IsParentedBy(selectedDragNDropNode)) selectedDragNDropNode.Parent = node;
                        else EditorApplication.Log.Trace($"drop failed (cyclic tree prevented)");
            
                        selectedDragNDropNode = null;
                        targetedNode = null;
                    }
                    ImGui.EndDragDropTarget();
                }
            }

            // context menu
            {
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Add child"))
                    {
                        var newboi = Context.Scene.CreateEntity();
                        newboi.Parent = node;
                    }
                    if (ImGui.MenuItem("Add empty child"))
                    {
                        var newboi = Context.Scene.CreateEmptyEntity();
                        newboi.Parent = node;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Remove entity", node != null))
                    {
                        Context.Scene.DestroyEntity(node);
                        Context.ActiveEntity = null;
                    }

                    ImGui.EndPopup();
                }
            }

            if (opened)
            {
                for (int cni = 0; cni < node.Children.Count; cni++)
                {
                    DrawEntityNode(node.Children[cni], id + "|" + i++.ToString());
                }
                ImGui.TreePop();
            }

            ImGui.PopID();
        }
    }
}
