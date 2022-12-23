using ImGuiNET;

using System.Linq;

namespace CrossEngineEditor.Panels
{
    class OperationHistoryPanel : EditorPanel
    {
        public OperationHistoryPanel() : base("Operation History")
        {

        }

        protected override unsafe void DrawWindowContent()
        {
            if (Context.Operations == null)
                return;

            var stack = Context.Operations;

            if (ImGui.Button("Undo"))
                stack.Undo();
            ImGui.SameLine();
            if (ImGui.Button("Redo"))
                stack.Redo();

            ImGui.PushID("##end of history");
            if (ImGui.Selectable("-- end of history --"))
            {
                var f = stack.History.DefaultIfEmpty(null).First();
                if (f != null) stack.JumpBefore(f);
            }
            ImGui.PopID();

            bool after = stack.RecentLast == null;
            if (after)
                ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));
            foreach (var edit in stack.History)
            {
                ImGui.PushID(edit.GetHashCode());
                
                if (ImGui.Selectable(edit.ToString(), edit == stack.RecentLast))
                    stack.JumpAfter(edit);

                ImGui.PopID();

                if (edit == stack.RecentLast && !after)
                {
                    after = true;
                    ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));
                }
            }
            if (after)
                ImGui.PopStyleColor();
        }
    }
}
