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
using CrossEngine.Rendering.Passes;
using CrossEngine.Inputs;

namespace CrossEngineEditor.Panels
{
    class GamePanel : EditorPanel
    {
        Vector2 viewportSize;
        RenderPipeline pipeline;

        public GamePanel() : base("Game")
        {
            
        }

        public override void OnAttach()
        {
        }

        public override void OnDetach()
        {
        }

        public override void OnOpen()
        {
            pipeline = new RenderPipeline();
            pipeline.RegisterPass(new Renderer2DPass());
            pipeline.RegisterPass(new LineRenderPass());
        }

        public override void OnClose()
        {
            pipeline.Dispose();
            pipeline = null;
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

                        pipeline.Framebuffer.Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                        // notify scene
                        if (Context.Scene.Running) Context.Scene.OnEvent(new WindowResizeEvent((uint)viewportSize.X, (uint)viewportSize.Y));

                        var camcomp = Context.Scene.GetPrimaryCameraComponent();
                        if (camcomp != null && camcomp.Usable && !camcomp.FixedAspectRatio)
                        {
                            camcomp.Resize(viewportSize.X, viewportSize.Y);
                        }
                    }
                }

                // draw the framebuffer as image
                ImGui.Image(new IntPtr(pipeline.Framebuffer.ColorAttachments[pipeline.FBStructureIndex.Color]),
                    viewportSize,
                    new Vector2(0, 1),
                    new Vector2(1, 0));

                pipeline.Framebuffer.Bind();
                Renderer.Clear();
                pipeline.Framebuffer.ClearAttachment((uint)pipeline.FBStructureIndex.ID, 0);

                // draw
                Context.Scene.Pipeline = pipeline;
                Context.Scene.OnRenderRuntime();

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

                // !
                // TODO: this seems kinda sus
                if (Context.Scene.Running && Focused)
                {
                    ImGuiLayer.Instance.BlockEvents = false;
                    Input.Enabled = true;
                }
                else Input.Enabled = false;
            }
        }

        public override void OnEvent(Event e)
        {
            if (Context.Scene?.Running == true)
            {
                if (Focused)
                {
                    Context.Scene.OnEvent(e);
                }
            }
        }
    }
}
