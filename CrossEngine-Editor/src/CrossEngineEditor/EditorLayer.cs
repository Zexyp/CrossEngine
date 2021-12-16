using ImGuiNET;
using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.IO;
using System.Reflection;
using System.Linq;

using CrossEngine;
using CrossEngine.Entities;
using CrossEngine.Entities.Components;
using CrossEngine.Events;
using CrossEngine.Layers;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Assets;
using CrossEngine.Physics;
using CrossEngine.Serialization;
using CrossEngine.Profiling;
using CrossEngine.Assemblies;

using CrossEngineEditor.Utils;
using CrossEngineEditor.Panels;
using CrossEngineEditor.Modals;

namespace CrossEngineEditor
{
    public class EditorLayer : Layer
    {
        static public EditorLayer Instance;

        public EditorCamera EditorCamera = new EditorCamera();
        public readonly EditorContext Context = new EditorContext();

        private List<EditorPanel> _panels = new List<EditorPanel>();
        private EditorModal _modal = null;


        public Type[] CoreComponentTypes = Assembly.GetAssembly(typeof(Component)).ExportedTypes.Where(type => type.IsSubclassOf(typeof(Component)) && !type.IsAbstract).ToArray();
        
        private readonly List<Type> _componentTypeRegistry = new List<Type>();
        public readonly ReadOnlyCollection<Type> ComponentTypeRegistry;

        //Texture dockspaceIconTexture;

        public bool SceneUpdate = false;

        private Scene workingScene = null;

        public EditorLayer()
        {
            if (Instance != null)
                Log.App.Warn("there should be only one editor layer");

            Instance = this;

            ComponentTypeRegistry = _componentTypeRegistry.AsReadOnly();

            AddPanel(new InspectorPanel());
            AddPanel(new SceneHierarchyPanel());
            AddPanel(new ViewportPanel());
            AddPanel(new GizmoPanel());
            AddPanel(new LagometerPanel());
            AddPanel(new ImageViewerPanel());
            AddPanel(new GamePanel());
        }

