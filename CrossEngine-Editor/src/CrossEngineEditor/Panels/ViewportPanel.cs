using System;
using ImGuiNET;
using ImGuizmoNET;

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
        Vector2 _viewportSize;

        private Ref<Framebuffer> _framebuffer;

        [EditorInnerDraw]
        public ControllableEditorCamera CurrentCamera
        {
            get { return _currentCamera; }
            set
            {
                EditorLayer.Instance.EditorCamera = _currentCamera = value;
                if (_currentCamera is OrthographicControllableEditorCamera) _projectionMode = ProjectionMode.Orthographic;
                else if (_currentCamera is PerspectiveControllableEditorCamera) _projectionMode = ProjectionMode.Perspective;
                else _projectionMode = ProjectionMode.Undefined;
                _currentCamera?.Resize(_viewportSize.X, _viewportSize.Y);
            }
        }

        private ControllableEditorCamera _currentCamera;
        [EditorDrag(Min = 0.25f)]
        public float _cameraMovementSpeed = 2;

        OrthographicControllableEditorCamera orthographicCamera;
        PerspectiveControllableEditorCamera perspectiveCamera;

        public enum ProjectionMode
        {
            Undefined = default,
            Orthographic,
            Perspective,
        }

        public enum ViewMode
        {
            Solid,
            Wireframe,
            Points,
        }

        private ProjectionMode _projectionMode = ProjectionMode.Orthographic;
        private ViewMode _viewMode = ViewMode.Solid;

        [EditorEnum]
        public ProjectionMode ProjectioMode
        {
            get { return _projectionMode; }
            set
            {
                _projectionMode = value;
                switch (_projectionMode)
                {
                    case ProjectionMode.Orthographic: CurrentCamera = orthographicCamera;
                        break;
                    case ProjectionMode.Perspective: CurrentCamera = perspectiveCamera;
                        break;
                }
            }
        }
        [EditorEnum]
        public ViewMode View
        {
            get { return _viewMode; }
            set
            {
                _viewMode = value;
                switch (_viewMode)
                {
                    case ViewMode.Solid:
                        Application.Instance.RendererAPI.SetPolygonMode(PolygonMode.Fill);
                        break;
                    case ViewMode.Wireframe:
                        Application.Instance.RendererAPI.SetPolygonMode(PolygonMode.Line);
                        break;
                    case ViewMode.Points:
                        Application.Instance.RendererAPI.SetPolygonMode(PolygonMode.Point);
                        break;
                }
            }
        }

        [EditorEnum]
        public OPERATION _currentGizmoOperation = OPERATION.TRANSLATE;
        [EditorEnum]
        public MODE _currentGizmoMode = MODE.WORLD;

        public ViewportPanel() : base("Viewport")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;

            orthographicCamera = new OrthographicControllableEditorCamera();
            perspectiveCamera = new PerspectiveControllableEditorCamera();
            ProjectioMode = ProjectionMode.Orthographic;
        }

        public override void OnAttach()
        {
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

            _viewportSize = -Vector2.One;

            ThreadManager.ExecuteOnRenderThread(() =>
            {
                _framebuffer = Framebuffer.Create(ref spec);
                Context.Scene._renderData.Output = (_framebuffer);
            });
        }

        public override void OnClose()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                ((Framebuffer?)_framebuffer).Dispose();
                Context.Scene._renderData.Output = (null);
                _framebuffer = null;
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

        protected override void DrawWindowContent()
        {
            #region MenuBar
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Gizmo"))
                {
                    PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetField(nameof(_currentGizmoMode)), this);
                    PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetField(nameof(_currentGizmoOperation)), this);

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View"))
                {
                    PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetMember(nameof(View))[0], this);
                    PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetMember(nameof(ProjectioMode))[0], this);
                    if (CurrentCamera != null)
                        PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetMember(nameof(CurrentCamera))[0], this);
                    PropertyDrawer.DrawEditorValue(typeof(ViewportPanel).GetField(nameof(_cameraMovementSpeed)), this);

                    ImGui.Separator();

                    if (ImGui.MenuItem("Focus"))
                    {
                        FocusView();
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
            #endregion

            // return if there is nothing to do
            if (Context.Scene == null || _currentCamera == null || _framebuffer == null)
                return;

            #region Framebuffer
            // resize check
            {
                Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                if (viewportPanelSize != _viewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                {
                    _viewportSize = viewportPanelSize;

                    ((Framebuffer?)_framebuffer).Resize((uint)_viewportSize.X, (uint)_viewportSize.Y);

                    CurrentCamera.Resize(_viewportSize.X, _viewportSize.Y);
                }
            }

            Context.Scene._overrideEditorCamera = (_currentCamera);

            // draw the framebuffer as image
            ImGui.Image(new IntPtr(((Framebuffer?)_framebuffer).GetColorAttachmentRendererID(0)),
                _viewportSize,
                new Vector2(0, 1),
                new Vector2(1, 0));
            #endregion

            bool disableSelect = false;
            if (Context.ActiveEntity?.Transform != null)
            {
                ImGuizmo.SetOrthographic(ProjectioMode == ViewportPanel.ProjectionMode.Orthographic);

                ImGuizmo.SetRect(WindowContentAreaMin.X,
                                    WindowContentAreaMin.Y,
                                    WindowContentAreaMax.X - WindowContentAreaMin.X,
                                    WindowContentAreaMax.Y - WindowContentAreaMin.Y);
                //ref *(float*)(void*)null

                var cameraView = _currentCamera.ViewMatrix;
                var cameraProjection = _currentCamera.ProjectionMatrix;
                var transformMat = Context.ActiveEntity.Transform.WorldTransformMatrix;

                ImGuizmo.SetDrawlist();
                if (ImGuizmo.Manipulate(ref cameraView.M11, ref cameraProjection.M11, _currentGizmoOperation, (_currentGizmoOperation != OPERATION.SCALE) ? _currentGizmoMode : MODE.LOCAL, ref transformMat.M11))
                {
                    // safety feature
                    if (!Matrix4x4Extension.HasNaNElement(transformMat))
                        Context.ActiveEntity.Transform.SetWorldTransform(transformMat);
                }

                disableSelect |= ImGuizmo.IsOver();
            }

            // interaction
            if (!Focused) return;

            var io = ImGui.GetIO();
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
            {
                if (io.KeyShift)
                {
                    CurrentCamera?.Pan(io.MouseDelta * new Vector2(-1, 1));
                }
                else
                {
                    CurrentCamera?.Move(io.MouseDelta * new Vector2(-1, 1));
                }
            }
            else if (ImGui.IsMouseDragging(ImGuiMouseButton.Right))
            {
                Vector3 move = Vector3.Zero;
                if (ImGui.IsKeyDown('W') || ImGui.IsKeyDown(ImGui.GetKeyIndex(ImGuiKey.UpArrow)))
                {
                    move += Vector3.UnitZ;
                }
                if (ImGui.IsKeyDown('S') || ImGui.IsKeyDown(ImGui.GetKeyIndex(ImGuiKey.DownArrow)))
                {
                    move -= Vector3.UnitZ;
                }
                if (ImGui.IsKeyDown('D') || ImGui.IsKeyDown(ImGui.GetKeyIndex(ImGuiKey.RightArrow)))
                {
                    move += Vector3.UnitX;
                }
                if (ImGui.IsKeyDown('A') || ImGui.IsKeyDown(ImGui.GetKeyIndex(ImGuiKey.LeftArrow)))
                {
                    move -= Vector3.UnitX;
                }
                if (ImGui.IsKeyDown('E') || ImGui.IsKeyDown(' '))
                {
                    move += Vector3.UnitY;
                }
                if (ImGui.IsKeyDown('Q') || io.KeyCtrl)
                {
                    move -= Vector3.UnitY;
                }

                if (io.KeyShift)
                {
                    move *= 2;
                }
                if (io.KeyAlt)
                {
                    move *= .5f;
                }

                move *= _cameraMovementSpeed * io.DeltaTime;
                _currentCamera.Fly(move, io.MouseDelta);

                string caminfo = $"x{_cameraMovementSpeed} | {_currentCamera.Position:0.0}";
                Vector2 tsize = ImGui.CalcTextSize(caminfo);
                ImGui.SetCursorPos(WindowContentAreaMax - WindowContentAreaMin - tsize);
                ImGui.Text(caminfo);
            }

            if (ImGui.IsItemHovered())
            {
                // camera
                if (io.MouseWheel != 0)
                {
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
                    {
                        _cameraMovementSpeed += io.MouseWheel / (1 / _cameraMovementSpeed * 4);
                        _cameraMovementSpeed = Math.Clamp(_cameraMovementSpeed, float.Epsilon, float.MaxValue);
                    }
                    else
                        CurrentCamera?.Zoom(io.MouseWheel);
                }
                
                // gizmo selection
                else
                {
                    if (ImGui.IsKeyPressed('F'))
                    {
                        FocusView();
                    }
                    if (ImGui.IsKeyPressed('G'))
                    {
                        _currentGizmoOperation = OPERATION.TRANSLATE;
                    }
                    if (ImGui.IsKeyPressed('R'))
                    {
                        _currentGizmoOperation = OPERATION.ROTATE;
                    }
                    if (ImGui.IsKeyPressed('S'))
                    {
                        _currentGizmoOperation = OPERATION.SCALE;
                    }
                }

                if (!disableSelect && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    Vector2 texpos = ImGui.GetMousePos() - new Vector2(WindowContentAreaMin.X, WindowContentAreaMax.Y);
                    texpos.Y = -texpos.Y;

                    int result = ((Framebuffer?)_framebuffer).ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                    ((Framebuffer?)_framebuffer).Unbind();
                    EditorApplication.Log.Trace($"selected entity id {result}");

                    Context.ActiveEntity = Context.Scene.GetEntityById(result);
                }
            }
        }

        private void FocusView()
        {
            TransformComponent transform = null;
            if (Context.ActiveEntity?.TryGetComponent(out transform) == true)
                CurrentCamera.Position = transform.Position;
        }
    }
}
