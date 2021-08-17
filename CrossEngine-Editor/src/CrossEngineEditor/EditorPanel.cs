using ImGuiNET;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Events;

namespace CrossEngineEditor
{
    public abstract class EditorPanel
    {
        public string Name = "";
        public bool? Open = true;
        public ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.None;
        public Vector2 WindowSize;
        public Vector2 WindowPos;
        public Vector2 ContentMin;
        public Vector2 ContentMax;
        public bool Focused;

        public EditorPanel(string name)
        {
            this.Name = name;
        }

        public EditorPanel()
        {
            this.Name = "Panel";
        }

        public void Draw()
        {
            if (Open != null && !(bool)Open) return;

            ImGui.PushID((int)ImGui.GetID(Name));

            PrepareWindow();

            if (!ImGuiExtension.BeginNullableOpen(Name, ref Open, WindowFlags))
            {
                EndPrepareWindow();

                ImGui.End();
            }
            else
            {
                EndPrepareWindow();

                ContentMin = ImGui.GetWindowContentRegionMin();
                ContentMax = ImGui.GetWindowContentRegionMax();

                ContentMin.X += ImGui.GetWindowPos().X;
                ContentMin.Y += ImGui.GetWindowPos().Y;
                ContentMax.X += ImGui.GetWindowPos().X;
                ContentMax.Y += ImGui.GetWindowPos().Y;

                WindowSize = ImGui.GetWindowSize();
                WindowPos = ImGui.GetWindowPos();

                Focused = ImGui.IsWindowFocused();

                DrawWindowContent();
                
                ImGui.End();
            }

            ImGui.PopID();
        }

        protected virtual void PrepareWindow() { }
        protected virtual void EndPrepareWindow() { }
        protected abstract void DrawWindowContent();
        public virtual void OnEvent(Event e) { }
        public virtual void OnAttach() { }
        public virtual void OnDetach() { }

        public bool IsOpen() => Open == null || (bool)Open;
    }
}
