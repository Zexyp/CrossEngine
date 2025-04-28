using CrossEngine.Assemblies;
using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Utils.Editor;
using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.UI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class InspectorPanel : EditorPanel
    {
        public InspectorPanel() : base("Inspector")
        {
            typeSelect = new TypeSelectPopup(typeof(Component)) { Callback = (sender, type) => { Context.ActiveEntity.AddComponent((Component)Activator.CreateInstance(type)); } };
        }

        static DragDropContext<Component> ddComponentContext = new DragDropContext<Component>();

        TypeSelectPopup typeSelect;

        protected override void DrawWindowContent()
        {
            var entity = Context.ActiveEntity;

            if (entity == null)
            {
                ImGui.TextDisabled("No entity selected");
                return;
            }

            if (entity.Name != null) ImGui.InputText("Name", ref entity.Name, 36);
            else if (ImGui.Button("Name")) entity.Name = "";

                var style = ImGui.GetStyle();

            for (int i = 0; i < entity.Components.Count; i++)
            {
                Component component = entity.Components[i];
                Type componentType = component.GetType();

                ImGui.PushID(component.GetHashCode());

                bool stay = true;
                bool collapsingHeader = ImGui.CollapsingHeader(componentType.Name, ref stay);

                ddComponentContext.MarkSource(component);
                if (ddComponentContext.MarkTarget())
                {
                    entity.MoveComponent(ddComponentContext.Source, entity.Components.IndexOf(component));
                    ddComponentContext.End();
                }

                float collapsingHeaderButtonOffset = ImGui.GetTextLineHeight() + style.FramePadding.Y * 2 + style.ItemSpacing.X;
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 1 + style.FramePadding.X + style.FramePadding.Y));
                bool enabled = component.Enabled;
                if (ImGui.Checkbox("##enabled", ref enabled))
                    component.Enabled = enabled;
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 2 + style.FramePadding.X + style.FramePadding.Y));
                if (ImGui.ArrowButton("up", ImGuiDir.Up))
                    entity.MoveComponent(component, Math.Max(0, i - 1));
                ImGui.SameLine(ImGui.GetContentRegionAvail().X - (collapsingHeaderButtonOffset * 3 + style.FramePadding.X + style.FramePadding.Y));
                if (ImGui.ArrowButton("down", ImGuiDir.Down))
                    entity.MoveComponent(component, Math.Min(entity.Components.Count - 1, i + 1));

                if (collapsingHeader)
                {
                    InspectDrawer.Inspect(component);

                    ImGui.Separator();
                }

                ImGui.PopID();

                if (!stay)
                {
                    entity.RemoveComponent(component);
                }
            }

            float size = ImGui.CalcTextSize("Add Component").X + style.FramePadding.X * 2.0f;
            float avail = ImGui.GetContentRegionAvail().X;

            float off = (avail - size) * .5f;
            if (off > 0.0f)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);

            const string componentPopup = "Add Component";

            if (ImGui.Button("Add Component"))
            {
                typeSelect.Open();
            }

            typeSelect.Draw();

            ddComponentContext.Update();
        }
    }
}
