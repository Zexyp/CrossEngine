using ImGuiNET;
using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Globalization;

using CrossEngine;
using CrossEngine.ECS;
using CrossEngine.Events;
using CrossEngine.Layers;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Physics;
using CrossEngine.Serialization;
using CrossEngine.Profiling;
//using CrossEngine.Assemblies;
using CrossEngine.Inputs;
using CrossEngine.Components;
using CrossEngine.Debugging;

using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.Gui;
using CrossEngineEditor.Panels;
using CrossEngineEditor.Modals;

namespace CrossEngineEditor
{
    static class EditorConfigSections
    {
        public static readonly string PanelsOpen = "panels.open";
    }

    public class EditorLayer : Layer
    {
        // instatnce
        static public EditorLayer Instance;

        // contexts
        public EditorCamera EditorCamera = new EditorCamera();
        
        public readonly EditorContext Context = new EditorContext();

        private Scene workingScene = null;
        public bool SceneUpdate = true;

        // ui things
        private List<EditorPanel> _panels = new List<EditorPanel>();
        private List<EditorModal> _modals = new List<EditorModal>();
        //Texture dockspaceIconTexture;

        // files
        readonly internal IniFile EditorConfig = new IniFile("editor");
        private string _savePath = null;
        private string SavePath
        {
            set
            {
                _savePath = value;

                var window = Application.Instance.Window;
                if (_savePath == null) window.Title = "CrossEngine Editor";
                else window.Title = $"CrossEngine Editor [{_savePath}]";
            }
        }

        // events
        public event Action PlaymodeStarted;
        public event Action PlaymodeEnded;

        public EditorLayer()
        {
            if (Instance != null)
                Log.App.Warn("there should be only one editor layer");

            Instance = this;

            AddPanel(new InspectorPanel());
            AddPanel(new HierarchyPanel());
            //AddPanel(new GamePanel());
            AddPanel(new ViewportPanel());
            //AddPanel(new GizmoPanel());
            AddPanel(new LagometerPanel());
            //AddPanel(new ImageViewerPanel());
            AddPanel(new ConfigPanel());

            var ci = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = ci;
        }

        private bool LoadConfig(IniFile config)
        {
            bool success = true;

            // panels
            {
                bool valid = true;
                for (int i = 0; i < _panels.Count; i++)
                {
                    EditorPanel panel = _panels[i];

                    string stringValue = config.Read(EditorConfigSections.PanelsOpen, panel.GetType().Name);

                    if (String.IsNullOrEmpty(stringValue)) continue;

                    if (bool.TryParse(stringValue, out bool val))
                    {
                        panel.Open = val;
                    }
                    else if (stringValue == "null")
                    {
                        panel.Open = null;
                    }
                    else
                    {
                        valid = false;
                        continue;
                    }
                }

                if (!valid) EditorApplication.Log.Trace($"invalid config section '{EditorConfigSections.PanelsOpen}'");

                success = success && valid;
            }

            return success;
        }

        private void SaveConfig(IniFile config)
        {
            // panels
            {
                for (int i = 0; i < _panels.Count; i++)
                {
                    EditorPanel panel = _panels[i];
                    config.Write(EditorConfigSections.PanelsOpen, panel.GetType().Name, (panel.Open != null) ? panel.Open.ToString() : "null" );
                }
            }
        }

        bool corruptedConfig = false;

