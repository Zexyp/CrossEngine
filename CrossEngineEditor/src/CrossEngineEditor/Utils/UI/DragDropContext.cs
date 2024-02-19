using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Utils.UI
{
    internal class DragDropContext<T> where T : class
    {
        private bool srcUpdated = false;

        public T? Source = null;
        public bool Active { get => Source != null; }

        public void MarkSource(T obj)
        {
            if (ImGui.BeginDragDropSource())
            {
                ImGui.Text(typeof(T).FullName);

                Source = obj;
                srcUpdated = true;

                ImGui.SetDragDropPayload(typeof(T).FullName, IntPtr.Zero, 0);

                ImGui.EndDragDropSource();
            }
        }

        public unsafe bool MarkTarget()
        {
            var result = false;

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload(typeof(T).FullName);
                    result = payload.NativePtr != null;

                ImGui.EndDragDropTarget();
            }

            return result;
        }

        public void End()
        {
            Source = null;
        }

        public void Update()
        {
            if (!srcUpdated)
                End();
            srcUpdated = false;
        }
    }
}
