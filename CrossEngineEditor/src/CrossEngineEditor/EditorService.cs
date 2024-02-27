using CrossEngine.Assets.Loaders;
using CrossEngine.Display;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Textures;
using CrossEngine.Scenes;
using CrossEngine.Services;
using CrossEngineEditor.Panels;
using CrossEngine.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Logging;
using System.Diagnostics;
using CrossEngineEditor.Utils;
using StbImageSharp;
using CrossEngine.Assets;
using System.Text.Json;
using CrossEngine.Serialization;
using CrossEngine.Utils.ImGui;
using System.Threading.Channels;

namespace CrossEngineEditor
{
    internal class EditorService : Service
    {
        readonly EditorContext Context = new EditorContext();
        readonly List<EditorPanel> _panels = new List<EditorPanel>();
        readonly List<EditorPanel> _registeredPanels = new List<EditorPanel>();
        //readonly List<EditorModal> _modals = new List<EditorModal>();
        private Window window = null;
        internal static Logger Log = new Logger("editor") { Color = 0xffCE1E6B };

        public override void OnStart()
        {
            Log.Info("editor started");
        }

        public override void OnDestroy()
        {
            while (_panels.Count > 0)
                RemovePanel(_panels[0]);

            _registeredPanels.Clear();

            Log.Info("editor closed");
        }

        public override unsafe void OnAttach()
        {
            var rs = Manager.GetService<RenderService>();
            Manager.GetService<WindowService>().Execute(() =>
            {
                window = Manager.GetService<WindowService>().Window;
                
                // eeww
                Theming.UseImmersiveDarkMode(Process.GetCurrentProcess().MainWindowHandle, true);
                var result = ImageResult.FromMemory(CrossEngine.Properties.Resources.Logo, ColorComponents.RedGreenBlueAlpha);
                fixed (void* p = &result.Data[0])
                    window.SetIcon(p, (uint)result.Width, (uint)result.Height);

            });
            rs.Frame += OnRender;
            rs.Execute(() =>
            {
                dockspaceIconTexture = TextureLoader.LoadTexture(CrossEngine.Properties.Resources.Logo);
                dockspaceIconTexture.GetValue().SetFilterParameter(FilterParameter.Nearest);
            });

            Init();
        }

        public override void OnDetach()
        {
            Deinit();

            var rs = Manager.GetService<RenderService>();
            rs.Frame -= OnRender;
            rs.Execute(() =>
            {
                dockspaceIconTexture.Dispose();
                dockspaceIconTexture = null;
            });
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

        private void Init()
        {
            Context.AssetsChanged += OnContextAssetsChanged;
            Context.SceneChanged += OnContextSceneChanged;

            var rs = Manager.GetService<RenderService>();
            RegisterPanel(new InspectorPanel());
            RegisterPanel(new HierarchyPanel());
            RegisterPanel(new ViewportPanel(rs));
            RegisterPanel(new GamePanel(rs));
            RegisterPanel(new AssetListPanel());
            RegisterPanel(new SimpleThemeGeneratorPanel());

            // debug thingy
            // ######
            var scene = new Scene();
            scene.CreateEntity();
            scene.CreateEntity();
            scene.CreateEntity();
            scene.Entities[1].Parent = scene.Entities[0];
            scene.Entities[0].AddComponent<CrossEngine.Components.OrthographicCameraComponent>();
            scene.Entities[0].AddComponent<CrossEngine.Components.PerspectiveCameraComponent>();
            scene.Entities[0].AddComponent<CrossEngine.Components.SpriteRendererComponent>();
            scene.Entities[0].AddComponent<CrossEngine.Components.TagComponent>();

            Context.Scene = scene;
            // ######
        }

        private void Deinit()
        {
            Context.Clear();

            Context.AssetsChanged -= OnContextAssetsChanged;
            Context.SceneChanged -= OnContextSceneChanged;
        }

        #region Dockspace
        private WeakReference<Texture> dockspaceIconTexture;

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

            Vector2 lastCur = ImGui.GetCursorPos();
            ImGui.SetCursorPos((ImGui.GetWindowSize() - new Vector2(256, 256)) * 0.5f);
            ImGui.Image(new IntPtr(dockspaceIconTexture.GetValue()?.RendererId ?? 0), new Vector2(256, 256), new Vector2(0, 1), new Vector2(1, 0), new Vector4(1, 1, 1, 0.25f));
            ImGui.SetCursorPos(lastCur);

            Vector4 col = *ImGui.GetStyleColorVec4(ImGuiCol.DockingEmptyBg);
            col.W *= 0.1f;
            ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, col);

            ImGui.DockSpace(ImGui.GetID("Dockspace"));

            //ImGui.PopStyleColor();
        }

        private void EndDockspace()
        {
            ImGui.End();
        }