        public override void OnAttach()
        {
            //dockspaceIconTexture = new Rendering.Textures.Texture(Properties.Resources.DefaultWindowIcon.ToBitmap());
            //dockspaceIconTexture.SetFilterParameter(Rendering.Textures.FilterParameter.Nearest);



            // --- test code
            Context.Scene = new Scene();
            for (int i = -5; i <= 5; i++)
            {
                if (i == 0)
                {
                    Entity okl = Context.Scene.CreateEntity();
                    okl.Transform.WorldPosition = new Vector3(i, 0, 0);
                    okl.AddComponent(new TagComponent("Main"));
                    okl.AddComponent(new SpriteRendererComponent() { Color = new Vector4(0, 1, 0, 1)});
                    okl.AddComponent(new CameraComponent(new OrthographicCamera() { OrthographicSize = 10 }));
                    continue;
                }
                Entity ent = Context.Scene.CreateEntity();
                ent.Transform.WorldPosition = new Vector3(i, 0, 0);
                ent.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), /*Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.LoadTexture("textures/prototype_512x512_grey1.png"))*/ });
                ent.AddComponent(new TagComponent("asd" + i));
                ent.AddComponent(new RigidBodyComponent() { LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
                ent.AddComponent(new Box2DColliderComponent());
            }
            
            Entity ground = Context.Scene.CreateEntity();
            ground.Transform.Scale = new Vector3(10, 1, 1);
            ground.Transform.Position = new Vector3(0, -5, 0);
            ground.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), /*Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.LoadTexture("textures/prototype_512x512_grey1.png"))*/ });
            ground.AddComponent(new RigidBodyComponent() { Mass = 0, Static = true, LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
            ground.AddComponent(new Box2DColliderComponent());
            //ground.AddComponent(new ExcComponent());

            //CrossEngine.Serialization.Json.JsonSerializer serializer = new CrossEngine.Serialization.Json.JsonSerializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
            //string json = serializer.Serialize(Scene);
            //Log.App.Debug(json);
            //CrossEngine.Serialization.Json.JsonDeserializer deserializer = new CrossEngine.Serialization.Json.JsonDeserializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
            //Scene = (Scene)deserializer.Deserialize(json, typeof(Scene));

            // ---
        }

        public override void OnDetach()
        {
            //Scene = null;
            //AssetManager.Textures.Purge();
            //GC.Collect();
            //CrossEngine.Assets.GC.GPUGarbageCollector.Collect();
        }

        // test pcode
        int sleep = 0;

        public unsafe override void OnRender()
        {
            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.OnRender)}");

            Renderer.Clear();

            ImGuiLayer.Instance.Begin();

            SetupDockspace();

            DrawModals();

            DrawMenuBar();

            ImGui.ShowDemoWindow(); // purely dev thing

            // debug
            ImGui.Begin("Debug");
            if (Context.Scene != null)
            {
                Vector3 gr = (Context.Scene.RigidBodyWorld != null) ? Context.Scene.RigidBodyWorld.Gravity : default;
                if (ImGui.DragFloat3("gravity", ref gr)) Context.Scene.RigidBodyWorld.Gravity = gr;
                ImGui.Text("editor camera pos:");
                ImGui.Text(EditorCamera.Position.ToString("0.00"));
                ImGui.SliderInt("sleep", ref sleep, 0, 1000);
                if (sleep > 0) System.Threading.Thread.Sleep(sleep);

                // test seri
                if (ImGui.Button("seri test"))
                {
                    string json;
                    json = SceneSerializer.SertializeJson(Context.Scene);
                    Log.App.Debug(json);

                    Context.Scene.Unload();
                    ClearContext();
                    Context.Scene = SceneSerializer.DeserializeJson(json);
                    Context.Scene.Load();
                }
            }
            ImGui.End();


            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.DrawPanels)}");
            DrawPanels();
            Profiler.EndScope();

            EndDockspace();

            ImGuiLayer.Instance.End();

            Profiler.EndScope();
        }

        public override void OnUpdate(float timestep)
        {
            if (Context.Scene?.Running == true && SceneUpdate) Context.Scene.OnUpdateRuntime(Time.DeltaTimeF);
        }

        public override void OnEvent(Event e)
        {
            Profiler.BeginScope($"{nameof(EditorLayer)}.{nameof(EditorLayer.OnEvent)}");

            for (int i = _panels.Count - 1; i >= 0; i--)
            {
                _panels[i].OnEvent(e);
            }

            Profiler.EndScope();
        }

        #region Dockspace
        private unsafe void SetupDockspace()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(Application.Instance.Width, Application.Instance.Height));

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

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                #region File Menu
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Scene"))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModalButtonFlags.OKCancel, (flags) =>
                        {
                            if (flags == ActionModalButtonFlags.OK) FileNewScene();
                        }));
                    }

                    if (ImGui.MenuItem("Open Scene..."))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModalButtonFlags.OKCancel, (flags) =>
                        {
                            if (flags == ActionModalButtonFlags.OK) FileOpenScene();
                        }));
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save Scene", Context.Scene != null))
                    {
                        FileSaveScene();
                    }

                    if (ImGui.MenuItem("Save Scene As...", Context.Scene != null))
                    {
                        FileSaveSceneAs();
                    }

                    if (ImGui.MenuItem("Save Scene Copy...", Context.Scene != null))
                    {
                        FileSaveSceneAs();
                    }

                    ImGui.EndMenu();
                }
                #endregion

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
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }

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
                            throw new NotImplementedException();
                            ResetComponentTypeRegistry();
                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Reload"))
                        {
                            throw new NotImplementedException();
                            ResetComponentTypeRegistry();
                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Save List Copy..."))
                        {
                            throw new NotImplementedException();
                        }
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

                ImGui.EndMenuBar();
            }
        }

        #region Panel Methods
        private void AddPanel(EditorPanel panel)
        {
            _panels.Add(panel);
            panel.Context = Context;

            panel.Attached = true;
            panel.OnAttach();

            panel.OnOpen();
        }

        private void RemovePanel(EditorPanel panel)
        {
            panel.OnClose();

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
            if (_modal != null)
            {
                if (!_modal.Draw())
                    _modal = null;
            }
        }

        public void PushModal(EditorModal modal)
        {
            if (_modal != null) throw new Exception("Modal already opened!");
            _modal = modal;
        }

        public void PopModal(EditorModal modal)
        {
            _modal = null;
        }
        #endregion

        #region Context
        public void ClearContext()
        {
            Context.SelectedEntities.Clear();
            Context.ActiveEntity = null;
        }
        #endregion

        #region File Menu Actions
        private string _savePath = null;
        private string SavePath
        {
            set
            {
                _savePath = value;

                if (_savePath == null) Application.Instance.Title = "CrossEngine Editor";
                else Application.Instance.Title = $"CrossEngine Editor [{_savePath}]";
            }
        }
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
                                    "JSON (*.json)\0*.json\0" +
                                    "All Files (*.*)\0*.*\0"))
            {
                Context.Scene?.Unload();
                Context.Scene?.Destroy();

                ClearContext();

                Context.Scene = SceneSerializer.DeserializeJson(File.ReadAllText(path));
                Context.Scene.Load();

                SavePath = path;
            }
        }
        private void FileSaveScene()
        {
            if (_savePath != null)
            {
                string json;
                json = SceneSerializer.SertializeJson(Context.Scene);

                string backupPath = _savePath + "1";

                if (File.Exists(_savePath))
                    File.Move(_savePath, backupPath, true);

                File.WriteAllText(_savePath, json);
            }
            else
            {
                FileSaveSceneAs();
            }
        }
        private void FileSaveSceneAs()
        {
            if (FileDialog.Save(out string path,
                            filter:
                            "JSON (*.json)\0*.json\0" +
                            "All Files (*.*)\0*.*\0",
                            name: "scene"))
            {
                string json;
                json = SceneSerializer.SertializeJson(Context.Scene);

                File.WriteAllText(path, json);

                SavePath = path;
            }
        }
        private void FileSaveCopy()
        {
            if (FileDialog.Save(out string path,
                            filter:
                            "JSON (*.json)\0*.json\0" +
                            "All Files (*.*)\0*.*\0",
                            name: "scene"))
            {
                string json;
                json = SceneSerializer.SertializeJson(Context.Scene);

                File.WriteAllText(path, json);
            }
        }
        #endregion

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

        private void ResetComponentTypeRegistry()
        {
            _componentTypeRegistry.Clear();
            _componentTypeRegistry.AddRange(AssemblyLoader.GetSubTypesOf(typeof(Component)));
        }
        #endregion

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
    }
}
