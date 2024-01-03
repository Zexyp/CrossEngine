﻿using ImGuiNET;
using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Events;
using CrossEngine.Utils.ImGui;

namespace CrossEngineEditor.Panels
{
    public abstract class EditorPanel
    {
        private bool? _open = true;

        public string WindowName = "";
        public bool? Open
        {
            get => _open;
            set
            {
                if (value == _open) return;
                _open = value;
                UpdateOpenState();
            }
        }

        public bool Attached { get; internal set; }

        public event Action<EditorPanel> InnerBeforeDrawCallback;
        public event Action<EditorPanel> InnerAfterDrawCallback;

        protected ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.None;
        protected Vector2 WindowSize;
        protected Vector2 WindowPosition;
        protected bool Focused;

        internal EditorContext Context;

        public EditorPanel(string name)
        {
            this.WindowName = name;
        }

        public EditorPanel()
        {
            this.WindowName = $"Unnamed Panel {this.GetHashCode()}";
        }

        public void Draw()
        {
            if (_open != null && !(bool)_open) return;

            ImGui.PushID((int)ImGui.GetID(WindowName));

            PrepareWindow();

            var lastOpen = _open;
            if (!ImGuiNull.Begin(WindowName, ref _open, WindowFlags))
            {
                EndPrepareWindow();

                ImGui.End();
            }
            else
            {
                EndPrepareWindow();

                WindowSize = ImGui.GetWindowSize();
                WindowPosition = ImGui.GetWindowPos();

                if (ImGui.IsWindowHovered() &&
                    !ImGui.IsMouseDragging(ImGuiMouseButton.Left) &&
                    !ImGui.IsMouseDragging(ImGuiMouseButton.Right) &&
                    !ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
                    ImGui.SetWindowFocus();
                Focused = ImGui.IsWindowFocused();

                InnerBeforeDrawCallback?.Invoke(this);
                DrawWindowContent();
                InnerAfterDrawCallback?.Invoke(this);

                ImGui.End();
            }
            if (lastOpen != _open)
                UpdateOpenState();

            ImGui.PopID();
        }

        public bool IsOpen => _open == null || (bool)_open;

        protected virtual void PrepareWindow() { }
        protected virtual void EndPrepareWindow() { }
        protected abstract void DrawWindowContent();
        public virtual void OnAttach() { }
        public virtual void OnDetach() { }
        public virtual void OnOpen() { }
        public virtual void OnClose() { }

        private void UpdateOpenState()
        {
            if (_open == false) OnClose();
            else if (_open == true) OnOpen();
        }
    }
}