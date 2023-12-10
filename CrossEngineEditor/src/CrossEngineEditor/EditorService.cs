using CrossEngine.Display;
using CrossEngine.Profiling;
using CrossEngine.Services;
using CrossEngineEditor.Panels;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor
{
    internal class EditorService : Service
    {
        readonly EditorContext Context = new EditorContext();
        readonly List<EditorPanel> _panels = new List<EditorPanel>();
        private Window window = null;

        public override void OnStart()
        {
            AddPanel(new InspctorPanel());
        }

        public override void OnDestroy()
        {
            while (_panels.Count > 0)
                RemovePanel(_panels[0]);
        }

        public override void OnAttach()
        {
            Manager.GetService<WindowService>().Execute(() => window = Manager.GetService<WindowService>().Window);
            var rs = Manager.GetService<RenderService>();
            rs.Frame += OnRender;
        }

        private void OnRender(RenderService rs)
        {
            Profiler.BeginScope();

            SetupDockspace(window);

            DrawMainMenuBar();

            var io = ImGui.GetIO();

            ImGui.ShowDemoWindow();
            Profiler.BeginScope($"{nameof(EditorService)}.{nameof(EditorService.DrawPanels)}");
            DrawPanels();
            Profiler.EndScope();

            EndDockspace();

            Profiler.EndScope();
        }

        public override void OnDetach()
        {
            var rs = Manager.GetService<RenderService>();
            rs.Frame -= OnRender;
        }

        #region Dockspace
        private unsafe void SetupDockspace(Window window)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            ImGuiWindowFlags flags =
                ImGuiWindowFlags.MenuBar |
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus;

            bool open = true;
            ImGui.Begin("Dockspace", ref open, flags);

            ImGui.PopStyleVar(3);

            //Vector2 lastCurPos = ImGui.GetCursorPos();
            //ImGui.SetCursorPos((ImGui.GetWindowSize() - new Vector2(256, 256)) * 0.5f);
            //ImGui.Image(new IntPtr(dockspaceIconTexture.ID), new Vector2(256, 256), new Vector2(0, 1), new Vector2(1, 0), new Vector4(1, 1, 1, 0.75f));
            //ImGui.SetCursorPos(lastCurPos);
            Vector4 col = *ImGui.GetStyleColorVec4(ImGuiCol.DockingEmptyBg);
            col.W *= 0.1f;
            ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, col);

            ImGui.DockSpace(ImGui.GetID("Dockspace"));

            ImGui.PopStyleColor();
        }

        private void EndDockspace()
        {
            ImGui.End();
        }

        private void DrawMainMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                
                ImGui.EndMainMenuBar();
            }
        }
        #endregion

        #region Panel Methods
        private void AddPanel(EditorPanel panel)
        {
            _panels.Add(panel);
            panel.Context = Context;

            panel.Attached = true;
            panel.OnAttach();

            if (panel.Open != false) panel.OnOpen();
        }

        private void RemovePanel(EditorPanel panel)
        {
            if (panel.Open != false) panel.OnClose();

            panel.OnDetach();
            panel.Attached = false;

            panel.Context = null;
            _panels.Remove(panel);
        }

        public T GetPanel<T>() where T : EditorPanel
        {
            return (T)GetPanel(typeof(T));
        }

        public EditorPanel GetPanel(Type typeOfPanel)
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                if (_panels[i].GetType() == typeOfPanel)
                    return _panels[i];
            }
            return null;
        }

        private void DrawPanels()
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                _panels[i].Draw();
            }
        }
        #endregion
    }
}
