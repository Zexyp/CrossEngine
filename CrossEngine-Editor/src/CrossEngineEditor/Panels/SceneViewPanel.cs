using ImGuiNET;
using System;
using System.Numerics;

using CrossEngine;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Events;

namespace CrossEngineEditor.Panels
{
    abstract class SceneViewPanel : EditorPanel
    {
        protected ICamera DrawCamera;

        protected Vector2 ViewportSize { get; private set; }
        protected Ref<Framebuffer> Framebuffer { get; private set; }
        protected bool Drawing = true;

        public SceneViewPanel()
        {
            
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
                Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                if (viewportPanelSize != ViewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                {
                    ViewportSize = viewportPanelSize;

                    if (Ref.IsNull(Framebuffer))
                        ThreadManager.ExecuteOnRenderThread(() => ((Framebuffer)Framebuffer).Resize((uint)ViewportSize.X, (uint)ViewportSize.Y));
                    else
                        ((Framebuffer)Framebuffer).Resize((uint)ViewportSize.X, (uint)ViewportSize.Y);

                    Resized();
                }
            }

            // needs to be set back so SceneManager can render only from given scene data
            var lastSceneOutput = Context.Scene.RenderData.Output;

            // draw the framebuffer as image
            Context.Scene.RenderData.Output = Framebuffer;
            if (Drawing)
                SceneRenderer.DrawScene(Context.Scene, DrawCamera);
            ImGui.Image(new IntPtr(((Framebuffer)Framebuffer)?.GetColorAttachmentRendererID(0) ?? 0),
                ViewportSize,
                new Vector2(0, 1),
                new Vector2(1, 0));

            Context.Scene.RenderData.Output = lastSceneOutput;
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

            ThreadManager.ExecuteOnRenderThread(() =>
            {
                Framebuffer = CrossEngine.Rendering.Buffers.Framebuffer.Create(ref spec);
            });
        }

        public override void OnClose()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                Framebuffer.Value.Dispose();
                Framebuffer = null;
            });
        }

        protected abstract void Resized();
    }
}
