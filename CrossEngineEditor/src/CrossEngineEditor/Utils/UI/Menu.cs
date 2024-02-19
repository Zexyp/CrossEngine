using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Utils.UI
{
    internal class Menu
    {
        public string Name;
        public Action Action;

        private List<(string Key, Menu Menu)> _submenus = new();

        public Menu(string name)
        {
            Name = name;
        }

        public Menu this[string index]
        {
            get
            {
                var select = _submenus.Where(item => item.Key == index);
                if (select.Count() == 0)
                {
                    var menu = new Menu(index);
                    _submenus.Add((index, menu));
                    return menu;
                }

                return select.First().Menu;
            }
        }

        public void AddSeparator()
        {
            _submenus.Add((null, null));
        }

        public void Draw()
        {
            if (ImGui.BeginMenu(Name))
            {
                for (int i = 0; i < _submenus.Count; i++)
                {
                    (var name, var sub) = _submenus[i];
                    if (name == null)
                    {
                        ImGui.Separator();
                        continue;
                    }
                    if (sub.Action != null)
                    {
                        if (ImGui.MenuItem(sub.Name))
                            sub.Action.Invoke();
                        continue;
                    }
                    sub.Draw();
                }

                ImGui.EndMenu();
            }
        }
    }
}
