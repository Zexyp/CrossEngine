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
using System.Reflection;
using CrossEngineEditor.Utils;
using StbImageSharp;
using CrossEngine.Assets;
using System.Text.Json;
using CrossEngine.Serialization;
using CrossEngine.Utils.ImGui;
using System.Threading.Channels;
using CrossEngine.Loaders;
using CrossEngine.Rendering;
using CrossEngineEditor.Modals;
using CrossEngineEditor.Platform;
using System.IO;
using CrossEngine.Events;

namespace CrossEngineEditor
{
    internal class EditorService : Service
    {
        public readonly PanelManager Panels = new PanelManager();
        public IniFile Preferences;
        
        internal const string PreferencesPath = "preferences.ini";
        internal Logger Log = new Logger("editor") { Color = 0xffCE1E6B };
        
        readonly EditorContext Context = new EditorContext();
        private Window window = null;
        
        public override void OnStart()
        {
            if (!File.Exists(PreferencesPath))
                File.Create(PreferencesPath).Close();
            Preferences = IniFile.Load(File.OpenRead(PreferencesPath));
            
            Panels.Init(Context);
            
            Log.Info("editor started");

            Manager.Event += OnEvent;
        }

        public override void OnDestroy()
        {
            Manager.Event -= OnEvent;

            Panels.Destroy();
            
            Log.Info("editor closed");
        }

        public override unsafe void OnAttach()
        {
            var rs = Manager.GetService<RenderService>();
            Manager.GetService<WindowService>().Execute(() =>
            {
                window = Manager.GetService<WindowService>().MainWindow;
                
                // eeww
#if WINDOWS
                Theming.UseImmersiveDarkMode(Process.GetCurrentProcess().MainWindowHandle, true);
#endif
                var result = ImageResult.FromMemory(CrossEngine.Properties.Resources.Logo, ColorComponents.RedGreenBlueAlpha);
                fixed (void* p = &result.Data[0])
                    window.SetIcon(p, (uint)result.Width, (uint)result.Height);

            });
            rs.MainSurface.Update += OnRender;
            rs.Execute(() =>
            {
                dockspaceIconTexture = TextureLoader.LoadTextureFromBytes(CrossEngine.Properties.Resources.Logo);
                dockspaceIconTexture.GetValue().SetFilterParameter(FilterParameter.Nearest);
            });

            Init();
        }

        public override void OnDetach()
        {
            Deinit();

            var rs = Manager.GetService<RenderService>();
            rs.MainSurface.Update -= OnRender;
            rs.Execute(() =>
            {
                dockspaceIconTexture.Dispose();
                dockspaceIconTexture = null;
            });
        }

        private void OnRender(ISurface surface)
        {
            Profiler.BeginScope();

            try
            {
                try
                {
                    InternalRender();
                }
                catch (NotImplementedException nie)
                {
                    var trace = new StackTrace(nie);
                    var frame = trace.GetFrame(0);
                    if (frame.GetMethod().DeclaringType.Assembly != Assembly.GetExecutingAssembly())
                        throw;
                    Log.Error($"action at {frame.GetMethod().DeclaringType}.{frame.GetMethod().Name} in {frame.GetFileName()}:{frame.GetFileLineNumber()} not implemented ({nie.Message})");
                }
            }
            catch (Exception e)
            {
                Log.Fatal($"ui drawing fail");
                throw;
            }

            Profiler.EndScope();
        }
        
        private void OnEvent(CrossEngine.Events.Event e)
        {
            if (e is WindowCloseEvent wce)
            {
                DestructiveDialog(EditorApplication.Instance.Close);
                wce.Handled = true;
            }
        }

        private void InternalRender()
        {
            SetupDockspace(window);

            DrawMainMenuBar();

            var io = ImGui.GetIO();

            ImGui.ShowDemoWindow();
            Profiler.BeginScope($"{nameof(PanelManager)}.{nameof(PanelManager.Draw)}");
            Panels.Draw();
            Profiler.EndScope();

            EndDockspace();
        }

