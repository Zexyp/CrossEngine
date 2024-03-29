﻿using ImGuiNET;
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
using CrossEngine.Inputs;
using CrossEngine.Components;
using CrossEngine.Debugging;
using CrossEngine.Assemblies;

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
        //public EditorCamera EditorCamera = new EditorCamera();
        
        public readonly EditorContext Context = new EditorContext();

        private Scene workingScene = null;
        //public bool SceneUpdate = true;

        // ui things
        private List<EditorPanel> _panels = new List<EditorPanel>();
        private List<EditorModal> _modals = new List<EditorModal>();
        //Texture dockspaceIconTexture;

        // files
        readonly internal IniFile EditorConfig = new IniFile("editor");
        //private string _scenePath = null;
        //private string ScenePath
        //{
        //    get => _scenePath;
        //    set
        //    {
        //        _scenePath = value;
        //
        //        var window = Application.Instance.Window;
        //        if (_scenePath == null) window.Title = "CrossEngine Editor";
        //        else window.Title = $"CrossEngine Editor [{_scenePath}]";
        //    }
        //}

        // events
        public event Action PlaymodeStarted;
        public event Action PlaymodeEnded;

        public EditorLayer()
        {
            if (Instance != null)
                EditorApplication.Log.Warn("there should be only one editor layer");

            Instance = this;

            AddPanel(new InspectorPanel());
            AddPanel(new HierarchyPanel());
            AddPanel(new GamePanel());
            AddPanel(new ViewportPanel());
            AddPanel(new LagometerPanel());
            AddPanel(new ConfigPanel());
            //AddPanel(new ImageViewerPanel());
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
            //dockspaceIconTexture = new Rendering.Textures.Texture(Properties.Resources.DefaultWindowIcon.ToBitmap());
            //dockspaceIconTexture.SetFilterParameter(Rendering.Textures.FilterParameter.Nearest);

            if (!LoadConfig(EditorConfig)) corruptedConfig = true;
            if (!ImGuiStyleConfig.Load(new IniFile("style"))) corruptedConfig = true;

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

            SceneManager.Update();
        }

        public override void OnEvent(Event e)
        {
            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.OnEvent)}");

            for (int i = _panels.Count - 1; i >= 0; i--)
            {
                _panels[i].OnEvent(e);
            }

            Profiler.EndScope();

            if (e is WindowCloseEvent)
            {
                /*
                PushModal(new ActionModal("Do you want to exti?", "Exit?", ActionModal.ButtonFlags.OK | ActionModal.ButtonFlags.No,
                    (button) =>
                    {
                        if ((button & ActionModal.ButtonFlags.OK) != 0)
                            Application.Instance.Window.ShouldClose = true;
                    }));
                e.Handled = true;
                */
                EditorApplication.Log.Debug("TODO: uncomment");
                return;
            }

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
                    json = SceneSerializer.SerializeJsonToString(Context.Scene);
                    Application.Log.Debug(json);

                    var oldOut = Context.Scene.RenderData.Output;

                    //Context.Scene.Stop();
                    Context.Scene.Unload();
                    //Context.Scene.Unload();
                    Context.Scene = SceneSerializer.DeserializeJsonFromString(json);
                    Context.Scene.Load();
                    //Context.Scene.Start();

                    Context.Scene.RenderData.Output = oldOut;

                    //Context.Scene.Load();
                }
            }

            if (ImGui.Button("get ex dir"))
            {
                Console.WriteLine(Environment.CurrentDirectory);
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
                ImGui.PushID(_panels[i].GetHashCode());
                _panels[i].Draw();
                ImGui.PopID();
            }
        }

        private void DrawMainMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                #region File Menu
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Project..."))
                    {
                        if (Context.Project != null)
                            PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", "Are you sure?", ActionModal.ButtonFlags.OKCancel, (flags) =>
                            {
                                if (flags == ActionModal.ButtonFlags.OK) FileMenu_NewProject();
                            }));
                        else
                            FileMenu_NewProject();
                    }
                
                    if (ImGui.MenuItem("Open Project..."))
                    {
                        if (Context.Project != null)
                            PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", "Are you sure?", ActionModal.ButtonFlags.OKCancel, (flags) =>
                            {
                                if (flags == ActionModal.ButtonFlags.OK) FileMenu_OpenProject();
                            }));
                        else
                            FileMenu_OpenProject();
                    }
                    
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Save Project", Context.Project != null))
                    {
                        Context.Project.Save();
                    }

                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Create Scene...", Context.Project != null))
                    {
                        FileMenu_CreateScene();
                    }

                    if (ImGui.MenuItem("Save Scene", Context.Scene != null))
                    {
                        Context.Project.SaveScene(Context.Scene);
                    }

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Load Scene", Context.Project != null))
                    {
                        string[] scenes = Directory.GetDirectories(Context.Project.ScenesDir);
                        for (int i = 0; i < scenes.Length; i++)
                        {
                            if (ImGui.MenuItem(Path.GetFileName(scenes[i])))
                            {
                                Context.Scene?.Unload();
                                Context.Scene = SceneLoader.Read(Path.Combine(scenes[i], "scene.json"));
                                Context.Scene.Load();
                            }
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Entry Scene", Context.Project != null))
                    {
                        string[] scenes = Directory.GetDirectories(Context.Project.ScenesDir);
                        for (int i = 0; i < scenes.Length; i++)
                        {
                            string name = Path.GetFileName(scenes[i]);
                            if (ImGui.MenuItem(name, "", name == Context.Project.EntryScene))
                            {
                                Context.Project.EntryScene = name;
                            }
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                #endregion

                #region Edit Menu
                if (ImGui.BeginMenu("Edit"))
                {
                    ImGui.Separator();

                    if (ImGui.MenuItem("Config"))
                    {
                        GetPanel<ConfigPanel>().Open = true;
                    }

                    ImGui.EndMenu();
                }
                #endregion

                #region Window Menu
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
                #endregion

                if (ImGui.BeginMenu("Resources", Context.Project != null))
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
                        if (ImGui.MenuItem("Reload"))
                        {
                            AssemblyLoader.UnloadAll();
                            string[] files = Directory.GetFiles(Context.Project.AssembliesDir);
                            for (int i = 0; i < files.Length; i++)
                            {
                                AssemblyLoader.Load(files[i]);
                            }
                        }
                        
                        ImGui.EndMenu();
                    }
                
                    ImGui.EndMenu();
                }

                if (Context.Scene != null)
                {
                    // play mode button
                    Vector2 cp = ImGui.GetCursorPos();
                    cp.X += ImGui.GetColumnWidth() / 2;
                    ImGui.SetCursorPos(cp);
                    bool colorPushed = Context.Playmode == true;
                    if (colorPushed) ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000dd/*new Vector4(1, 0.2f, 0.1f, 1)*/);
                    if (ImGui.ArrowButton("##play", Context.Playmode ? ImGuiDir.Down : ImGuiDir.Right))
                    {
                        if (!Context.Playmode)
                        {
                            StartPlaymode();
                            var gamePanel = GetPanel<GamePanel>();
                            if (gamePanel != null)
                            {
                                ImGui.SetWindowFocus(gamePanel.WindowName);
                            }
                        }
                        else
                        {
                            EndPlaymode();
                            ImGui.SetWindowFocus(GetPanel<ViewportPanel>()?.WindowName);
                        }
                    }
                    if (colorPushed) ImGui.PopStyleColor();

                    //ImGui.Checkbox("##update", ref EditorLayer.Instance.SceneUpdate);
                }

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
                var mod = _modals[i];
                ImGui.PushID(mod.GetHashCode());
                if (!mod.Pushed)
                {
                    mod.Pushed = true;
                    ImGui.OpenPopup(mod.ModalName);
                }

                // TODO: stacked modals need to handled recursivly
                if (!mod.Draw())
                {
                    _modals.RemoveAt(i);
                    i--;
                }
                ImGui.PopID();
            }
        }

        public void PushModal(EditorModal modal)
        {
            _modals.Add(modal);
        }

        public void PopModal(EditorModal modal)
        {
            _modals.Remove(modal);
        }
        #endregion

        #region Real Magic
        private void StartPlaymode()
        {
            EditorApplication.Log.Info("starting playmode");

            Context.Playmode = true;

            string json;
            json = SceneSerializer.SerializeJsonToString(Context.Scene);
            EditorApplication.Log.Debug($"\n{json}");
            workingScene = Context.Scene;

            Context.Scene = SceneSerializer.DeserializeJsonFromString(json);

            string[] scenes = Directory.GetDirectories(Context.Project.ScenesDir);
            for (int i = 0; i < scenes.Length; i++)
            {
                SceneManager.Add(scenes[i]);
            }
            SceneManager.Load(Context.Scene);

            PlaymodeEnded?.Invoke();

            EditorApplication.Log.Info("playmode started");
        }

        private void EndPlaymode()
        {
            EditorApplication.Log.Info("ending playmode");

            Context.Playmode = false;

            PlaymodeEnded?.Invoke();

            SceneManager.Clear();
            SceneManager.Reset();

            Context.Scene = workingScene;
            workingScene = null;

            EditorApplication.Log.Info("playmode ended");
        }
        #endregion

        #region File Menu Actions
        private void FileMenu_NewProject()
        {
            if (FileDialog.Save(out string path,
                            filter:
                            FileDialog.Filters.IniFile +
                            FileDialog.Filters.AllFiles,
                            name: "ce"))
            {
                Context.Project = EditorProject.Create(path);
            }
        }

        private void FileMenu_OpenProject()
        {
            if (FileDialog.Open(out string path,
                            filter:
                            FileDialog.Filters.IniFile +
                            FileDialog.Filters.AllFiles))
            {
                Context.Project = EditorProject.Read(path);
                var ass = Directory.GetFiles(Context.Project.AssembliesDir);
                for (int i = 0; i < ass.Length; i++)
                {
                    AssemblyLoader.Load(ass[i]);
                }
            }
        }

        private void FileMenu_CreateScene()
        {
            if (FileDialog.Save(out string path,
                            filter:
                            FileDialog.Filters.JsonFile +
                            FileDialog.Filters.AllFiles,
                            name: "scene"))
            {
                Context.Scene?.Unload();
                Context.Scene = Context.Project.CreateScene(Path.GetFileName(Path.GetDirectoryName(path)));
                Context.Scene.Load();
            }
        }

        //private void FileOpenScene()
        //{
        //    if (FileDialog.Open(out string path,
        //                            filter:
        //                            "JSON File (*.json)\0*.json\0" +
        //                            "All Files (*.*)\0*.*\0"))
        //    {
        //        Context.Scene?.Unload();
        //
        //        ClearContext();
        //
        //        Context.Scene = SceneSerializer.DeserializeJson(File.ReadAllText(path), Path.GetDirectoryName(path));
        //        Context.Scene.Load();
        //
        //        ScenePath = path;
        //    }
        //}
        //private void FileSaveScene()
        //{
        //    if (ScenePath != null)
        //    {
        //        EditorApplication.Log.Warn("sry not implemented");
        //        FileSaveSceneAs();
        //    }
        //    else
        //    {
        //        FileSaveSceneAs();
        //    }
        //}
        //private void FileSaveSceneAs(bool copy = false)
        //{
        //    if (FileDialog.Save(out string path,
        //                    filter:
        //                    "Scene File Env. INI (*.sfe.ini)\0*.sfe.ini\0" +
        //                    "All Files (*.*)\0*.*\0",
        //                    name: "scene"))
        //    {
        //        string asmListFile = $"./{Path.GetFileNameWithoutExtension(path)}.assemblies.list";
        //        string sceneFile = $"./{Path.GetFileNameWithoutExtension(path)}.scene.json";
        //
        //        if (File.Exists(asmListFile) || File.Exists(sceneFile))
        //        {
        //            PushModal(new ActionModal("File collison detected!\nOverwrite?",
        //                ActionModal.ButtonFlags.OKCancel,
        //                (button) => { if (button == ActionModal.ButtonFlags.OK) FinishSave(); }
        //                ));
        //            return;
        //        }
        //
        //        FinishSave();
        //        void FinishSave()
        //        {
        //            SceneLoader.Save(path, Context.Scene, new SceneFileEnvironment()
        //            {
        //                AssembliesList = asmListFile,
        //                Scene = sceneFile,
        //            });
        //
        //            if (!copy) SavePath = path;
        //        }
        //    }
        //}
        #endregion

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
    }
}
