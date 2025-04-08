using CrossEngine.Components;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Services;
using CrossEngine.Utils;
using CrossEngineEditor.Viewport;
using ImGuiNET;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class ViewportPanel : SceneViewPanel
    {
        protected override ICamera DrawCamera => _editorCamera;
        protected override Scene Scene => Context.Scene;
        
        private Camera _editorCamera = new Camera();
        private Vector3 _cameraPosition = Vector3.Zero;
        private Quaternion _cameraRotation = Quaternion.Identity;
        private float _near = .1f;
        private float _far = 1000f;
        private float _fov = 90f;
        private float _zoom = 10;
        private bool _perspective = false;
        private bool _projectionDirty = true;
        private bool _viewDirty = true;
        private readonly List<IViewportOverlay> _overlays = new();
        private readonly List<IViewportOverlay> _drawOverlays = new();
        private Vector2 _lookRot = Vector2.Zero;

        public ViewportPanel(RenderService rs) : base(rs)
        {
            WindowName = "Viewport";
            WindowFlags = ImGuiWindowFlags.MenuBar;

            AddOverlay(new TransformsOverlay());
            AddOverlay(new SelectedOverlay());
            AddOverlay(new CameraOverlay());
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Frame Selected", Context.ActiveEntity?.Transform != null))
                    {
                        _cameraPosition = Context.ActiveEntity.Transform.Position;
                        _viewDirty = true;
                    }
                    
                    if (ImGui.MenuItem("Orthographic/Perspective"))
                    {
                        _perspective = !_perspective;
                        _projectionDirty = true;
                        _viewDirty = true;
                    }
                    
                    ImGui.Separator();

                    if (ImGui.BeginMenu("Camera"))
                    {
                        _projectionDirty |= ImGui.DragFloat("FOV", ref _fov);
                        _projectionDirty |= ImGui.DragFloat("Near", ref _near);
                        _projectionDirty |= ImGui.DragFloat("Far", ref _far);
                        if (ImGui.DragFloat("Zoom", ref _zoom))
                        {
                            _projectionDirty |= !_perspective;
                            _viewDirty |= _perspective;
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Viewpoint"))
                    {
                        var cam = Context.Scene?.Initialized == true ? Context.Scene?.World.GetSystem<RenderSystem>().PrimaryCamera : null;
                        if (ImGui.MenuItem("Camera", null, _editorCamera.ViewMatrix == cam?.ViewMatrix, cam != null)) {
                            _editorCamera.ViewMatrix = cam.ViewMatrix;
                        }

                        ImGui.Separator();

                        if (ImGui.MenuItem("Top", null, _cameraRotation == Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 2)))
                        {
                            _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 2);
                            _viewDirty = true;
                        }
                        if (ImGui.MenuItem("Bottom", null, _cameraRotation == Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2)))
                        {
                            _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 2);
                            _viewDirty = true;
                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Front", null, _cameraRotation == Quaternion.Identity))
                        {
                            _cameraRotation = Quaternion.Identity;
                            _viewDirty = true;
                        }
                        if (ImGui.MenuItem("Back", null, _cameraRotation == Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI)))
                        {
                            _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI);
                            _viewDirty = true;
                        }
                        ImGui.Separator();
                        if (ImGui.MenuItem("Right", null, _cameraRotation == Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2)))
                        {
                            _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2);
                            _viewDirty = true;
                        }
                        if (ImGui.MenuItem("Left", null, _cameraRotation == Quaternion.CreateFromAxisAngle(Vector3.UnitY, -MathF.PI / 2)))
                        {
                            _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -MathF.PI / 2);
                            _viewDirty = true;
                        }

                        ImGui.EndMenu();
                    }
                    
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Overlays"))
                {
                    for (int i = 0; i < _overlays.Count; i++)
                    {
                        var o = _overlays[i];
                        var drawn = _drawOverlays.Contains(o);
                        if (ImGui.MenuItem(o.GetType().Name, "", drawn))
                            if (drawn) _drawOverlays.Remove(o);
                            else _drawOverlays.Add(o);
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            base.DrawWindowContent();

            var io = ImGui.GetIO();
            
            if (ImGui.IsItemClicked() && Context.Scene != null)
            {
                Vector2 winpos = ImGui.GetWindowPos();
                Vector2 texpos = ImGui.GetMousePos() - new Vector2(ImGui.GetWindowContentRegionMin().X + winpos.X, ImGui.GetWindowContentRegionMax().Y + winpos.Y);
                texpos.Y = -texpos.Y;
                var result = Framebuffer.GetValue().ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                Context.ActiveEntity = Context.Scene.GetEntity(result);
            }

            else
            {
                // todo: keypad nav
                if (!_perspective)
                {
                    OrthoControl(io);
                }
                else
                {
                    if (!EditorApplication.Service.Preferences["navigation"].ReadBoolean("touchpad")) // mouse
                        PerspectiveMouseControl(io);
                    else
                        PerspectiveTouchpadControl(io);
                }
            }

            if (_projectionDirty)
                UpdateProjection();

            if (_viewDirty)
                UpdateView();
        }

        private void OrthoControl(in ImGuiIOPtr io)
        {
            if (ImGui.IsItemHovered() && Focused)
            {
                // zoom
                if (!io.KeyShift && io.MouseWheel != 0)
                {
                    _zoom -= io.MouseWheel * .5f / (1f / _zoom * 4f);
                    _zoom = Math.Max(_zoom, 0.1f);

                    _projectionDirty = true;
                }

                // pan
                if (io.KeyShift && (io.MouseWheel != 0 || io.MouseWheelH != 0))
                {
                    if (io.KeyAlt)
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheel * _zoom * .25f, io.MouseWheelH * _zoom * .25f, 0), _cameraRotation);
                    else
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * .25f, io.MouseWheel * _zoom * .25f, 0), _cameraRotation);

                    _viewDirty = true;
                }
            }

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle) && Focused)
            {
                // pan
                _cameraPosition += Vector3.Transform(new Vector3(-io.MouseDelta.X / ViewportSize.Y * _zoom, io.MouseDelta.Y / ViewportSize.Y * _zoom, 0), _cameraRotation);

                _viewDirty = true;
            }
        }

        private void PerspectiveMouseControl(in ImGuiIOPtr io)
        {
            if (ImGui.IsItemHovered() && Focused)
            {
                // zoom
                if (io.MouseWheel != 0)
                {
                    _zoom -= io.MouseWheel * .5f / (1f / _zoom * 4f);
                    _zoom = Math.Max(_zoom, 0.1f);

                    _viewDirty = true;
                }
            }

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle) && Focused)
            {
                // pan
                if (io.KeyShift)
                    _cameraPosition += Vector3.Transform(new Vector3(-io.MouseDelta.X / ViewportSize.Y * _zoom, io.MouseDelta.Y / ViewportSize.Y * _zoom, 0), _cameraRotation);
                // rotate
                else
                {
                    _lookRot += new Vector2(-io.MouseDelta.X, -io.MouseDelta.Y) / 256;
                    _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _lookRot.X) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, _lookRot.Y);
                }

                _viewDirty = true;
            }

            // todo: fly navigation
        }

        private void PerspectiveTouchpadControl(in ImGuiIOPtr io)
        {
            if (ImGui.IsItemHovered() && Focused)
            {
                // rotate
                if (!io.KeyShift && !io.KeyCtrl && (io.MouseWheel != 0 || io.MouseWheelH != 0))
                {
                    if (io.KeyAlt)
                        _lookRot += new Vector2(-io.MouseWheel, -io.MouseWheelH) / 8;
                    else
                        _lookRot += new Vector2(-io.MouseWheelH, -io.MouseWheel) / 8;

                    _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _lookRot.X) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, _lookRot.Y);

                    _viewDirty = true;
                }

                // zoom
                if (!io.KeyShift && io.KeyCtrl && io.MouseWheel != 0)
                {
                    _zoom -= io.MouseWheel * .5f / (1f / _zoom * 4f);
                    _zoom = Math.Max(_zoom, 0.1f);

                    _viewDirty = true;
                }

                // pan
                if (io.KeyShift && !io.KeyCtrl && (io.MouseWheel != 0 || io.MouseWheelH != 0))
                {
                    if (io.KeyAlt)
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheel * _zoom * .25f, io.MouseWheelH * _zoom * .25f, 0), _cameraRotation);
                    else
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * .25f, io.MouseWheel * _zoom * .25f, 0), _cameraRotation);

                    _viewDirty = true;
                }
            }
        }

        private void UpdateView()
        {
            if (!_perspective)
                _editorCamera.ViewMatrix = Matrix4x4.CreateTranslation(-_cameraPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(_cameraRotation));
            else
                _editorCamera.ViewMatrix = Matrix4x4.CreateTranslation(-_cameraPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(_cameraRotation)) * Matrix4x4.CreateTranslation(new Vector3(0, 0, -_zoom));
            _viewDirty = false;
        }

        private void UpdateProjection()
        {
            if (!_perspective)
                _editorCamera.SetOrtho((ViewportSize.X / ViewportSize.Y) * _zoom, 1 * _zoom, -_far, _far);
            else
                _editorCamera.SetPerspective(_fov * (MathF.PI / 180), (ViewportSize.X / ViewportSize.Y), _near, _far);
            _projectionDirty = false;
        }

        protected override void OnCameraResize()
        {
            base.OnCameraResize();

            UpdateProjection();

            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Resize(ViewportSize.X, ViewportSize.Y);
            }
        }

        public override void OnAttach()
        {
            base.OnAttach();

            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Context = Context;
            }
        }

        protected override void AugmentSceneRender()
        {
            for (int i = 0; i < _drawOverlays.Count; i++)
            {
                _drawOverlays[i].Draw();
            }
        }

        private void AddOverlay(IViewportOverlay overlay)
        {
            overlay.EditorCamera = _editorCamera;
            overlay.Context = Context;
            _overlays.Add(overlay);
            _drawOverlays.Add(overlay);
        }

        private void RemoveOverlay(IViewportOverlay overlay)
        {
            _drawOverlays.Remove(overlay);
            _overlays.Remove(overlay);
            overlay.EditorCamera = null;
            overlay.Context = null;
        }
    }
}