        // pretty weird
        private void Init()
        {
            Context.AssetsChanged += OnContextAssetsChanged;
            Context.SceneChanged += OnContextSceneChanged;

            var rs = Manager.GetService<RenderService>();
            Panels.RegisterPanel(new InspectorPanel());
            Panels.RegisterPanel(new HierarchyPanel());
            //Panels.RegisterPanel(new SceneViewPanel(rs));
            Panels.RegisterPanel(new ViewportPanel(rs));
            Panels.RegisterPanel(new GamePanel(rs));
            Panels.RegisterPanel(new AssetListPanel());
            Panels.RegisterPanel(new SimpleThemeGeneratorPanel());
        }

        private void Deinit()
        {
            //Context.Clear();

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
                    if (ImGui.MenuItem("New...")) Panels.PushModal(new CreateProjectModal());
                    if (ImGui.MenuItem("Open...")) throw new NotImplementedException();
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save...")) throw new NotImplementedException();
                    if (ImGui.MenuItem("Save As...")) throw new NotImplementedException();
                    ImGui.Separator();
                    if (ImGui.MenuItem("Quit"))
                        DestructiveDialog(EditorApplication.Instance.Close);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Preferences..."))
                        Panels.PushModal(new PreferencesModal());
                    
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.BeginMenu("Panels"))
                    {
                        for (int i = 0; i < Panels.Registered.Count; i++)
                        {
                            var p = Panels.Registered[i];
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
                
                // todo: move to dropdown
                if (ImGui.BeginMenu("Scene", Context.Assets != null))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        void CreateScene() => Context.Scene = new Scene();

                        DestructiveDialog(CreateScene, Context.Scene != null);
                    }
                    if (ImGui.BeginMenu("Load", Context.Assets != null))
                    {
                        if (Context.Assets?.HasCollection<SceneAsset>() == true)
                        {
                            foreach (var item in Context.Assets.GetCollection<SceneAsset>())
                            {
                                if (!item.Loaded) ImGui.BeginDisabled();
                                if (ImGui.Selectable(item.GetName(), item.Scene != null && item.Scene == Context.Scene))
                                {
                                    void LoadScene()
                                    {
                                        Context.Scene = null;
                                        Context.Scene = item.Scene;
                                    }
                                    DestructiveDialog(LoadScene, Context.Scene != null);
                                }
                                if (!item.Loaded) ImGui.EndDisabled();
                            }
                        }

                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save As...", Context.Scene != null))
                    {
                        var filepath = EditorPlatformHelper.FileSaveDialog();
                        if (filepath != null)
                            using (Stream stream = File.Create(filepath))
                                SceneSerializer.SerializeJson(stream, Context.Scene);
                    }
                    if (ImGui.MenuItem("Dump", Context.Scene != null))
                        using (var stream = Console.OpenStandardOutput())
                            SceneSerializer.SerializeJson(stream, Context.Scene);

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }
        #endregion
        
        private void OnContextSceneChanged(Scene old)
        {
            Context.ActiveEntity = null;

            if (old != null) SceneManager.Remove(old);

            if (Context.Scene != null) SceneManager.PushBackground(Context.Scene);
        }

        private void OnContextAssetsChanged(AssetList old)
        {
            Context.Scene = null;

            if (old != null) AssetManager.Current.UnloadAll();

            AssetManager.Bind(Context.Assets);

            if (Context.Assets != null) AssetManager.Current.LoadAll();
        }

        internal void DestructiveDialog(Action action, bool destructiveIf = true)
        {
            if (destructiveIf)
                Panels.PushModal(new ActionModal("Are you sure?", "Destructive", ActionModal.ButtonFlags.YesNo)
                {
                    Color = ActionModal.TextColor.Warn,
                    Success = action
                });
            else
                action.Invoke();
        }
    }
}
