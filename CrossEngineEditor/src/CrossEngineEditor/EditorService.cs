using CrossEngine.Display;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Textures;
using CrossEngine.Scenes;
using CrossEngine.Core.Services;
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
using CrossEngine.Assemblies;
using CrossEngine.Core;
using CrossEngine.Events;
using CrossEngine.Platform.Glfw;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.IO;

namespace CrossEngineEditor
{
    internal class EditorService : Service
    {
        public readonly PanelManager Panels = new PanelManager();
        public IniFile Preferences;
        
        internal const string ConfigPreferencesPath = "preferences.ini";
        internal const string ConfigRecentsPath = "recents.ini";
        internal static Logger Log = new Logger("editor") { Color = 0xffCE1E6B };

        public readonly EditorContext Context;
        private Window window = null;
        
        // static modals
        private ExitModal _exitModal = new ExitModal() { Exit = () => EditorApplication.Instance.Close()}; // iks de
        private BlockModal _contextBlockModal = new BlockModal("Context Change");
        private int _blockDepth = 0;
        
        public EditorProject Project { get; set; }
        private List<string> _recentProjects;

        private Stream GetIconStream() => Assembly.GetAssembly(typeof(Application))
            .GetManifestResourceStream("CrossEngine.res.logo.png");

        public EditorService()
        {
            Context = new EditorContext((msg) =>
            {
                if (Panels.GetModal<BlockModal>() == null)
                {
                    _contextBlockModal.Open = null;
                    _contextBlockModal.Text = msg;
                    Panels.PushModal(_contextBlockModal);
                }
                _blockDepth++;
            }, () =>
            {
                _blockDepth--;
                if (_blockDepth == 0)
                {
                    _contextBlockModal.Open = false;
                    _contextBlockModal.Text = "";
                }
            });
            
            Panels.RegisterPanel(new InspectorPanel());
            Panels.RegisterPanel(new HierarchyPanel());
            //Panels.RegisterPanel(new SceneViewPanel(rs));
            Panels.RegisterPanel(new ViewportPanel());
            Panels.RegisterPanel(new GamePanel());
            Panels.RegisterPanel(new AssetListPanel());
            Panels.RegisterPanel(new SimpleThemeGeneratorPanel());
            
#if DEBUG
            Panels.PushPanel(new WidgetTestPanel());
#endif
        }

        public override void OnInit()
        {
            ReadConfig();
            
            Manager.Event += OnEvent;
            
            Log.Info("editor started");
        }

        public override void OnDestroy()
        {
            Manager.Event -= OnEvent;

            WriteConfig();
            
            Log.Info("editor closed");
        }

        public override unsafe void OnAttach()
        {
            // visuals
            Manager.GetService<WindowService>().Execute(() =>
            {
                window = Manager.GetService<WindowService>().MainWindow;
                
                // eeww
#if WINDOWS
                Theming.UseImmersiveDarkMode(Process.GetCurrentProcess().MainWindowHandle, true);
#endif
                
                var result = ImageResult.FromStream(GetIconStream(), ColorComponents.RedGreenBlueAlpha);
                fixed (void* p = &result.Data[0])
                    window.SetIcon(p, (uint)result.Width, (uint)result.Height);

            });
            var rs = Manager.GetService<RenderService>();
            rs.MainSurface.Update += OnRender;
            rs.Execute(() =>
            {
                dockspaceIconTexture = TextureLoader.LoadTextureFromStream(GetIconStream());
                dockspaceIconTexture.GetValue().SetFilterParameter(FilterParameter.Nearest);
            });

            Init();
        }

        public override void OnDetach()
        {
            Deinit();

            // dispose visuals
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
                    // check if exception is coming from within assembly
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
                ((GlfwWindow)window).RequestWindowAttention();
                if (Panels.GetModal<ExitModal>() == null)
                {
                    _exitModal.Open = true;
                    Panels.PushModal(_exitModal);
                }
                wce.Handled = true;
            }
        }

