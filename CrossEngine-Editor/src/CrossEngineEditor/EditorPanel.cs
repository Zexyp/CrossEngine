using ImGuiNET;
using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Events;

namespace CrossEngineEditor
{
    public abstract class EditorPanel
    {
        private bool? _open = true;

        public EditorContext Context;
        public string WindowName = "";
        public bool? Open
        {
            get => _open;
            set
            {
                if (value == _open || !Attached) return;
                _open = value;
                if (_open == false) OnClose();
                else OnOpen();
            }
        }

        public bool Attached { get; internal set; }
        public ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.None;
        public Vector2 WindowSize;
        public Vector2 WindowPos;
        public Vector2 WindowContentAreaMin;
        public Vector2 WindowContentAreaMax;
        public bool Focused;

        public event Action<EditorPanel> InnerBeforeDrawCallback;
        public event Action<EditorPanel> InnerAfterDrawCallback;

        public EditorPanel(string name)
        {
            this.WindowName = name;
        }

        public EditorPanel()
        {
            this.WindowName = "Panel";
        }

        public void Draw()
        {
            if (_open != null && !(bool)_open) return;

            ImGui.PushID((int)ImGui.GetID(WindowName));

            PrepareWindow();

            if (!ImGuiExtension.BeginNullableOpen(WindowName, ref _open, WindowFlags))
            {
                EndPrepareWindow();

                ImGui.End();
            }
            else
            {
                EndPrepareWindow();

                WindowSize = ImGui.GetWindowSize();
                WindowPos = ImGui.GetWindowPos();

                // content area calculation
                WindowContentAreaMin = ImGui.GetWindowContentRegionMin();
                WindowContentAreaMax = ImGui.GetWindowContentRegionMax();

                WindowContentAreaMin += WindowPos;
                WindowContentAreaMax += WindowPos;

                if (ImGui.IsWindowHovered()) ImGui.SetWindowFocus();
                Focused = ImGui.IsWindowFocused();

                InnerBeforeDrawCallback?.Invoke(this);
                DrawWindowContent();
                InnerAfterDrawCallback?.Invoke(this);

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
        public virtual void OnOpen() { }
        public virtual void OnClose() { }

        public bool IsOpen() => _open == null || (bool)_open;
    }
}
