using CrossEngine.Utils.ImGui;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Modals
{
    public abstract class EditorModal
    {
        public string ModalName
        {
            get => _modalName;
            set => _modalName = string.IsNullOrEmpty(value) ? $"Unnamed Modal '{this.GetType().FullName}' ({this.GetHashCode()})" : value;
        }
        public bool? Open = true;
        public ImGuiWindowFlags ModalFlags = ImGuiWindowFlags.AlwaysAutoResize;

        private string _modalName;

        public EditorModal(string name)
        {
            this.ModalName = name;
        }

        public EditorModal()
        {
            this.ModalName = default;
        }

        public void Draw(Action innerDraw)
        {
            ImGui.OpenPopup(ModalName);

            PrepareModal();

            var modalOpen = ImGuiNull.BeginPopupModal(ModalName, ref Open, ModalFlags);

            EndPrepareModal();

            if (modalOpen)
            {
                DrawModalContent();

                innerDraw?.Invoke();

                ImGui.EndPopup();
            }

            if ((Open != null) && !(bool)Open)
            {
                Default();
            }

            //bool resultOpen = ImGui.IsPopupOpen(ModalName);
            //
            //return resultOpen;
        }

        protected virtual void Default() { }
        protected virtual void PrepareModal() { }
        protected virtual void EndPrepareModal() { }

        abstract protected void DrawModalContent();
        
        protected void End()
        {
            ImGui.CloseCurrentPopup();
            Open = false;
        }
    }
}
