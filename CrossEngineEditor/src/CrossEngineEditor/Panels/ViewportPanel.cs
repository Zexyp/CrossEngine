﻿using CrossEngine.Components;
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
using CrossEngine.Rendering;
using CrossEngine.Core;

namespace CrossEngineEditor.Panels
{
    internal class ViewportPanel : SceneViewPanel
    {
        protected override ICamera DrawCamera => _editorCamera;
        protected override Scene Scene => Context.Scene;
        
        private Camera _editorCamera = new Camera();
        private Vector3 _cameraPosition = Vector3.Zero;
        private Quaternion _cameraRotation = Quaternion.Identity;
        private float _cameraSpeed = 10;
        private float _near = .1f;
        private float _far = 1000f;
        private float _fov = 90f;
        private float _zoom = 10;
        private bool _perspective = false;
        private bool _projectionDirty = true;
        private bool _viewDirty = true;
        private readonly List<(IViewportOverlay Overlay, bool Draw)> _overlays = new();
        private Vector2 _lookRot = Vector2.Zero;
        private bool IsTouchpad => EditorApplication.Service.Preferences["navigation"].ReadBoolean("touchpad");
        private ViewportPass _viewportPass;
        private float _sensitivityPan = .25f;
        private float _sensitivityRotate = 8;

        public ViewportPanel(RenderService rs) : base(rs)
        {
            WindowName = "Viewport";
            WindowFlags = ImGuiWindowFlags.MenuBar;

            AddOverlay(new TransformsOverlay());
            AddOverlay(new CameraOverlay());
            AddOverlay(new SelectedOverlay());
            AddOverlay(new NameOverlay());
            
            _viewportPass = new ViewportPass() { Overlays = _overlays };
        }

        protected override void DrawWindowContent()
        {
            DrawMenuBar();

            if (Scene?.IsInitialized == true) AttachPass(Scene);
            
            base.DrawWindowContent();

            if (Scene?.IsInitialized == true) DetachPass(Scene);

            if (Scene?.IsInitialized != true || Framebuffer == null)
                return;

            var io = ImGui.GetIO();

            if (ImGui.IsItemClicked() && Context.Scene != null)
            {
                Vector2 winpos = ImGui.GetWindowPos();
                Vector2 texpos = ImGui.GetMousePos() - new Vector2(ImGui.GetWindowContentRegionMin().X + winpos.X,
                    ImGui.GetWindowContentRegionMax().Y + winpos.Y);
                texpos.Y = -texpos.Y;
                var result = Framebuffer.GetValue().ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                Context.ActiveEntity = Context.Scene.GetEntity(result);
            }

            else
                Navigate(io);

            if (_projectionDirty)
                UpdateProjection();

            if (_viewDirty)
                UpdateView();
        }

        private void Navigate(in ImGuiIOPtr io)
        {
            // todo: fly navigation

            KeyboardNav(io);
            if (!_perspective)
            {
                OrthoControl(io);
            }
            else
            {
                if (!IsTouchpad)
                    PerspectiveMouseControl(io);
                else
                    PerspectiveTouchpadControl(io);
            }
        }

        #region Controls
        private void KeyboardNav(in ImGuiIOPtr io)
        {
            if (!(ImGui.IsItemHovered() && Focused))
                return;

            var offset = Vector3.Zero;
            if (ImGui.IsKeyDown(ImGuiKey.Keypad8)) offset.Y += 1;
            if (ImGui.IsKeyDown(ImGuiKey.Keypad2)) offset.Y -= 1;
            if (ImGui.IsKeyDown(ImGuiKey.Keypad6)) offset.X += 1;
            if (ImGui.IsKeyDown(ImGuiKey.Keypad4)) offset.X -= 1;
            if (io.KeyShift) offset *= 2;
            offset *= Time.DeltaF;
            if (offset != Vector3.Zero)
            {
                if (io.KeyCtrl)
                {
                    offset = Vector3.Transform(offset, _cameraRotation);
                    _cameraPosition += offset * _cameraSpeed;
                }
                else
                {
                    _lookRot += new Vector2(offset.X, -offset.Y);
                    _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, _lookRot.X) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, _lookRot.Y);
                }
                _viewDirty = true;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Keypad5))
            {
                _perspective = !_perspective;
                _projectionDirty = true;
            }
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
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheel * _zoom * _sensitivityPan, io.MouseWheelH * _zoom * _sensitivityPan, 0), _cameraRotation);
                    else
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * _sensitivityPan, io.MouseWheel * _zoom * _sensitivityPan, 0), _cameraRotation);

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

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && Focused)
            {


                _viewDirty = true;
            }
        }

        private void PerspectiveTouchpadControl(in ImGuiIOPtr io)
        {
            if (ImGui.IsItemHovered() && Focused)
            {
                // rotate
                if (!io.KeyShift && !io.KeyCtrl && (io.MouseWheel != 0 || io.MouseWheelH != 0))
                {
                    if (io.KeyAlt)
                        _lookRot += new Vector2(-io.MouseWheel, -io.MouseWheelH) / _sensitivityRotate;
                    else
                        _lookRot += new Vector2(-io.MouseWheelH, -io.MouseWheel) / _sensitivityRotate;

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
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheel * _zoom * _sensitivityPan, io.MouseWheelH * _zoom * _sensitivityPan, 0), _cameraRotation);
                    else
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * _sensitivityPan, io.MouseWheel * _zoom * _sensitivityPan, 0), _cameraRotation);

                    _viewDirty = true;
                }
            }
        }
        #endregion


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
                _overlays[i].Overlay.Resize(ViewportSize.X, ViewportSize.Y);
            }
        }

        public override void OnAttach()
        {
            base.OnAttach();

            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Overlay.Context = Context;
            }
        }
        
        private void AttachPass(Scene scene) => scene?.World.GetSystem<RenderSystem>().Pipeline.PushBack(_viewportPass);
        private void DetachPass(Scene scene) => scene?.World.GetSystem<RenderSystem>().Pipeline.Remove(_viewportPass);

        private void DrawMenuBar()
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

                    if (ImGui.MenuItem("Reset"))
                    {
                        _cameraPosition = Vector3.Zero;
                        _cameraRotation = Quaternion.Identity;
                        _lookRot = Vector2.Zero;
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
                        var cam = Context.Scene?.IsInitialized == true ? Context.Scene?.World.GetSystem<RenderSystem>().PrimaryCamera : null;
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
                        var drawn = _overlays[i].Draw;
                        if (ImGui.MenuItem(_overlays[i].Overlay.GetType().Name, "", drawn))
                            if (drawn) _overlays[i] = (_overlays[i].Overlay, false);
                            else _overlays[i] = (_overlays[i].Overlay, true);
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        private void AddOverlay(IViewportOverlay overlay, bool draw = true)
        {
            overlay.EditorCamera = _editorCamera;
            overlay.Context = Context;
            _overlays.Add((overlay, draw));
        }

        private void RemoveOverlay(IViewportOverlay overlay)
        {
            _overlays.RemoveAll(tup => tup.Overlay == overlay);
            overlay.EditorCamera = null;
            overlay.Context = null;
        }

        private class ViewportPass : Pass
        {
            public List<(IViewportOverlay Overlay, bool Draw)> Overlays;
            
            public override void Draw()
            {
                for (int i = 0; i < Overlays.Count; i++)
                {
                    if (Overlays[i].Draw)
                        Overlays[i].Overlay.Draw();
                }
            }
        }
    }
}
