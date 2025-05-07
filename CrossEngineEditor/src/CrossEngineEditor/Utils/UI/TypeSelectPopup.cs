using CrossEngine.Assemblies;
using CrossEngine.Assets;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Utils.UI
{
    internal class TypeSelectPopup : Popup
    {
        private Type _baseType;
        public Action<TypeSelectPopup, Type> Callback;

        public TypeSelectPopup(Type baseType)
        {
            _baseType = baseType;
        }

        protected override void DrawPopupContent()
        {
            // slow but who cares
            foreach (var assembly in AssemblyManager.Loaded)
            {
                var types = assembly.GetTypes().Where(t => t.IsPublic && !t.IsAbstract && t.IsSubclassOf(_baseType));

                ImGui.SeparatorText(assembly.GetName().Name);

                foreach (var t in types)
                {
                    if (ImGui.Selectable(t.FullName))
                    {
                        Callback.Invoke(this, t);
                    }
                }
            }
        }
    }
}
