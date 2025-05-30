﻿using CrossEngine.Ecs;
using CrossEngine.Inputs;
using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.UI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static CrossEngine.Display.WindowService;

namespace CrossEngineEditor.Panels
{
    internal class HierarchyPanel : EditorPanel
    {
        public HierarchyPanel() : base("Hierarchy")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Entity", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Add"))
                        Context.Scene.CreateEntity();
                    if (ImGui.MenuItem("Add Empty"))
                        Context.Scene.CreateEmptyEntity();
                    ImGui.Separator();
                    if (ImGui.MenuItem("Delete", Context.ActiveEntity != null))
                    {
                        Context.Scene.RemoveEntity(Context.ActiveEntity);
                        Context.ActiveEntity = null;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (Context.Scene == null)
            {
                ImGui.TextDisabled("No scene selected");
                return;
            }
            
            foreach (var ent in Context.Scene.Entities.Where(e => e.Parent == null).ToArray())
            {
                DrawEntityNode(ent);
            }

            ddEntityContext.Update();
        }

        DragDropContext<Entity> ddEntityContext = new DragDropContext<Entity>();

        unsafe void DrawEntityNode(Entity node)
        {
            ImGui.PushID(node.GetHashCode());

            ImGuiTreeNodeFlags flags = ((node == Context.ActiveEntity) ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) |
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                       ImGuiTreeNodeFlags.DefaultOpen |
                                       ((node.Children.Count <= 0) ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None);

            //if (node == Context.ActiveEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));

            string label = node.ToString();
            //string label = node.TryGetComponent<TagComponent>(out var tagComponent) ? tagComponent.Tag : $"Entity (Id: {node.Id})";
            label ??= "";

            bool opened = ImGui.TreeNodeEx(node.GetHashCode().ToString(), flags, label);

            if (ImGui.IsItemClicked())
                Context.ActiveEntity = node;

            //if (node == Context.ActiveEntity) ImGui.PopStyleColor();

            ddEntityContext.MarkSource(node);
            if (ddEntityContext.MarkTarget())
                if (!node.IsParentedBy(ddEntityContext.Source))
                {
                    ddEntityContext.Source.Parent = node;
                    ddEntityContext.End();
                }

            if (opened)
            {
                for (int cni = 0; cni < node.Children.Count; cni++)
                {
                    DrawEntityNode(node.Children[cni]);
                }

                ImGui.TreePop();
            }

            if (ddEntityContext.Source != node && ddEntityContext.Active)
            {
                const float SEPARATOR_HEIGHT = 4;

                Vector2 prevCur = ImGui.GetCursorPos();

                ImGui.SetCursorPos(new Vector2(prevCur.X, prevCur.Y - SEPARATOR_HEIGHT / 2));
                ImGui.Button("##target", new Vector2(ImGui.GetColumnWidth(), SEPARATOR_HEIGHT));

                if (ddEntityContext.MarkTarget())
                {
                    if (node.Parent != ddEntityContext.Source && !node.IsParentedBy(ddEntityContext.Source))
                    {
                        ddEntityContext.Source.Parent = node.Parent;
                        if (node.Parent == null)
                        {
                            Context.Scene.MoveEntity(ddEntityContext.Source, Context.Scene.Entities.IndexOf(node));
                        }
                        else
                        {
                            node.Parent.MoveChild(ddEntityContext.Source, node.Parent.Children.IndexOf(node));
                        }
                    }
                    ddEntityContext.End();
                }

                ImGui.SetCursorPos(prevCur);
            }

            ImGui.PopID();
        }
    }
}
