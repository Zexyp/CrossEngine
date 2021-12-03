using ImGuiNET;
using System;

using System.Collections.Generic;
using System.Numerics;
using System.IO;

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
            AddPanel(new LagometerPanel());
            AddPanel(new ImageViewerPanel());
        }

        public override void OnAttach()
        {
            //dockspaceIconTexture = new Rendering.Textures.Texture(Properties.Resources.DefaultWindowIcon.ToBitmap());
            //dockspaceIconTexture.SetFilterParameter(Rendering.Textures.FilterParameter.Nearest);



            // --- test code
            Context.Scene = new Scene();
            for (int i = -5; i <= 5; i++)
            {
                Entity ent = Context.Scene.CreateEntity();
                ent.Transform.WorldPosition = new Vector3(i, 0, 0);
                ent.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), /*Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.LoadTexture("textures/prototype_512x512_grey1.png"))*/ });
                ent.AddComponent(new TagComponent("asd"));
                ent.AddComponent(new RigidBodyComponent() { LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
                ent.AddComponent(new Box2DColliderComponent());
            }
            
            Entity ground = Context.Scene.CreateEntity();
            ground.Transform.Scale = new Vector3(10, 1, 1);
            ground.Transform.Position = new Vector3(0, -5, 0);
            ground.AddComponent(new SpriteRendererComponent() { Color = new Vector4(1, 1, 1, 1), /*Sprite = new CrossEngine.Rendering.Sprites.Sprite(AssetManager.Textures.LoadTexture("textures/prototype_512x512_grey1.png"))*/ });
            ground.AddComponent(new RigidBodyComponent() { Mass = 0, Static = true, LinearFactor = new Vector3(1, 1, 0), AngularFactor = new Vector3(0, 0, 1) });
            ground.AddComponent(new Box2DColliderComponent());
            ground.AddComponent(new TestCompo());
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
        bool su = false;
        int sleep = 0;

        public unsafe override void OnRender()
        {
            Renderer.Clear();

            ImGuiLayer.Instance.Begin();

            SetupDockspace();

            DrawModals();
            

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New scene"))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModal.ModalButtonFlags.OKCancel, (flags) =>
                        {
                            if (flags == ActionModal.ModalButtonFlags.OK)
                            {
                                Context.Scene?.Unload();

                                ClearContext();

                                Context.Scene = new Scene();
                            }
                        }));
                    }

                    if (ImGui.MenuItem("Open Scene..."))
                    {
                        PushModal(new ActionModal("All those beautiful changes will be lost.\nThis operation cannot be undone!\n", ActionModal.ModalButtonFlags.OKCancel, (flags) =>
                        {
                            if (flags == ActionModal.ModalButtonFlags.OK)
                            {
                                if (FileDialog.Open(out string path, initialDir: Environment.CurrentDirectory))
                                {
                                    Context.Scene?.Unload();

                                    ClearContext();

                                    Context.Scene = SceneSerializer.DeserializeJson(File.ReadAllText(path));
                                    Context.Scene.Load();
                                }
                            }
                        }));
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save Scene As...", Context.Scene != null))
                    {
                        if (FileDialog.Save(out string path, initialDir: Environment.CurrentDirectory))
                        {
                            string json;
                            json = SceneSerializer.SertializeJson(Context.Scene);
                            Log.App.Debug(json);

                            Context.Scene.Unload();
                            ClearContext();
                            Context.Scene = SceneSerializer.DeserializeJson(json);
                            Context.Scene.Load();
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
            if (Context.Scene != null)
            {
                if (ImGui.ArrowButton("start", Context.Scene.Running ? ImGuiDir.Left : ImGuiDir.Right))
                {
                    if (!Context.Scene.Running)
                    {
                        Context.Scene.Load();
                        Context.Scene.Start();
                    }
                    else
                    {
                        Context.Scene.End();
                        Context.Scene.Unload();
                    }
                }
                ImGui.Checkbox("Update physics", ref su);
                if (su) Context.Scene.OnUpdateRuntime(Time.DeltaTimeF);
                Vector3 gr = (Context.Scene.RigidBodyWorld != null) ? Context.Scene.RigidBodyWorld.Gravity : default;
                if (ImGui.DragFloat3("gravity", ref gr)) Context.Scene.RigidBodyWorld.Gravity = gr;
                ImGui.Text("editor camera:");
                ImGui.Text(EditorCamera.Position.ToString("0.00"));
                ImGui.SliderInt("sleep", ref sleep, 0, 1000);
                if (sleep > 0) System.Threading.Thread.Sleep(sleep);
            }
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
            Context.Scene = null;
        }
        #endregion
    }

    class TestCompo : ScriptableComponent
    {
        const int NumRays = 360;
        Ray[] _rays = new Ray[NumRays];

        struct Ray
        {
            public Vector3 Source;
            public Vector3 Destination;
            public Vector3 HitPoint;
            public Vector3 Normal;

            public void MoveX(float move)
            {
                Source.X += move;
                Destination.X += move;
            }
        }

        public override void OnAttach()
        {
            for (int i = 0; i < _rays.Length; i++)
            {
                Ray ray = new Ray();
                ray.Destination = Vector3.Transform(Vector3.UnitY, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, ((float)i / NumRays) * MathF.PI * 2)) * 10;
                _rays[i] = ray;
            }
        }

        public override void OnUpdate(float timestep)
        {
            for (int i = 0; i < _rays.Length; i++)
            {
                Ray ray = _rays[i];

                Physics.Raycast(_rays[i].Source, _rays[i].Destination, out RaycastHitInfo info);
                ray.HitPoint = info.point;

                _rays[i] = ray;
            }
        }

        public override void OnRender(RenderEvent re)
        {
            if (re is LineRenderEvent)
            {
                for (int i = 0; i < _rays.Length; i++)
                {
                    CrossEngine.Rendering.Lines.LineRenderer.DrawLine(_rays[i].Source, _rays[i].HitPoint, new Vector4(1, 0, 0, 1));
                }
            }
        }
    }

    class ExcComponent : ScriptableComponent
    {
        protected override void OnEnable()
        {
            throw new Exception();
        }
        protected override void OnDisable()
        {
            throw new Exception();
        }

        public override void OnUpdate(float timestep)
        {
            throw new Exception();
        }
        public override void OnRender(RenderEvent re)
        {
            throw new Exception();
        }
        public override void OnEvent(Event e)
        {
            throw new Exception();
        }

        public override void OnAttach()
        {
            throw new Exception();
        }
        public override void OnDetach()
        {
            throw new Exception();
        }

        public override void OnStart()
        {
            throw new Exception();
        }
        public override void OnEnd()
        {
            throw new Exception();
        }

        public override void OnSerialize(SerializationInfo info)
        {
            throw new Exception();
        }
        public override void OnDeserialize(SerializationInfo info)
        {
            throw new Exception();
        }
    }
}
