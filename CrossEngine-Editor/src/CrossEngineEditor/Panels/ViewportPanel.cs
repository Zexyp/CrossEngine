using System;
using ImGuiNET;
using ImGuizmoNET;

using System.Numerics;
using System.Diagnostics;

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
using CrossEngine.ECS;

using CrossEngineEditor.Utils;
using CrossEngineEditor.Operations;

namespace CrossEngineEditor.Panels
{
    class ViewportPanel : SceneViewPanel
    {
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

        [EditorInnerDraw]
        public ControllableEditorCamera CurrentCamera
        {
            get { return (ControllableEditorCamera)DrawCamera; }
            set
            {
                DrawCamera = value;
                if (DrawCamera is OrthographicControllableEditorCamera) _projectionMode = ProjectionMode.Orthographic;
                else if (DrawCamera is PerspectiveControllableEditorCamera) _projectionMode = ProjectionMode.Perspective;
                else _projectionMode = ProjectionMode.Undefined;
                ((ControllableEditorCamera)DrawCamera)?.Resize(ViewportSize.X, ViewportSize.Y);
            }
        }

        [EditorDrag(0.25f, float.MaxValue)]
        public float _cameraMovementSpeed = 2;

        private OrthographicControllableEditorCamera _orthographicCamera;
        private PerspectiveControllableEditorCamera _perspectiveCamera;

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
                    case ProjectionMode.Orthographic: CurrentCamera = _orthographicCamera;
                        break;
                    case ProjectionMode.Perspective: CurrentCamera = _perspectiveCamera;
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

        public ViewportPanel()
        {
            WindowName = "Viewport";

            WindowFlags |= ImGuiWindowFlags.MenuBar;

            _orthographicCamera = new OrthographicControllableEditorCamera();
            _perspectiveCamera = new PerspectiveControllableEditorCamera();
            ProjectioMode = ProjectionMode.Orthographic;
        }

        public override void OnAttach()
        {
        }

        public override void OnDetach()
        {
        }

        private bool gizmoUsed = false;
        private bool lastGizmoUsed = false;
        private EntityTransformChangeOperation transformOperation = null;
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
            if (Context.Scene == null || CurrentCamera == null)
                return;

            base.DrawWindowContent();

            bool disableSelect = false;
            var transformComponent = Context.ActiveEntity?.Transform;
            if (transformComponent != null)
            {
                ImGuizmo.SetOrthographic(ProjectioMode == ViewportPanel.ProjectionMode.Orthographic);

                ImGuizmo.SetRect(WindowContentAreaMin.X,
                                    WindowContentAreaMin.Y,
                                    WindowContentAreaMax.X - WindowContentAreaMin.X,
                                    WindowContentAreaMax.Y - WindowContentAreaMin.Y);
                //ref *(float*)(void*)null

                var cameraView = CurrentCamera.ViewMatrix;
                var cameraProjection = CurrentCamera.ProjectionMatrix;
                var transformMat = transformComponent.WorldTransformMatrix;

                bool manipulated;

                var prevTransform = transformComponent.TransformMatrix;

                ImGuizmo.SetDrawlist();
                if (manipulated = ImGuizmo.Manipulate(ref cameraView.M11, ref cameraProjection.M11, _currentGizmoOperation, (_currentGizmoOperation != OPERATION.SCALE) ? _currentGizmoMode : MODE.LOCAL, ref transformMat.M11))
                {
                    if (!Matrix4x4Extension.HasNaNElement(transformMat))
                        transformComponent.SetWorldTransform(transformMat);
                }

                gizmoUsed = ImGuizmo.IsUsing();
                if (gizmoUsed != lastGizmoUsed && gizmoUsed)
                {
                    Debug.Assert(transformOperation == null);
                    transformOperation = new EntityTransformChangeOperation(transformComponent, prevTransform);
                }
                if (gizmoUsed != lastGizmoUsed && !gizmoUsed)
                {
                    if (!Matrix4x4Extension.HasNaNElement(transformMat) && transformOperation.PreviousTransform != transformMat)
                    {
                        transformOperation.NextTransform = transformMat;
                        Context.Operations?.Push(transformOperation);
                    }
                    transformOperation = null;
                }
                lastGizmoUsed = gizmoUsed;

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

                _cameraMovementSpeed += io.MouseWheel * 0.25f * _cameraMovementSpeed;
                _cameraMovementSpeed = Math.Max(0.1f, _cameraMovementSpeed);
                move *= _cameraMovementSpeed * io.DeltaTime;
                CurrentCamera.Fly(move, io.MouseDelta);

                string caminfo = $"x{_cameraMovementSpeed} | {CurrentCamera.Position:0.0}";
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
                
                // keyboard shorcuts
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

                // selection
                if (!disableSelect && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    Vector2 texpos = ImGui.GetMousePos() - new Vector2(WindowContentAreaMin.X, WindowContentAreaMax.Y);
                    texpos.Y = -texpos.Y;

                    int result = ((Framebuffer)Framebuffer).ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                    ((Framebuffer)Framebuffer).Unbind();
                    EditorApplication.Log.Trace($"selected entity id {result}");

                    var selectedEnt = Context.Scene.GetEntityById(result);
                    if (selectedEnt != Context.ActiveEntity)
                    {
                        var op = new EntitySelectOpertion(Context, Context.ActiveEntity, selectedEnt);
                        Context.Operations.Push(op);
                        Context.ActiveEntity = selectedEnt;
                    }
                }
            }
        }

        private void FocusView()
        {
            TransformComponent transform = null;
            if (Context.ActiveEntity?.TryGetComponent(out transform) == true)
                CurrentCamera.Position = transform.Position;
        }

        protected override void Resized()
        {
            ((ControllableEditorCamera)DrawCamera)?.Resize(ViewportSize.X, ViewportSize.Y);
        }
    }
}
