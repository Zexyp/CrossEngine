using CrossEngine.Ecs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static CrossEngine.Services.WindowService;

namespace CrossEngineEditor.Panels
{
    internal class HierarchyPanel : EditorPanel
    {
        public HierarchyPanel() : base("Hierarchy")
        {
            
        }

        protected override void DrawWindowContent()
        {
            if (Context.Scene == null)
                return;
            
            foreach (var ent in Context.Scene.Entities.Where(e => e.Parent == null))
            {
                DrawEntityNode(ent);
            }
        }

        unsafe void DrawEntityNode(Entity node)
        {
            ImGui.PushID(node.GetHashCode());

            ImGuiTreeNodeFlags flags = ((node == Context.ActiveEntity) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) |
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                       ImGuiTreeNodeFlags.DefaultOpen |
                                       ((node.Children.Count <= 0) ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);

            if (node == Context.ActiveEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));

            string label = $"Entity (Id: {node.Id})";

            bool opened = ImGui.TreeNodeEx(node.GetHashCode().ToString(), flags, label);

            if (node == Context.ActiveEntity) ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
                Context.ActiveEntity = node;

            if (opened)
            {
                for (int cni = 0; cni < node.Children.Count; cni++)
                {
                    DrawEntityNode(node.Children[cni]);
                }

                ImGui.TreePop();
            }

            ImGui.PopID();
        }
    }
}