        private void InternalRender()
        {
            SetupDockspace(window);

            DrawMainMenuBar();

            var io = ImGui.GetIO();

            ImGui.ShowDemoWindow();
            Panels.Draw();

            EndDockspace();
        }

        private void Init()
        {
            Context.SceneChanged += OnContextSceneChanged;
            
            Panels.Init(Context);
        }

        private void Deinit()
        {
            Panels.Destroy();
            //Context.Clear();

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
            void DrawSceneDropdown()
            {
                var disable = Context.Assets == null;
                if (disable) ImGui.BeginDisabled();
                var name = GetCurrentSceneAsset()?.GetName();
                name ??= Context.Scene == null ? "<null>" : "<unknown>";
                ImGui.SetNextItemWidth(120);
                if (ImGui.BeginCombo("Scene", name))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        void CreateScene() => Context.SetScene(new Scene());

                        DialogDestructive(CreateScene, Context.Scene != null);
                    }
                    
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Save As...", Context.Scene != null))
                    {
                        DialogFileSave().ContinueWith(t =>
                        {
                            var filepath = t.Result;
                            if (filepath != null)
                                using (Stream stream = File.Create(filepath))
                                    SceneSerializer.SerializeJson(stream, Context.Scene);
                        });
                    }
                    if (ImGui.MenuItem("Dump", Context.Scene != null))
                        using (var stream = Console.OpenStandardOutput())
                            SceneSerializer.SerializeJson(stream, Context.Scene);
                    
                    ImGui.Separator();

                    if (Context.Assets?.HasCollection<SceneAsset>() == true)
                    {
                        foreach (var item in Context.Assets.GetCollection<SceneAsset>())
                        {
                            if (!item.Loaded) ImGui.BeginDisabled();
                            var isSelected = item.Scene != null && item.Scene == Context.Scene;
                            if (ImGui.Selectable(item.GetName(), isSelected))
                            {
                                void LoadScene()
                                {
                                    Context.SetScene(item.Scene);
                                }
                                DialogDestructive(LoadScene, Context.Scene != null);
                            }
                            if (isSelected) ImGui.SetItemDefaultFocus();
                            if (!item.Loaded) ImGui.EndDisabled();
                        }
                    }

                    ImGui.EndCombo();
                }
                
