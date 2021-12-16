using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Scenes;
using CrossEngine.Utils;

namespace CrossEngineEditor.Panels
{
    class GamePanel : EditorPanel
    {
        Framebuffer framebuffer;
        Vector2 viewportSize;

        public GamePanel() : base("Game")
        {
            
        }

        public override void OnAttach()
        {
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
                    framebuffer = null;
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
            if (Context.Scene != null)
            {
                // resize check
                {
                    Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                    if (viewportPanelSize != viewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                    {
                        viewportSize = viewportPanelSize;

                        framebuffer?.Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                        // notify scene
                        if (Context.Scene.Running) Context.Scene.OnEvent(new WindowResizeEvent((uint)viewportSize.X, (uint)viewportSize.Y));

                        Context.Scene.GetPrimaryCamera()?.Resize(viewportSize.X, viewportSize.Y);
                    }
                }

                // draw the framebuffer as image
                ImGui.Image(new IntPtr(framebuffer.ColorAttachments[Context.Scene.Pipeline.FBStructureIndex.Color]),
                    viewportSize,
                    new Vector2(0, 1),
                    new Vector2(1, 0));

                framebuffer.Bind();
                Renderer.Clear();
                framebuffer.ClearAttachment((uint)Context.Scene.Pipeline.FBStructureIndex.ID, 0);

                // draw
                Context.Scene.OnRenderRuntime(framebuffer);

                if (Context.Scene.GetPrimaryCamera() == null)
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
    }
}