        private void DrawMainMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    //ImGui.Separator();
                    if (ImGui.MenuItem("Quit"))
                        EditorApplication.Instance.Close();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Scene"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        Context.Scene = new Scene();
                    }
                    if (ImGui.BeginMenu("Load", Context.Assets?.HasCollection<SceneAsset>() == true))
                    {
                        foreach (var item in Context.Assets.GetCollection<SceneAsset>())
                        {
                            if (!item.Loaded) ImGui.BeginDisabled();
                            if (ImGui.Selectable(item.GetName(), item.Scene != null && item.Scene == Context.Scene))
                            {
                                Context.Scene = null;
                                Context.Scene = item.Scene;
                            }
                            if (!item.Loaded) ImGui.EndDisabled();
                        }

                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save As...", Context.Scene != null))
                    {
                        var filepath = ShellFileDialogs.FileSaveDialog.ShowDialog(0, null, null, null, null);
                        if (filepath != null)
                            using (Stream stream = File.OpenWrite(filepath))
                            {
                                stream.SetLength(0);
                                SceneSerializer.SerializeJson(stream, Context.Scene);
                            }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.BeginMenu("Panels"))
                    {
                        for (int i = 0; i < _registeredPanels.Count; i++)
                        {
                            var p = _registeredPanels[i];
                            if (ImGui.MenuItem(p.WindowName, null, p.Open ?? false))
                                p.Open = !p.Open;
                        }

                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Fullscreen", null, window.Fullscreen))
                        window.SetFullscreen(!window.Fullscreen);

                    // TODO: Workspace saving

                    ImGui.EndMenu();
                }

                var dis = Context.Scene == null;
                if (dis) ImGui.BeginDisabled();
                ImGui.SetCursorPosX(ImGui.GetColumnWidth() / 2);
                var on = Context.Mode == EditorContext.Playmode.Playing || Context.Mode == EditorContext.Playmode.Paused;
                if (on) ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered]);
                if (ImGui.ArrowButton("##play", on ? ImGuiDir.Down : ImGuiDir.Right))
                {
                    if (on)
                        StopScene();
                    else
                        StartScene();
                }
                if (on) ImGui.PopStyleColor();
                if (dis) ImGui.EndDisabled();

                ImGui.EndMainMenuBar();
            }
        }
        #endregion

        #region Panel Methods
        private void RegisterPanel(EditorPanel panel)
        {
            if (_registeredPanels.Contains(panel)) throw new InvalidOperationException();

            _registeredPanels.Add(panel);

            PushPanel(panel);

            Log.Trace($"registered panel '{panel.GetType().FullName}'");
        }

        private void UnregisterPanel(EditorPanel panel)
        {
            if (!_registeredPanels.Contains(panel)) throw new InvalidOperationException();

            _registeredPanels.Remove(panel);

            RemovePanel(panel);

            Log.Trace($"unregistered panel '{panel.GetType().FullName}'");
        }

        private void PushPanel(EditorPanel panel)
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

        //public T GetPanel<T>() where T : EditorPanel
        //{
        //    return (T)GetPanel(typeof(T));
        //}
        //
        //public EditorPanel GetPanel(Type typeOfPanel)
        //{
        //    for (int i = 0; i < _panels.Count; i++)
        //    {
        //        if (_panels[i].GetType() == typeOfPanel)
        //            return _panels[i];
        //    }
        //    return null;
        //}

        private void DrawPanels()
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                var p = _panels[i];

                try
                {
                    p.Draw();
                }
                catch (Exception e)
                {
                    Log.Error($"incident while drawing a panel '{p.WindowName}' ({p.GetType().FullName}): {e}");
                }
            }
        }
        #endregion

        private void OnContextSceneChanged(Scene old)
        {
            if (old != null) SceneManager.Unload(old);

            if (Context.Scene != null) SceneManager.Load(Context.Scene, new SceneService.SceneConfig() { Update = false, Render = false, Resize = false });
        }

        private void OnContextAssetsChanged(AssetPool old)
        {
            if (old != null) AssetManager.Unload();

            AssetManager.Bind(Context.Assets);

            if (Context.Assets != null) AssetManager.Load();
        }

        Scene prevScene;
        private void StartScene()
        {
            Debug.Assert(prevScene == null);
            prevScene = Context.Scene;
            Context.Scene = (Scene)Context.Scene.Clone();
            
            SceneManager.Start(Context.Scene);
            SceneManager.Configure(Context.Scene, new SceneService.SceneConfig() { Update = true, Render = false, Resize = false });
            Context.Mode = EditorContext.Playmode.Playing;
        }

        private void StopScene()
        {
            Context.Mode = EditorContext.Playmode.Stopped;
            SceneManager.Configure(Context.Scene, new SceneService.SceneConfig() { Update = false, Render = false, Resize = false });
            SceneManager.Stop(Context.Scene);

            Debug.Assert(prevScene != null);
            Context.Scene = prevScene;
            prevScene = null;
        }
    }
}
