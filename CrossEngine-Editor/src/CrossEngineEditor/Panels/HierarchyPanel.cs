using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine.Logging;
using CrossEngine.ECS;
using CrossEngine.Utils;
using CrossEngine.Components;

using CrossEngineEditor.Operations;

namespace CrossEngineEditor.Panels
{
    public class HierarchyPanel : EditorPanel
    {
        private const string DragDropIdentifier = nameof(TreeNode<Entity>);

        public HierarchyPanel() : base("Hierarchy")
        {
            WindowFlags = ImGuiWindowFlags.MenuBar;
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Edit", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Add Entity", Context.Scene != null))
                    {
                        var ent = Context.Scene.CreateEntity();
                        var op = new EntityAddOperation(ent, Context.Scene);
                        Context.Operations.Push(op);
                    }
                    if (ImGui.MenuItem("Remove Entity", Context.ActiveEntity != null))
                    {
                        var entToRemove = Context.ActiveEntity;
                        var op = new EntityRemoveOperation(entToRemove, Context.Scene, entToRemove.Parent, System.Linq.Enumerable.ToArray(entToRemove.Children),
                            Context.Scene.GetEntityIndex(entToRemove),
                            entToRemove.Parent == null ? Context.Scene.GetRootEntityIndex(entToRemove) : entToRemove.Parent.GetChildIndex(entToRemove));
                        Context.Operations.Push(op);
                        // we need to only remove because of the undo, redo
                        Context.Scene.RemoveEntity(entToRemove);
                        Context.ActiveEntity = null;
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (Context.Scene != null)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4));
                var ents = Context.Scene.HierarchyRoot;
                for (int i = 0; i < ents.Count; i++)
                {
                    DrawEntityNode(ents[i], i.ToString());
                }
                ImGui.PopStyleVar();
            }
        }

        Entity selectedDragNDropNode = null;
        Entity targetedNode = null;

        void DrawEntityNode(Entity node, string id, int i = 0)
        {
            ImGui.PushID(node.Id.GetHashCode());
            // drag drop
            unsafe
            {
                // smoll fix
                bool resetTarget = false;
                // draw drop target
                if (targetedNode == node && targetedNode != null)
                {
                    ImGui.Button("##target", new Vector2(ImGui.GetColumnWidth(), 2.5f));
            
                    if (!ImGui.IsMouseDown(ImGuiMouseButton.Left)) resetTarget = true;
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
                            {
                                var op = new EntityParentChangeOperation(selectedDragNDropNode, selectedDragNDropNode.Parent, targetedNode.Parent);
                                Context.Operations.Push(op);
                                selectedDragNDropNode.Parent = targetedNode.Parent;
                            }

                            int prevSelectedDND = Context.Scene.GetEntityIndex(selectedDragNDropNode);
                            int tnei = Context.Scene.GetEntityIndex(targetedNode);
                            if (tnei > prevSelectedDND) tnei--;
                            if (prevSelectedDND != tnei)
                            {
                                Context.Scene.ShiftEntity(selectedDragNDropNode, tnei);
                                var op = new SceneShiftEntityOperation(Context.Scene, selectedDragNDropNode, tnei, prevSelectedDND);
                                Context.Operations.Push(op);
                            }
            
                            if (targetedNode.Parent != null)
                            {
                                int prevSelectedDNDi = targetedNode.Parent.GetChildIndex(selectedDragNDropNode);
                                int tnci = targetedNode.Parent.GetChildIndex(targetedNode);
                                if (tnci > prevSelectedDNDi) tnci--;
                                if (prevSelectedDNDi != tnei)
                                {
                                    targetedNode.Parent.ShiftChild(selectedDragNDropNode, tnci);
                                    var op = new EntityShiftChildOperation(targetedNode.Parent, selectedDragNDropNode, tnei, prevSelectedDNDi);
                                    Context.Operations.Push(op);
                                }
                            }
                            else
                            {
                                int prevSelectedDNDi = Context.Scene.GetRootEntityIndex(selectedDragNDropNode);
                                int tnci = Context.Scene.GetRootEntityIndex(targetedNode);
                                if (tnci > Context.Scene.GetRootEntityIndex(selectedDragNDropNode)) tnci--;
                                if (prevSelectedDNDi != tnei)
                                {
                                    Context.Scene.ShiftRootEntity(selectedDragNDropNode, tnci);
                                    var op = new SceneShiftRootEntityOperation(Context.Scene, selectedDragNDropNode, tnei, prevSelectedDNDi);
                                    Context.Operations.Push(op);
                                }
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
                                       ((node.Children.Count <= 0) ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None) |
                                       ImGuiTreeNodeFlags.OpenOnArrow |
                                       ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                       ImGuiTreeNodeFlags.DefaultOpen |
                                       ImGuiTreeNodeFlags.FramePadding;
            
            if (node == Context.ActiveEntity) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.259f, 0.588f, 0.98f, 1.0f));

            string label = node.TryGetComponent(out TagComponent tagcomp) ? tagcomp.Tag : $"Entity (UID: {node.Id})";
            
            bool opened = ImGui.TreeNodeEx(id + i.ToString(), flags, label);
            
            if (node == Context.ActiveEntity) ImGui.PopStyleColor();

            if (ImGui.IsItemClicked())
            {

                if (Context.ActiveEntity != node)
                {
                    var op = new EntitySelectOpertion(Context, Context.ActiveEntity, node);
                    Context.Operations.Push(op);
                    Context.ActiveEntity = node;
                }
            }

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

                        if (!node.IsParentedBy(selectedDragNDropNode))
                        {
                            var op = new EntityParentChangeOperation(selectedDragNDropNode, selectedDragNDropNode.Parent, node);
                            Context.Operations.Push(op);
                            selectedDragNDropNode.Parent = node;
                        }
                        else
                            EditorApplication.Log.Trace($"drop failed (cyclic tree prevented)");
            
                        selectedDragNDropNode = null;
                        targetedNode = null;
                    }
                    ImGui.EndDragDropTarget();
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
