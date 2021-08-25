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
using CrossEngine.Assets;

using CrossEngineEditor.Utils;

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
            for (int i = -5; i < 6; i++)
            {
                Entity ent = Scene.CreateEntity();
                ent.Transform.WorldPosition = new Vector3(i, 0, 0);
                ent.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.GetTexture("textures/prototype_512x512_grey1.png")) });
                ent.AddComponent(new TagComponent("asd"));
                ent.AddComponent(new RigidBodyComponent() { LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
                ent.AddComponent(new Box2DColliderComponent());
            }

            Entity ground = Scene.CreateEntity();
            ground.Transform.LocalScale = new Vector3(10, 1, 1);
            ground.Transform.LocalPosition = new Vector3(0, -5, 0);
            ground.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.GetTexture("textures/prototype_512x512_grey1.png")) });
            ground.AddComponent(new RigidBodyComponent() { Mass = 0, Static = true, /*LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1)*/ });
            ground.AddComponent(new Box2DColliderComponent());

            //CrossEngine.Serialization.Json.JsonSerializer serializer = new CrossEngine.Serialization.Json.JsonSerializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
            //string json = serializer.Serialize(Scene);
            //Log.App.Debug(json);
            //CrossEngine.Serialization.Json.JsonDeserializer deserializer = new CrossEngine.Serialization.Json.JsonDeserializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
            //Scene = (Scene)deserializer.Deserialize(json, typeof(Scene));

            //Scene.Start();
            // ---
        }

        public override void OnDetach()
        {
            //Scene = null;
            //AssetManager.Textures.Purge();
            //GC.Collect();
            //CrossEngine.Assets.GC.GPUGarbageCollector.Collect();
        }

        private EditorModal _modal;

        bool ph = false;

        public unsafe override void OnRender()
        {
            Renderer.Clear();

            ImGuiLayer.Instance.Begin();

            SetupDockspace();

            // draw modals
            if (_modal != null)
            {
                if (!_modal.Draw())
                    _modal = null;
            }

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Scene"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModal.ModalButtonFlags.OKCancel, (flags) =>
                        {
                            if ((flags & ActionModal.ModalButtonFlags.OK) > 0)
                                EditorLayer.Instance.Scene = new CrossEngine.Scenes.Scene();
                        }));
                    }
                    if (ImGui.MenuItem("Open..."))
                    {
                        PushModal(new OpenSceneModal());
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save as..."))
                    {
                        if (FileDialog.Save(out string path, initialDir: Environment.CurrentDirectory))
                        {
                            CrossEngine.Serialization.Json.JsonSerializer serializer = new CrossEngine.Serialization.Json.JsonSerializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
                            string json = serializer.Serialize(Scene);
                            //Log.App.Debug(json);
                            //System.IO.File.WriteAllText(path, json);
                        }
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

            // debug
            ImGui.Begin("Debug");
            ImGui.Checkbox("Update physics", ref ph);
            if (ph) Scene.OnFixedUpdateRuntime();
            //Vector3 gr = Scene.RigidBodyWorld.Gravity;
            //if (ImGui.DragFloat3("gravity", ref gr)) Scene.RigidBodyWorld.Gravity = gr;
            ImGui.Text("editor camera:");
            ImGui.Text(EditorCamera.Position.ToString("0.00"));
            ImGui.End();


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

        public void PushModal(EditorModal modal)
        {
            if (_modal != null) throw new Exception("Modal already opened!");
            _modal = modal;
        }

        public void PopModal(EditorModal modal)
        {
            _modal = null;
        }
    }
}
