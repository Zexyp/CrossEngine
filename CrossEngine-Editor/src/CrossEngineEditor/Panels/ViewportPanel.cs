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
            ImGuiLayer.Instance.BlockEvents = !Focused;
            if (Focused)
                EditorCameraController.OnUpdate(Time.DeltaTimeF);

            if (Context.Scene != null)
            {
                Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                if (viewportPanelSize != viewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                {
                    viewportSize = viewportPanelSize;

                    framebuffer.Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                    EditorCameraController.SetViewportSize(viewportSize.X, viewportSize.Y);
                }
                ImGui.Image(new IntPtr(framebuffer.ColorAttachments[Context.Scene.Pipeline.FBStructureIndex.Color]),
                    viewportSize,
                    new Vector2(0, 1),
                    new Vector2(1, 0));

                framebuffer.Bind();

                Renderer.Clear();
                framebuffer.ClearAttachment(1, Context.Scene.Pipeline.FBStructureIndex.Color);

                if (Context.Scene != null) Context.Scene.OnRenderEditor(EditorLayer.Instance.EditorCamera, framebuffer);

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && Focused && EnableSelect)
                {
                    Vector2 texpos = ImGui.GetMousePos() - new Vector2(WindowContentAreaMin.X, WindowContentAreaMax.Y);
                    texpos.Y = -texpos.Y;
                    int result = framebuffer.ReadPixel(1, (int)texpos.X, (int)texpos.Y);
                    EditorApplication.Log.Trace($"selected entity id {result}");

                    Context.ActiveEntity = Context.Scene.GetEntity(result);
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
