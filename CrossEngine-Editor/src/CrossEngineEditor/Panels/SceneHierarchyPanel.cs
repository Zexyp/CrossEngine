using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine.Logging;
using CrossEngine.Entities;
using CrossEngine.Utils;
using CrossEngine.Entities.Components;

namespace CrossEngineEditor.Panels
{
    class SceneHierarchyPanel : EditorPanel
    {
        public SceneHierarchyPanel() : base("Outliner")
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
                        Context.Scene.RemoveEntity(Context.ActiveEntity);
                        Context.ActiveEntity = null;
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (Context.Scene != null)
                DrawEntityNode(Context.Scene.HierarchyRoot, "");
        }

        TreeNode<Entity> selectedDragNDropNode = null;
        TreeNode<Entity> targetedNode = null;

        void DrawEntityNode(TreeNode<Entity> node, string id, int i = 0)
        {
            Entity nodeEntity = node.Value;


            // drag drop
            unsafe
            {
                // smoll fix
                bool resetTarget = false;
                if (targetedNode == node && targetedNode.Value != null)
                {
                    ImGui.Button("##target", new Vector2(ImGui.GetColumnWidth(), 2.5f));

                    if (ImGui.IsItemHovered()) resetTarget = true;
                }

                // dropped next to
                if (ImGui.BeginDragDropTarget())
                {
                    var payload = ImGui.AcceptDragDropPayload("_TREENODE<ENTITY>");
                    if (payload.NativePtr != null && selectedDragNDropNode != null)
                    {
                        EditorApplication.Log.Trace("dropped " +
                            $"{((selectedDragNDropNode != null && selectedDragNDropNode.Value != null) ? selectedDragNDropNode.Value.UID : "null")} before " +
                            $"{((targetedNode != null && nodeEntity != null) ? nodeEntity.UID : "null")}");

                        if (!targetedNode.IsParentedBy(selectedDragNDropNode))
                        {
                            if (selectedDragNDropNode.Parent != targetedNode.Parent)
                                selectedDragNDropNode.Value.Parent = targetedNode.Value.Parent;

                            int tnei = Context.Scene.GetEntityIndex(targetedNode.Value);
                            if (tnei > Context.Scene.GetEntityIndex(selectedDragNDropNode.Value)) tnei--;
                            Context.Scene.ShiftEntity(selectedDragNDropNode.Value, tnei);

                            int tnci = targetedNode.Parent.GetChildIndex(targetedNode);
                            if (tnci > targetedNode.Parent.GetChildIndex(selectedDragNDropNode)) tnci--;
                            targetedNode.Parent.ShiftChild(selectedDragNDropNode, tnci);
                        }
                        else EditorApplication.Log.Trace($"drop failed (cyclic tree prevented)");

                        selectedDragNDropNode = null;
                        targetedNode = null;
                    }
                    ImGui.EndDragDropTarget();
                }

                if (resetTarget) targetedNode = null;
            }

            ImGuiTreeNodeFlags flags = ((nodeEntity == Context.ActiveEntity) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) |
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                       ImGuiTreeNodeFlags.DefaultOpen |
                                       ((node.Children.Count <= 0) ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);
            
            if (nodeEntity == Context.ActiveEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));
            
            string label = (Context.Scene.HierarchyRoot == node) ? "Scene" : (nodeEntity != null) ? (
                nodeEntity.TryGetComponent(out TagComponent tagcomp) ?
                    tagcomp.Tag :
                    "uid: " + nodeEntity.UID.ToString())
                : "";
            
            bool opened = ImGui.TreeNodeEx(id + i.ToString(), flags, label);
            
            if (nodeEntity == Context.ActiveEntity) ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
                Context.ActiveEntity = nodeEntity;

            // drag drop
            unsafe
            {
                if (ImGui.BeginDragDropSource())
                {
                    ImGui.Text("Entity: " + label);
                    selectedDragNDropNode = node;
                    ImGui.SetDragDropPayload("_TREENODE<ENTITY>", IntPtr.Zero, 0);
                    ImGui.EndDragDropSource();
                }

                // dropped on to
                if (ImGui.BeginDragDropTarget())
                {
                    targetedNode = node;

                    var payload = ImGui.AcceptDragDropPayload("_TREENODE<ENTITY>");
                    if (payload.NativePtr != null)
                    {
                        EditorApplication.Log.Trace("dropped " +
                            $"{((selectedDragNDropNode != null && selectedDragNDropNode.Value != null) ? selectedDragNDropNode.Value.UID : "null")} onto " +
                            $"{((node != null && nodeEntity != null) ? nodeEntity.UID : "null")}");

                        if (!node.IsParentedBy(selectedDragNDropNode)) selectedDragNDropNode.Value.Parent = nodeEntity;
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
                        newboi.Parent = nodeEntity;
                    }
                    if (ImGui.MenuItem("Add empty child"))
                    {
                        var newboi = Context.Scene.CreateEmptyEntity();
                        newboi.Parent = nodeEntity;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Remove entity", nodeEntity != null))
                    {
                        Context.Scene.RemoveEntity(nodeEntity);
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
        }
    }
}