        public override void OnAttach()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                GLDebugging.EnableGLDebugging(LogLevel.Warn);
                Renderer2D.Init();
                LineRenderer.Init();
                Application.Instance.RendererAPI.SetDepthFunc(DepthFunc.Default);
                Application.Instance.RendererAPI.SetBlendFunc(BlendFunc.OneMinusSrcAlpha);
            });

            //dockspaceIconTexture = new Rendering.Textures.Texture(Properties.Resources.DefaultWindowIcon.ToBitmap());
            //dockspaceIconTexture.SetFilterParameter(Rendering.Textures.FilterParameter.Nearest);

            if (!LoadConfig(EditorConfig)) corruptedConfig = true;
            if (!ImGuiStyleConfig.Load(new IniFile("style"))) corruptedConfig = true;

            // --- test code
            Context.Scene = new Scene();
            Application.Instance.GetLayer<SceneLayer>().AddScene(Context.Scene);
            Context.Scene.Init();
            for (int i = -5; i <= 5; i++)
            {
                if (i == 0)
                {
                    Entity okl = Context.Scene.CreateEntity();
                    okl.GetComponent<TransformComponent>().WorldPosition = new Vector3(i, 0, 0);
                    okl.AddComponent(new TagComponent("Main"));
                    okl.AddComponent(new SpriteRendererComponent() { Color = new Vector4(0, 1, 0, 1)});
                    okl.AddComponent(new CameraComponent(new OrthographicCamera() { OrthographicSize = 10 }) { Primary = true });
                    okl.AddComponent<ParticleSystemComponent>();
                    continue;
                }
                Entity ent = Context.Scene.CreateEntity();
                ent.GetComponent<TransformComponent>().WorldPosition = new Vector3(i, 0, 0);
                ent.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), /*Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.LoadTexture("textures/prototype_512x512_grey1.png"))*/ });
                ent.AddComponent(new TagComponent("asd" + i));
                ent.AddComponent(new RigidBodyComponent() { LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
                ent.AddComponent(new BoxColliderComponent());
            }
            
            Entity ground = Context.Scene.CreateEntity();
            ground.GetComponent<TransformComponent>().Scale = new Vector3(10, 1, 1);
            ground.GetComponent<TransformComponent>().Position = new Vector3(0, -5, 0);
            ground.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), /*Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.LoadTexture("textures/prototype_512x512_grey1.png"))*/ });
            ground.AddComponent(new RigidBodyComponent() { Static = true, LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
            ground.AddComponent(new BoxColliderComponent());

            Context.Scene.Start();

            gre.AddElement(0, Vector4.Zero);
            gre.AddElement(1, Vector4.One);
        }

        public override void OnDetach()
        {
            //Scene = null;
            //AssetManager.Textures.Purge();
            //GC.Collect();
            //CrossEngine.Assets.GC.GPUGarbageCollector.Collect();
        }

        // test pcode
        int updSleep = 0;
        int rndSleep = 0;
        Gradient<Vector4> gre = new Gradient<Vector4>();

        int profFrames = 0;
        public override void OnUpdate()
        {
            if (profFrames > 0)
            {
                profFrames--;
                if (profFrames <= 0) Application.Instance.EndProfiler();
            }
            if (Input.GetKeyDown(Key.F9))
            {
                profFrames = 60;
                Application.Instance.StartProfiler();
            }
        }

        public override void OnEvent(Event e)
        {
            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.OnEvent)}");

            for (int i = _panels.Count - 1; i >= 0; i--)
            {
                _panels[i].OnEvent(e);
            }

            Profiler.EndScope();



            if (e is not ImGuiRenderEvent) return;

            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.OnRender)}");

            SetupDockspace();

            DrawModals();

            DrawMainMenuBar();

            ImGui.ShowDemoWindow(); // purely dev thing

            if (corruptedConfig)
            {
                PushModal(new ActionModal("Config seems to be corrupted!", "Ouha!", ActionModal.ButtonFlags.OK));
                corruptedConfig = false;
            }

            // debug
            ImGui.Begin("Debug");
            if (Context.Scene != null)
            {
                ImGui.SliderInt("upd sleep", ref updSleep, 0, 1000);
                ImGui.SliderInt("rnd sleep", ref rndSleep, 0, 1000);
                if (updSleep > 0) ThreadManager.ExecuteOnMianThread(() => Thread.Sleep(updSleep));
                if (rndSleep > 0) ThreadManager.ExecuteOnRenderThread(() => Thread.Sleep(rndSleep));

                ImGradient.Manipulate(gre);

                // test seri
                if (ImGui.Button("seri test"))
                {
                    string json;
                    json = SceneSerializer.SertializeJson(Context.Scene);
                    Log.App.Debug(json);
                
                    //Context.Scene.Unload();
                    //ClearContext();
                    //Context.Scene = SceneSerializer.DeserializeJson(json);
                    //Context.Scene.Load();
                }
            }
            ImGui.End();


            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.DrawPanels)}");
            DrawPanels();
            Profiler.EndScope();

            EndDockspace();

            Profiler.EndScope();
        }

        #region Dockspace
        private unsafe void SetupDockspace()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(Application.Instance.Window.Width, Application.Instance.Window.Height));

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
        #endregion

        private void DrawPanels()
        {
            for (int i = 0; i < _panels.Count; i++)
            {
                _panels[i].Draw();
            }
        }

        private void DrawMainMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                /*
                #region File Menu
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModal.ButtonFlags.OKCancel, (flags) =>
                        {
                            if (flags == ActionModal.ButtonFlags.OK) FileNewScene();
                        }));
                    }

                    if (ImGui.MenuItem("Open..."))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModal.ButtonFlags.OKCancel, (flags) =>
                        {
                            if (flags == ActionModal.ButtonFlags.OK) FileOpenScene();
                        }));
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save", Context.Scene != null))
                    {
                        FileSaveScene();
                    }

                    if (ImGui.MenuItem("Save As...", Context.Scene != null))
                    {
                        FileSaveSceneAs();
                    }

                    if (ImGui.MenuItem("Save Copy...", Context.Scene != null))
                    {
                        FileSaveSceneAs(true);
                    }

                    ImGui.EndMenu();
                }
                #endregion
                */

                if (ImGui.BeginMenu("Edit"))
                {
                    ImGui.Separator();

                    if (ImGui.MenuItem("Config"))
                    {
                        GetPanel<ConfigPanel>().Open = true;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.BeginMenu("Panels"))
                    {
                        for (int i = 0; i < _panels.Count; i++)
                        {
                            if (_panels[i].Open != null)
                            {
                                if (ImGui.MenuItem(_panels[i].WindowName, null, (bool)_panels[i].Open))
                                {
                                    _panels[i].Open = !(bool)_panels[i].Open;
                                }
                            }
                            else
                            {
                                ImGui.MenuItem(_panels[i].WindowName);
                            }
                        }

                        ImGui.Separator();

                        if (ImGui.MenuItem("Save Open States"))
                        {
                            SaveConfig(EditorConfig);
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                /*
                if (ImGui.BeginMenu("Resources", Context.Scene != null))
                {
                    //if (ImGui.BeginMenu("Import Assets"))
                    //{
                    //    if (ImGui.MenuItem("Texture"))
                    //    {
                    //        
                    //    }
                    //
                    //    ImGui.EndMenu();
                    //}
                    //
                    //ImGui.Separator();

                    if (ImGui.BeginMenu("Assemblies"))
                    {
                        if (ImGui.MenuItem("Load..."))
                        {
                            AssembliesLoad();
                            ResetComponentTypeRegistry();
                        }
                        if (ImGui.MenuItem("Load List..."))
                        {
                            AssembliesLoadList();
                            ResetComponentTypeRegistry();
                        }

                        ImGui.Separator();

                        if (ImGui.BeginMenu("Reload", AssemblyLoader.LoadedAssemblies.Count > 0))
                        {
                            if (ImGui.MenuItem("All"))
                            {
                                AssemblyLoader.ReloadAll();
                                ResetComponentTypeRegistry();
                            }

                            ImGui.Separator();

                            foreach (var item in AssemblyLoader.LoadedAssemblies)
                            {
                                if (ImGui.MenuItem(item.Path))
                                {
                                    AssemblyLoader.Reload(item.Path);
                                    ResetComponentTypeRegistry();
                                    break;
                                }
                            }

                            ImGui.EndMenu();
                        }

                        ImGui.Separator();

                        if (ImGui.MenuItem("Save List Copy..."))
                        {
                            AssembliesSaveList();
                        }

                        ImGui.Separator();

                        if (ImGui.BeginMenu("Unload", AssemblyLoader.LoadedAssemblies.Count > 0))
                        {
                            foreach (var item in AssemblyLoader.LoadedAssemblies)
                            {
                                if (ImGui.MenuItem(item.Path))
                                {
                                    AssemblyLoader.Unload(item.Path);
                                    ResetComponentTypeRegistry();
                                    break;
                                }
                            }

                            ImGui.EndMenu();
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                */

                //// play mode button
                //Vector2 cp = ImGui.GetCursorPos();
                //cp.X += ImGui.GetColumnWidth() / 2;
                //ImGui.SetCursorPos(cp);
                //bool colorPushed = Context.Scene?.Running == true;
                //if (colorPushed) ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000dd/*new Vector4(1, 0.2f, 0.1f, 1)*/);
                //if (ImGui.ArrowButton("##play", Context.Scene?.Running == true ? ImGuiDir.Down : ImGuiDir.Right))
                //{
                //    if (Context.Scene != null)
                //    {
                //        if (!Context.Scene.Running)
                //        {
                //            StartPlaymode();
                //            var gamePanel = GetPanel<GamePanel>();
                //            if (gamePanel != null)
                //            {
                //                // initial resize event
                //                Context.Scene.OnEvent(new WindowResizeEvent((uint)gamePanel.WindowSize.X, (uint)gamePanel.WindowSize.Y));
                //                ImGui.SetWindowFocus(gamePanel.WindowName);
                //            }
                //        }
                //        else
                //        {
                //            EndPlaymode();
                //            SceneUpdate = true;
                //            ImGui.SetWindowFocus(GetPanel<ViewportPanel>()?.WindowName);
                //        }
                //    }
                //}
                //if (colorPushed) ImGui.PopStyleColor();

                //ImGui.Checkbox("##update", ref EditorLayer.Instance.SceneUpdate);

                ImGui.EndMainMenuBar();
            }
        }

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

            panel.Attached = false;
            panel.OnDetach();

            _panels.Remove(panel);
        }

        public T GetPanel<T>() where T : EditorPanel
        {
            Type typeOfPanel = typeof(T);
            for (int i = 0; i < _panels.Count; i++)
            {
                if (_panels[i].GetType() == typeOfPanel)
                    return (T)_panels[i];
            }
            return null;
        }
        #endregion

        #region Modal Methods
        private void DrawModals()
        {
            for (int i = 0; i < _modals.Count; i++)
            {
                if (!_modals[i].Draw())
                    _modals.RemoveAt(i);
            }
        }

        public void PushModal(EditorModal modal)
        {
            ImGui.OpenPopup(modal.ModalName);

            _modals.Add(modal);
        }

        public void PopModal(EditorModal modal)
        {
            _modals.Remove(modal);
        }
        #endregion

        #region Context
        public void ClearContext()
        {
            Context.SelectedEntities.Clear();
            Context.ActiveEntity = null;
        }
        #endregion

        /*
        #region File Menu Actions
        private void FileNewScene()
        {
            Context.Scene?.Unload();
            Context.Scene?.Destroy();

            ClearContext();

            Context.Scene = new Scene();
            Context.Scene.Load();

            SavePath = null;
        }
        private void FileOpenScene()
        {
            if (FileDialog.Open(out string path,
                                    filter:
                                    "Scene File Env. INI (*.sfe.ini)\0*.sfe.ini\0" +
                                    "All Files (*.*)\0*.*\0"))
            {
                Context.Scene?.Unload();
                Context.Scene?.Destroy();

                AssemblyLoader.UnloadAll();

                ClearContext();

                Context.Scene = SceneLoader.Load(path);
                Context.Scene.Load();

                SavePath = path;
            }
        }
        private void FileSaveScene()
        {
            if (_savePath != null)
            {
                EditorApplication.Log.Warn("sry not implemented");
                FileSaveSceneAs();
            }
            else
            {
                FileSaveSceneAs();
            }
        }
        private void FileSaveSceneAs(bool copy = false)
        {
            if (FileDialog.Save(out string path,
                            filter:
                            "Scene File Env. INI (*.sfe.ini)\0*.sfe.ini\0" +
                            "All Files (*.*)\0*.*\0",
                            name: "scene"))
            {
                string asmListFile = $"./{Path.GetFileNameWithoutExtension(path)}.assemblies.list";
                string sceneFile = $"./{Path.GetFileNameWithoutExtension(path)}.scene.json";

                if (File.Exists(asmListFile) || File.Exists(sceneFile))
                {
                    PushModal(new ActionModal("File collison detected!\nOverwrite?",
                        ActionModal.ButtonFlags.OKCancel,
                        (button) => { if (button == ActionModal.ButtonFlags.OK) FinishSave(); }
                        ));
                    return;
                }

                FinishSave();
                void FinishSave()
                {
                    SceneLoader.Save(path, Context.Scene, new SceneFileEnvironment()
                    {
                        AssembliesList = asmListFile,
                        Scene = sceneFile,
                    });

                    if (!copy) SavePath = path;
                }
            }
        }
        #endregion
        */

        /*
        #region Assemblies Menu Actions
        private void AssembliesLoad()
        {
            if (FileDialog.Open(out string path,
                                    filter:
                                    "DLL (*.dll)\0*.dll\0" +
                                    "All Files (*.*)\0*.*\0"))
            {
                AssemblyLoader.Load(path);
            }
        }

        private void AssembliesLoadList()
        {
            if (FileDialog.Open(out string listpath,
                                    filter:
                                    "List (*.list)\0*.list\0" +
                                    "All Files (*.*)\0*.*\0"))
            {
                string[] paths = Array.ConvertAll(File.ReadAllText(listpath)
                        .Trim('\n', ' ').Split("\n"),
                        str => str.Trim(' '))
                    .Where(str => !string.IsNullOrWhiteSpace(str))
                    .ToArray();
                for (int i = 0; i < paths.Length; i++)
                {
                    AssemblyLoader.Load(paths[i]);
                }
            }
        }

        private void AssembliesSaveList()
        {
            if (FileDialog.Save(out string path,
                                    name: "asseblies",
                                    filter:
                                    "List (*.list)\0*.list\0" +
                                    "All Files (*.*)\0*.*\0"))
            {
                File.WriteAllText(path, String.Join("\n", AssemblyLoader.LoadedAssemblies.Select(pair => pair.Path)));
            }
        }

        private void ResetComponentTypeRegistry()
        {
            _componentTypeRegistry.Clear();
            _componentTypeRegistry.AddRange(AssemblyLoader.GetSubclassesOf(typeof(Component)));
        }
        #endregion
        */

        /*
        #region Real Magic
        public void StartPlaymode()
        {
            EditorApplication.Log.Info("starting playmode");

            string json;
            json = SceneSerializer.SertializeJson(Context.Scene);
            //EditorApplication.Log.Debug($"\n{json}");
            workingScene = Context.Scene;
            ClearContext();

            Context.Scene = SceneSerializer.DeserializeJson(json);

            Context.Scene.Load();
            Context.Scene.Start();

            EditorApplication.Log.Info("playmode started");
        }

        public void EndPlaymode()
        {
            EditorApplication.Log.Info("ending playmode");

            Context.Scene.End();
            Context.Scene.Unload();
            Context.Scene.Destroy();

            Context.Scene = workingScene;
            ClearContext();
            workingScene = null;

            EditorApplication.Log.Info("playmode ended");
        }
        #endregion
        */
    }
}
