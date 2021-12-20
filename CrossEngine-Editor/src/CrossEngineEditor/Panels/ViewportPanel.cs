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

namespace CrossEngineEditor.Panels
{
    class ViewportPanel : EditorPanel
    {
        Vector2 viewportSize;
        RenderPipeline pipeline;

        public OrthographicEditorCameraController EditorCameraController;

        public bool EnableSelect = true;

        public ViewportPanel() : base("Viewport")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;
        }

        public override void OnAttach()
        {
            EditorCameraController = new OrthographicEditorCameraController(EditorLayer.Instance.EditorCamera);
            new CrossEngine.Rendering.Textures.Texture(0x00000000);
        }

        public override void OnDetach()
        {
        }

        public override void OnOpen()
        {
            pipeline = new RenderPipeline();
            pipeline.RegisterPass(new Renderer2DPass());
            pipeline.RegisterPass(new LineRenderPass());
            pipeline.RegisterPass(new EditorDrawPass());
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
            if (ImGui.BeginMenuBar())
            {
                ImGui.EndMenuBar();
            }

            if (Context.Scene != null)
            {
                // resize check
                {
                    Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                    if (viewportPanelSize != viewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                    {
                        viewportSize = viewportPanelSize;

                        pipeline.Framebuffer.Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                        EditorCameraController.Resize(viewportSize);
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

                // interaction
                if (ImGui.IsItemHovered() && Focused)
                {
                    // camera
                    var io = ImGui.GetIO();
                    EditorCameraController.Zoom(io.MouseWheel);
                        
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                    {
                        EditorCameraController.Move(io.MouseDelta * new Vector2(-1, 1));
                        io.MouseDelta = Vector2.Zero;
                    }
                    else if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && EnableSelect)
                    {
                        Vector2 texpos = ImGui.GetMousePos() - new Vector2(WindowContentAreaMin.X, WindowContentAreaMax.Y);
                        texpos.Y = -texpos.Y;
                        int result = pipeline.Framebuffer.ReadPixel((uint)Context.Scene.Pipeline.FBStructureIndex.ID, (int)texpos.X, (int)texpos.Y);
                        EditorApplication.Log.Trace($"selected entity id {result}");

                        Context.ActiveEntity = Context.Scene.GetEntity(result);
                    }
                }

                // draw
                Context.Scene.Pipeline = pipeline;
                Context.Scene.OnRenderEditor(EditorCameraController.Camera.ViewProjectionMatrix);

                Framebuffer.Unbind();
            }
        }
    }
}
