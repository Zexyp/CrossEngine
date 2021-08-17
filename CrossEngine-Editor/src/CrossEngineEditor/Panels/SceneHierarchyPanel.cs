using ImGuiNET;
using System.Numerics;

using CrossEngine.Logging;
using CrossEngine.Entities;
using CrossEngine.Utils;

namespace CrossEngineEditor
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
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Add Entity"))
                    {
                        if (EditorLayer.Instance.Scene != null)
                            EditorLayer.Instance.Scene.CreateEntity();
                    }
                    if (ImGui.MenuItem("Remove Entity"))
                    {
                        if (EditorLayer.Instance.SelectedEntity != null && EditorLayer.Instance.Scene != null)
                        {
                            EditorLayer.Instance.Scene.RemoveEntity(EditorLayer.Instance.SelectedEntity);
                            EditorLayer.Instance.SelectedEntity = null;
                        }
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (EditorLayer.Instance.Scene != null)
                DrawEntityNode(EditorLayer.Instance.Scene.HierarchyRoot, "");
        }

        void DrawEntityNode(TreeNode<Entity> node, string id)
        {
            int i = 0;

            ImGuiTreeNodeFlags flags = ((node.Value == EditorLayer.Instance.SelectedEntity) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow;

            if (node.Value == EditorLayer.Instance.SelectedEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));
            bool opened = ImGui.TreeNodeEx(id + i++.ToString(), flags, (node.Value != null) ? node.Value.debugName : "");
            if (node.Value == EditorLayer.Instance.SelectedEntity) ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
                EditorLayer.Instance.SelectedEntity = node.Value;

            if (opened)
            {
                foreach (TreeNode<Entity> childNode in node.Children)
                {
                    DrawEntityNode(childNode, id + i++.ToString());
                }
                ImGui.TreePop();
            }
        }
    }
}
