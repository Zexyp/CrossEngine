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

        static TreeNode<Entity> selectedDragNDropNode = null;

        void DrawEntityNode(TreeNode<Entity> node, string id)
        {
            int i = 0;

            Entity entity = node.Value;

            ImGuiTreeNodeFlags flags = ((entity == Context.ActiveEntity) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) |
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                       ImGuiTreeNodeFlags.DefaultOpen |
                                       ((node.Children.Count <= 0) ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);

            if (entity == Context.ActiveEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));
            
            string label = (Context.Scene.HierarchyRoot == node) ? "Scene" : (entity != null) ? (
                entity.TryGetComponent(out TagComponent tagcomp) ?
                    tagcomp.Tag :
                    "uid: " + entity.UID.ToString())
                : "";
            
            bool opened = ImGui.TreeNodeEx(id + i++.ToString(), flags, label);
            
            if (entity == Context.ActiveEntity) ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
                Context.ActiveEntity = entity;

            unsafe
            {
                if (ImGui.BeginDragDropSource())
                {
                    ImGui.Text("Entity: " + label);
                    selectedDragNDropNode = node;
                    ImGui.SetDragDropPayload("_ENTITY_UID", IntPtr.Zero, 0);
                    ImGui.EndDragDropSource();
                }

                if (ImGui.BeginDragDropTarget())
                {
                    var payload = ImGui.AcceptDragDropPayload("_ENTITY_UID");
                    if (payload.NativePtr != null)
                    {
                        EditorApplication.Log.Trace("dropped " +
                            $"{((selectedDragNDropNode != null && selectedDragNDropNode.Value != null) ? selectedDragNDropNode.Value.UID : "null")} onto " +
                            $"{((node != null && node.Value != null) ? node.Value.UID : "null")}");

                        if (!node.IsParentedBy(node)) selectedDragNDropNode.Value.Parent = node.Value;
                        else EditorApplication.Log.Trace($"drop failed (cyclic tree prevented)");

                        selectedDragNDropNode = null;
                    }
                    ImGui.EndDragDropTarget();
                }
            }

            {
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Add child"))
                    {
                        var newboi = Context.Scene.CreateEntity();
                        newboi.Parent = node.Value;
                    }
                    if (ImGui.MenuItem("Add empty child"))
                    {
                        var newboi = Context.Scene.CreateEmptyEntity();
                        newboi.Parent = node.Value;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Remove entity", node.Value != null))
                    {
                        Context.Scene.RemoveEntity(node.Value);
                        Context.ActiveEntity = null;
                    }

                    ImGui.EndPopup();
                }
            }

            if (opened)
            {
                for (int cni = 0; cni < node.Children.Count; cni++)
                {
                    DrawEntityNode(node.Children[cni], id + i++.ToString());
                }
                ImGui.TreePop();
            }
        }
    }
}
