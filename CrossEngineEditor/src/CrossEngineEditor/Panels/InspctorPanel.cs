using CrossEngine.Utils.Editor;
using CrossEngineEditor.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class InspctorPanel : EditorPanel
    {
        public InspctorPanel() : base("Inspector")
        {
            
        }

        protected override void DrawWindowContent()
        {
            if (Context.ActiveEntity == null)
                return;

            for (int i = 0; i < Context.ActiveEntity.Components.Count; i++)
            {
                var component = Context.ActiveEntity.Components[i];
                var componentType = component.GetType();
                ImGui.PushID(component.GetHashCode());
                if (ImGui.CollapsingHeader(componentType.Name))
                {
                    var membs = componentType.GetMembers().Where(m => m.GetCustomAttribute<EditorValueAttribute>() != null);
                    foreach (var item in membs)
                    {
                        PropertyDrawer.DrawEditorValue(item, component);
                    }
                }
                ImGui.PopID();
            }
        }
    }
}
