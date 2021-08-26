using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Logging;

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

        }

        public override void OnAttach()
        {
            EditorCameraController = new OrthographicEditorCameraController(EditorLayer.Instance.EditorCamera);
            EditorCameraController.SetViewportSize(Application.Instance.Width, Application.Instance.Height);

            var spec = new FramebufferSpecification();
            spec.Attachments = new FramebufferAttachmentSpecification(
                new FramebufferTextureSpecification(FramebufferTextureFormat.ColorRGBA8),
                new FramebufferTextureSpecification(FramebufferTextureFormat.Depth24Stencil8),
                new FramebufferTextureSpecification(FramebufferTextureFormat.ColorRedInteger)
                );
            spec.Width = 1;
            spec.Height = 1;
            framebuffer = new Framebuffer(spec);
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

            Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
            if (viewportPanelSize != viewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
            {
                viewportSize = viewportPanelSize;
                
                framebuffer.Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                EditorCameraController.SetViewportSize(viewportSize.X, viewportSize.Y);
            }
            ImGui.Image(new IntPtr(framebuffer.ColorAttachments[0]), viewportSize, new Vector2(0, 1), new Vector2(1, 0));

            framebuffer.Bind();

            Renderer.Clear();
            framebuffer.ClearAttachment(1, 0);

            EditorLayer.Instance.Scene.OnRenderEditor(EditorLayer.Instance.EditorCamera);
            
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && Focused && EnableSelect)
            {
                Vector2 texpos = ImGui.GetMousePos() - new Vector2(ContentMin.X, ContentMax.Y);
                texpos.Y = -texpos.Y;
                int result = framebuffer.ReadPixel(1, (int)texpos.X, (int)texpos.Y);
                Log.App.Trace($"selected entity id {result}");

                EditorLayer.Instance.SelectedEntity = EditorLayer.Instance.Scene.GetEntity(result);
            }

            Framebuffer.Unbind();
        }

        public override void OnEvent(Event e)
        {
            EditorCameraController.OnEvent(e);
        }
    }
}