                if (disable) ImGui.EndDisabled();
            }
            
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    void OpenProject(string path)
                    {
                        Context.Clear().ContinueWith(t =>
                        {
                            Project = new EditorProject();
                            Project.Load(Context, path);

                            AppendRecent(Project.Filepath);
                        });
                    }

                    void SaveProject()
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(Project.Filepath));
                        Project.Save(Context, Project.Filepath);
                        
                        AppendRecent(Project.Filepath);
                    }
                    
                    if (ImGui.MenuItem("New...")) Panels.PushModal(new CreateProjectModal() {Callback = path =>
                    {
                        if (Path.Exists(path))
                        {
                            DialogGenericError();
                            return;
                        }
                        Project = new EditorProject();
                        Project.Filepath = path;
                        SaveProject();
                    }});
                    if (ImGui.MenuItem("Open..."))
                    {
                        DialogDestructive(() =>
                        {
                            DialogFileOpen().ContinueWith(t =>
                            {
                                if (t.Result == null) return;
                                OpenProject(t.Result);
                            });
                        });
                    }
                    if (ImGui.BeginMenu("Open Recent"))
                    {
                        for (int i = 0; i < _recentProjects.Count; i++)
                        {
                            var propth = _recentProjects[i];
                            if (ImGui.Selectable(propth))
                                DialogDestructive(() => OpenProject(propth));
                        }

                        ImGui.Separator();
                        
                        if (ImGui.MenuItem("Clear"))
                            _recentProjects.Clear();
                        
                        ImGui.EndMenu();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save"))
                    {
                        SaveProject();
                    }
                    if (ImGui.MenuItem("Save As..."))
                    {
                        DialogFileSave().ContinueWith(t =>
                        {
                            if (t.Result == null) return;
                            Project.Save(Context, t.Result);
                        });
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Quit"))
                    {
                        _exitModal.Open = true;
                        Panels.PushModal(_exitModal);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo")) throw new NotImplementedException();
                    if (ImGui.MenuItem("Redo")) throw new NotImplementedException();
                    
                    ImGui.Separator();
                    
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
                
                ImGui.SameLine();
                DrawSceneDropdown();

                var projectText = Project == null ? "*" : Project.Filepath;
                ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.CalcTextSize(projectText).X - ImGui.GetStyle().ItemSpacing.X * 2);
                ImGui.TextDisabled(projectText);

                ImGui.EndMainMenuBar();
            }
        }
        #endregion

        #region Context Changes
        private void OnContextSceneChanged(Scene old)
        {
            Context.ActiveEntity = null;
        }
        #endregion

        #region Dialogs
        internal Task<string> DialogFileOpen()
        {
            var modal = new BlockModal("File Dialog") { Text = "File open dialog is open." };
            Panels.PushModal(modal);
            return Task.Run(() => { var result = EditorPlatformHelper.FileOpenDialog(); modal.Open = false; return result; });
        }

        internal Task<string> DialogFileSave()
        {
            var modal = new BlockModal("File Dialog") { Text = "File save dialog is open." };
            Panels.PushModal(modal);
            return Task.Run(() => { var result = EditorPlatformHelper.FileSaveDialog(); modal.Open = false; return result; });
        }

        internal Task<string> DialogPickDirectory()
        {
            var modal = new BlockModal("File Dialog") { Text = "Pick directory dialog is open." };
            Panels.PushModal(modal);
            return Task.Run(() => { var result = EditorPlatformHelper.DirectoryPickDialog(); modal.Open = false; return result; });
        }

        internal void DialogDestructive(Action action, bool destructiveIf = true)
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

        internal void DialogGenericError(string msg = null)
        {
            Panels.PushModal(new ActionModal(msg ?? "Whoops...\nThat's an error.", "Error") { Color = ActionModal.TextColor.Error });
        }
        #endregion

        internal SceneAsset GetCurrentSceneAsset()
        {
            if (Context.Scene == null || Context.Assets?.HasCollection<SceneAsset>() == false)
                return null;
            
            return  Context.Assets.GetCollection<SceneAsset>().FirstOrDefault(a => a.Scene == Context.Scene);
        }

        internal void RendererRequest(Action action) => Manager.GetService<RenderService>().Execute(action);

        private void ReadConfig()
        {
            Log.Debug("configuring");
            
            // assert files
            if (!File.Exists(ConfigPreferencesPath)) File.Create(ConfigPreferencesPath).Close();
            if (!File.Exists(ConfigRecentsPath)) File.Create(ConfigRecentsPath).Close();
            
            Preferences = IniFile.Load(File.OpenRead(ConfigPreferencesPath));
            
            var ediini = IniFile.Load(File.OpenRead(ConfigRecentsPath));
            int i = 0;
            _recentProjects = new();
            while (ediini["recents"].TryReadString($"recent{i}", out var recent))
            {
                _recentProjects.Add(recent);
                i++;
            }
        }

        private void WriteConfig()
        {
            Log.Debug("saving configuration");

            var ediini = new IniFile();
            for (int i = 0; i < _recentProjects.Count; i++)
            {
                ediini["recents"].Write($"recent{i}", _recentProjects[i]);
            }
            IniFile.Dump(ediini, File.OpenWrite(ConfigRecentsPath));
        }

        private void AppendRecent(string filepath)
        {
            if (!_recentProjects.Contains(filepath)) _recentProjects.Add(filepath);
        }
    }
}
