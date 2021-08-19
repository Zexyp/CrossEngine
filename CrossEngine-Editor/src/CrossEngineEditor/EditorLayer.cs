using ImGuiNET;
using System;

using System.Collections.Generic;
using System.Numerics;

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

namespace CrossEngineEditor
{
    public class EditorLayer : Layer
    {
        static public EditorLayer Instance;

        public Entity SelectedEntity = null;
        public Scene Scene;
        private List<EditorPanel> _panels = new List<EditorPanel>();

        public EditorCamera EditorCamera = new EditorCamera();

        //Texture dockspaceIconTexture;

        public EditorLayer()
        {
            if (Instance != null)
                Log.App.Warn("there should be only one editor layer");

            Instance = this;

            AddPanel(new InspectorPanel());
            AddPanel(new SceneHierarchyPanel());
            AddPanel(new ViewportPanel());
            AddPanel(new GizmoPanel());
        }

        public override void OnAttach()
        {
            Scene = new Scene();

            //dockspaceIconTexture = new Rendering.Textures.Texture(Properties.Resources.DefaultWindowIcon.ToBitmap());
            //dockspaceIconTexture.SetFilterParameter(Rendering.Textures.FilterParameter.Nearest);



            // --- test code
            Entity ent = Scene.CreateEntity();
            ent.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.GetTexture("textures/prototype_512x512_grey1.png")) });
            ent.AddComponent(new TagComponent("asd"));
            ent.AddComponent(new RigidBodyComponent());
            ent.AddComponent(new Box2DColliderComponent());
            // ---

            Scene.Start();
        }

        List<EditorModal> modals = new List<EditorModal>();

        public unsafe override void OnRender()
        {
            Renderer.Clear();

            ImGuiLayer.Instance.Begin();

            SetupDockspace();

            for (int i = 0; i < modals.Count; i++)
            {
                if (!modals[i].Draw())
                    modals.RemoveAt(i);
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Scene"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        modals.Add(new NewSceneModal());
                    }
                    if (ImGui.MenuItem("Open..."))
                    {
                        //modals.Add(new OpenSceneModal());
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save as..."))
                    {
                        if (Scene == null)
                        {
                        }
                        CrossEngine.Serialization.Json.JsonSerializer serializer = new CrossEngine.Serialization.Json.JsonSerializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
                        string json = serializer.Serialize(Scene);
                        Log.App.Debug(json);
                        CrossEngine.Serialization.Json.JsonDeserializer deserializer = new CrossEngine.Serialization.Json.JsonDeserializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
                        Scene = (Scene)deserializer.Deserialize(json, typeof(Scene));
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
                                if (ImGui.MenuItem(_panels[i].Name, null, (bool)_panels[i].Open))
                                {
                                    _panels[i].Open = !(bool)_panels[i].Open;
                                }
                            }
                            else
                            {
                                ImGui.MenuItem(_panels[i].Name);
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            ImGui.ShowDemoWindow(); // purely dev thing

            ImGui.Begin("Debug");
            if (ImGui.Button("Update physics"))
                Scene.OnFixedUpdateRuntime();
            ImGui.End();

            //ImGui.Text("editor camera:");
            //ImGui.Text(editorCamera.GetPosition().ToString());
            //ImGui.Text(editorCamera.GetForwardDirection().ToString());

            for (int i = 0; i < _panels.Count; i++)
            {
                _panels[i].Draw();
            }

            EndDockspace();

            ImGuiLayer.Instance.End();
        }

        public override void OnEvent(Event e)
        {
            for (int i = _panels.Count - 1; i >= 0; i--)
            {
                _panels[i].OnEvent(e);
            }
        }

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

        private void AddPanel(EditorPanel panel)
        {
            _panels.Add(panel);
            panel.OnAttach();
        }

        private void RemovePanel(EditorPanel panel)
        {
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
    }
}
