using ImGuiNET;
using System;
using System.Numerics;

using CrossEngine;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using CrossEngine.Scenes;
using CrossEngine.Services;

namespace CrossEngineEditor.Panels
{
    class SceneViewPanel : EditorPanel
    {
        protected virtual ICamera DrawCamera { get; }
        protected virtual Scene Scene { get => Context.Scene; }

        protected Vector2 ViewportSize { get; private set; }
        protected WeakReference<Framebuffer> Framebuffer { get; private set; }
        protected bool ViewportResized;
        protected bool Drawing = true;
        private RenderService rs;

        public SceneViewPanel(RenderService rs)
        {
            this.rs = rs;
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
            // resize check
            {
                ViewportResized = false;
                Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                if (viewportPanelSize != ViewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                {
                    ViewportSize = viewportPanelSize;

                    var fb = Framebuffer.GetValue();
                    fb.Resize((uint)ViewportSize.X, (uint)ViewportSize.Y);

                    OnCameraResize();

                    ViewportResized = true;
                }
            }

            if (Scene == null)
                return;

            // needs to be set back so SceneManager can render only from given scene data
            // as of latest rewrite this is not valid
            var lastSceneOutput = Scene.RenderData.Output;

            // draw the framebuffer as image
            Scene.RenderData.Output = Framebuffer;
            if (Drawing)
            {
                SceneRenderer.DrawScene(Scene.RenderData, rs.RendererApi, DrawCamera);
            }
            ImGui.Image(new IntPtr(Framebuffer.GetValue()?.GetColorAttachmentRendererID(0) ?? 0),
                ViewportSize,
                new Vector2(0, 1),
                new Vector2(1, 0));

            Scene.RenderData.Output = lastSceneOutput;
        }

        public override void OnOpen()
        {
            var spec = new FramebufferSpecification();
            spec.Attachments = new FramebufferAttachmentSpecification(
                // using floating point colors
                new FramebufferTextureSpecification(TextureFormat.ColorRGBA32F),
                new FramebufferTextureSpecification(TextureFormat.ColorR32I),
                new FramebufferTextureSpecification(TextureFormat.Depth24Stencil8)
                );
            spec.Width = 1;
            spec.Height = 1;

            ViewportSize = -Vector2.One;

            rs.Execute(() =>
            {
                Framebuffer = CrossEngine.Rendering.Buffers.Framebuffer.Create(ref spec);
            });
        }

        public override void OnClose()
        {
            rs.Execute(() =>
            {
                Framebuffer.Dispose();
                Framebuffer = null;
            });
        }

        protected virtual void OnCameraResize()
        {
            Scene?.RenderData.PerformResize(ViewportSize.X, ViewportSize.Y);
        }
    }
}