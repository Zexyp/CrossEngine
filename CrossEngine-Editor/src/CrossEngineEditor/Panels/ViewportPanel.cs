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
using CrossEngine.Utils.Editor;
using CrossEngine.Components;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    class ViewportPanel : EditorPanel
    {
        Vector2 viewportSize;

        private Ref<Framebuffer> _framebuffer;

        private ControllableEditorCamera _currentCamera;

        [EditorInnerValue]
        public ControllableEditorCamera CurrentCamera
        {
            get { return _currentCamera; }
            set
            {
                if (value is OrthographicControllableEditorCamera) _projectionMode = ViewMode.Orthographic;
                else if (value is PerspectiveControllableEditorCamera) _projectionMode = ViewMode.Perspective;
                else _projectionMode = ViewMode.Undefined;
                EditorLayer.Instance.EditorCamera = _currentCamera = value;
                _currentCamera?.Resize(viewportSize.X, viewportSize.Y);
            }
        }


        OrthographicControllableEditorCamera orthographicCamera;
        PerspectiveControllableEditorCamera perspectiveCamera;

        public bool EnableSelect = true;

        public enum ViewMode
        {
            Orthographic,
            Perspective,
            Undefined,
        }

        private ViewMode _projectionMode;

        [EditorEnumValue]
        public ViewMode ProjectioMode
        {
            get { return _projectionMode; }
            set
            {
                _projectionMode = value;
                switch (_projectionMode)
                {
                    case ViewMode.Orthographic: CurrentCamera = orthographicCamera;
                        break;
                    case ViewMode.Perspective: CurrentCamera = perspectiveCamera;
                        break;
                    case ViewMode.Undefined: CurrentCamera = null; break;
                    default: throw new InvalidOperationException();
                }
            }
        }


        public ViewportPanel() : base("Viewport")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;
        }

        public override void OnAttach()
        {
            orthographicCamera = new OrthographicControllableEditorCamera();
            perspectiveCamera = new PerspectiveControllableEditorCamera();
            ProjectioMode = ViewMode.Orthographic;
        }

        public override void OnDetach()
        {
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

            ThreadManager.ExecuteOnRenderThread(() =>
            {
                _framebuffer = Framebuffer.Create(ref spec);
                Context.Scene.SetOutput(_framebuffer);
            });
        }

        public override void OnClose()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                ((Framebuffer?)_framebuffer).Dispose();
            });
        }

        protected override void PrepareWindow()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        protected override void EndPrepareWindow()
        {
            ImGui.PopStyleVar();
        }

        float lastZoomVectorLength;

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.BeginMenu("Mode"))
                    {
                        PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetMember(nameof(ViewportPanel.ProjectioMode))[0], this);
                        if (CurrentCamera != null) PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetMember(nameof(ViewportPanel.CurrentCamera))[0], this);

                        ImGui.EndMenu();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Focus"))
                    {
                        TransformComponent transform = null;
                        if (Context.ActiveEntity?.TryGetComponent(out transform) == true) CurrentCamera.Position = transform.Position;
                    }

                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Reset"))
                    {
                        CurrentCamera.Position = new Vector3(0, 0, 0);
                    }

                    ImGui.EndMenu();
                }

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

                        ((Framebuffer?)_framebuffer).Resize((uint)viewportSize.X, (uint)viewportSize.Y);

                        CurrentCamera.Resize(viewportSize.X, viewportSize.Y);
                    }
                }

                Context.Scene.SetEditorCamera(_currentCamera);

                // draw the framebuffer as image
                ImGui.Image(new IntPtr(((Framebuffer?)_framebuffer).GetColorAttachmentRendererID(0)),
                    viewportSize,
                    new Vector2(0, 1),
                    new Vector2(1, 0));

                // interaction
                if (ImGui.IsItemHovered() && Focused)
                {
                    // camera
                    var io = ImGui.GetIO();
                    if (io.MouseWheel != 0) CurrentCamera?.Zoom(io.MouseWheel);

                    if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                    {
                        if ((io.KeyMods & ImGuiKeyModFlags.Shift) > 0)
                        {
                            CurrentCamera?.Pan(io.MouseDelta * new Vector2(-1, 1));
                        }
                        else
                        {
                            CurrentCamera?.Move(io.MouseDelta * new Vector2(-1, 1));
                        }
                    }
                    // selection
                    else if (EnableSelect && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        Vector2 texpos = ImGui.GetMousePos() - new Vector2(WindowContentAreaMin.X, WindowContentAreaMax.Y);
                        texpos.Y = -texpos.Y;

                        int result = ((Framebuffer?)_framebuffer).ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                        ((Framebuffer?)_framebuffer).Unbind();
                        EditorApplication.Log.Trace($"selected entity id {result}");

                        Context.ActiveEntity = Context.Scene.GetEntityById(result);
                    }
                }

                // draw
                //Context.Scene.Pipeline = pipeline;
                //if (CurrentCamera != null) Context.Scene.OnRenderEditor(CurrentCamera.ViewProjectionMatrix);

                //Framebuffer.Unbind();
            }
        }
    }
}
