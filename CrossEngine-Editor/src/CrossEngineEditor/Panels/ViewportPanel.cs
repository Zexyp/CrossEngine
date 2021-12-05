using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Scenes;
using CrossEngine.Utils;

namespace CrossEngineEditor.Panels
{
    class ViewportPanel : EditorPanel
    {
        Framebuffer framebuffer;
        Vector2 viewportSize;

        public OrthographicEditorCameraController EditorCameraController;

        public bool EnableSelect = true;

        public ViewportPanel() : base("Viewport")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;
        }

        public override void OnAttach()
        {
            EditorCameraController = new OrthographicEditorCameraController(EditorLayer.Instance.EditorCamera);
            EditorCameraController.SetViewportSize(Application.Instance.Width, Application.Instance.Height);

            Context.OnSceneChanged += OnContextSceneChanged;
        }

        public override void OnDetach()
        {
            Context.OnSceneChanged -= OnContextSceneChanged;
        }

        private void OnContextSceneChanged()
        {
            var scene = Context.Scene;
            if (scene != null)
            {
                if (framebuffer != null)
                {
                    framebuffer.Dispose();
                }

                var spec = new FramebufferSpecification();
                spec.Attachments = new FramebufferAttachmentSpecification(
                    // using floating point colors
                    new FramebufferTextureSpecification(TextureFormat.ColorRGBA32F) { Index = scene.Pipeline.FBStructureIndex.Color },
                    new FramebufferTextureSpecification(TextureFormat.ColorR32I) { Index = scene.Pipeline.FBStructureIndex.ID },
                    new FramebufferTextureSpecification(TextureFormat.Depth24Stencil8)
                    );
                spec.Width = Math.Max((uint)viewportSize.X, 1);
                spec.Height = Math.Max((uint)viewportSize.Y, 1);
                framebuffer = new Framebuffer(spec);
            }
            else
            {
                framebuffer.Dispose();
                framebuffer = null;
            }
        }

        protected override void PrepareWindow()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        protected override void EndPrepareWindow()
        {
            ImGui.PopStyleVar();
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                // view mode selection
                if (ImGui.BeginTabBar("##viewmode"))
                {
                    unsafe
                    {
                        if (ImGuiExtension.BeginTabItemNullableOpen("Edit", null, ImGuiTabItemFlags.SetSelected))
                        {
                            ImGui.EndTabItem();
                        }
                        if (ImGuiExtension.BeginTabItemNullableOpen("Game", null, ImGuiTabItemFlags.SetSelected))
                        {
                            ImGui.EndTabItem();
                        }
                        ImGui.EndTabBar();
                    }
                }

                ImGui.SameLine();

                // play mode button
                Vector2 cp = ImGui.GetCursorPos();
                cp.X += ImGui.GetColumnWidth() / 2;
                ImGui.SetCursorPos(cp);
                bool colorPushed = Context.Scene.Running;
                if (colorPushed) ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000ff/*new Vector4(1, 0.2f, 0.1f, 1)*/);
                if (ImGui.ArrowButton("##play", Context.Scene.Running ? ImGuiDir.Down : ImGuiDir.Right))
                {
                    if (!Context.Scene.Running) EditorLayer.Instance.StartPlaymode();
                    else EditorLayer.Instance.EndPlaymode();
                }
                if (colorPushed) ImGui.PopStyleColor();


                ImGui.EndMenuBar();
            }

            // !
            // TODO: this seems kinda sus
            ImGuiLayer.Instance.BlockEvents = !Focused;

            if (Context.Scene != null)
            {
                // resize check
                {
                    Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                    if (viewportPanelSize != viewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                    {
                        viewportSize = viewportPanelSize;

                        framebuffer.Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                        EditorCameraController.SetViewportSize(viewportSize.X, viewportSize.Y);

                        // notify scene
                        if (Context.Scene.Running) Context.Scene.OnEvent(new WindowResizeEvent((uint)viewportSize.X, (uint)viewportSize.Y));
                    }
                }

                // draw the framebuffer as 
                ImGui.Image(new IntPtr(framebuffer.ColorAttachments[Context.Scene.Pipeline.FBStructureIndex.Color]),
                    viewportSize,
                    new Vector2(0, 1),
                    new Vector2(1, 0));

                framebuffer.Bind();
                Renderer.Clear();
                framebuffer.ClearAttachment((uint)Context.Scene.Pipeline.FBStructureIndex.ID, 0);

                // draw
                if (Context.Scene != null)
                {
                    if (Context.Scene.Running) Context.Scene.OnRenderRuntime(framebuffer);
                    else Context.Scene.OnRenderEditor(EditorLayer.Instance.EditorCamera, framebuffer);
                }

                // interaction
                if (ImGui.IsItemHovered() && Focused)
                {
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && EnableSelect)
                    {
                        Vector2 texpos = ImGui.GetMousePos() - new Vector2(WindowContentAreaMin.X, WindowContentAreaMax.Y);
                        texpos.Y = -texpos.Y;
                        int result = framebuffer.ReadPixel(1, (int)texpos.X, (int)texpos.Y);
                        EditorApplication.Log.Trace($"selected entity id {result}");

                        Context.ActiveEntity = Context.Scene.GetEntity(result);
                    }
                    
                    EditorCameraController.OnUpdate(Time.DeltaTimeF);
                }

                // check camera source
                if (Context.Scene?.Running == true && Context.Scene?.GetPrimaryCameraEntity() == null)
                {
                    //Vector2 cursorBackup = ImGui.GetCursorPos();
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                    const string noCameraMessage = "No Primary Camera!";
                    Vector2 buttSize = ImGui.CalcTextSize(noCameraMessage) + ImGui.GetStyle().FramePadding * 2;
                    ImGui.SetCursorPos(WindowSize / 2 - buttSize / 2);
                    ImGui.Button(noCameraMessage, buttSize);
                    ImGui.PopStyleColor();
                    //ImGui.SetCursorPos(cursorBackup);
                }

                Framebuffer.Unbind();
            }
        }

        public override void OnEvent(Event e)
        {
            EditorCameraController.OnEvent(e);
        }
    }
}
