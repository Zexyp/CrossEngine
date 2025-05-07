using CrossEngine.Components;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Core.Services;
using CrossEngine.Utils;
using CrossEngineEditor.Viewport;
using ImGuiNET;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering;
using CrossEngine.Core;
using CrossEngine.Logging;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Serialization;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.ImGui;
using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    internal class ViewportPanel : SceneViewPanel
    {
        protected override ICamera DrawCamera => _editorCamera;
        protected override Scene Scene => Context.Scene;
        
        private Camera _editorCamera = new Camera();
        private float _cameraSpeed = 10;
        
        private Vector3 _cameraPosition = Vector3.Zero;
        private Quaternion _cameraRotation = Quaternion.Identity;
        private float _near = .1f;
        private float _far = 1000f;
        private float _fov = 90f;
        private float _zoom = 10;
        private bool _perspective = false;
        private bool _enableOverlays = true;
        
        private bool _projectionDirty = true;
        private bool _viewDirty = true;
        private readonly List<(IViewportOverlay Overlay, bool Draw)> _overlays = new();
        private Vector2 _lookRot = Vector2.Zero;
        private bool IsTouchpad => EditorApplication.Service.Preferences["navigation"].ReadBooleanOrDefault("touchpad", true);
        private ViewportPass _viewportPass;
        private float SensitivityPan => EditorApplication.Service.Preferences["navigation"].ReadSingleOrDefault("touchpad.sensitivity.pan", .25f);
        private float SensitivityRotate => EditorApplication.Service.Preferences["navigation"].ReadSingleOrDefault("touchpad.sensitivity.rotate", 8);
        //private ImGuiHelper.ImDrawCallback _callbackHolder;
        private bool _showGbuffers = false;
        private bool _simpleMove = false;

        public ViewportPanel()
        {
            WindowName = "Viewport";
            WindowFlags = ImGuiWindowFlags.MenuBar;

            AddOverlay(new TransformsOverlay());
            AddOverlay(new CameraOverlay());
            AddOverlay(new IconOverlay());
            AddOverlay(new EmitterOverlay());
            AddOverlay(new SelectedOverlay());
            AddOverlay(new NameOverlay());
            AddOverlay(new ViewportCullCkecker());
            
            _viewportPass = new ViewportPass() { Overlays = _overlays };
        }

        protected override unsafe void DrawWindowContent()
        {
            DrawMenuBar();

            var cursorPos = ImGui.GetCursorPos();

            // overlay begin
            bool overlaysAttached = false;
            if (_enableOverlays)
            {
                AttachPass(Scene);
                overlaysAttached = true;
            }

            base.DrawWindowContent();

            // remove overlay
            if (overlaysAttached) DetachPass(Scene);

            if (Scene == null || Framebuffer == null)
                return;

            var io = ImGui.GetIO();

            // selection
            if (ImGui.IsItemClicked() && Context.Scene != null)
            {
                Vector2 winpos = ImGui.GetWindowPos();
                Vector2 texpos = ImGui.GetMousePos() - new Vector2(ImGui.GetWindowContentRegionMin().X + winpos.X,
                    ImGui.GetWindowContentRegionMax().Y + winpos.Y);
                texpos.Y = -texpos.Y;
                var result = Framebuffer.GetValue().ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                Context.ActiveEntity = Context.Scene.GetEntity(result);
            }
            // simple move
            else if (_simpleMove && Context.ActiveEntity != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left) && Focused)
            {
                var trans = Context.ActiveEntity.Transform;
                if (!_perspective)
                    trans.WorldPosition += Vector3.Transform(new Vector3(io.MouseDelta.X / ViewportSize.Y * _zoom, -io.MouseDelta.Y / ViewportSize.Y * _zoom, 0), _cameraRotation);
                else
                {
                    var tan = MathF.Tan(_fov) * _zoom;
                    trans.WorldPosition -= Vector3.Transform(new Vector3(io.MouseDelta.X / ViewportSize.Y * tan, -io.MouseDelta.Y / ViewportSize.Y * tan, 0), _cameraRotation);
                }
            }

            // navigation
            else
                Navigate(io);

            // update after navigate since navigate causes update
            if (_projectionDirty)
                UpdateProjection();

            if (_viewDirty)
                UpdateView();

            if (_showGbuffers)
            {
                // debug buffers
                ImGui.SetCursorPos(cursorPos);
                var buffer = Scene.World.GetSystem<RenderSystem>().Pipeline.Buffer.GetValue();
                
                //var drawList = ImGui.GetWindowDrawList();
                //drawList.AddCallback(Marshal.GetFunctionPointerForDelegate<ImGuiHelper.ImDrawCallback>(_callbackHolder = ((drawListPtr, drawCmdPtr) =>
                //{
                //    drawCmdPtr++;
                //    drawCmdPtr->TextureId = (IntPtr)buffer.GetColorAttachmentRendererID(0);
                //})), IntPtr.Zero);
                //drawList.AddImage(IntPtr.Zero, Vector2.Zero, Vector2.One * 200);
                
                ImGui.Image((IntPtr)buffer.GetColorAttachmentRendererID(0), Vector2.One * 200, 
                    new Vector2(0, 1),
                    new Vector2(1, 0));
                ImGui.SameLine();
                ImGui.Image((IntPtr)buffer.GetColorAttachmentRendererID(1), Vector2.One * 200, 
                    new Vector2(0, 1),
                    new Vector2(1, 0));
                
                ImGui.Image((IntPtr)buffer.GetColorAttachmentRendererID(2), Vector2.One * 200, 
                    new Vector2(0, 1),
                    new Vector2(1, 0));
                ImGui.SameLine();
                ImGui.Image((IntPtr)buffer.GetColorAttachmentRendererID(3), Vector2.One * 200, 
                    new Vector2(0, 1),
                    new Vector2(1, 0));
            }
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
                _viewDirty = true;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.KeypadDecimal) && Context.ActiveEntity?.Transform != null)
            {
                _cameraPosition = Context.ActiveEntity.Transform.Position;
                _viewDirty = true;
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
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheel * _zoom * SensitivityPan, io.MouseWheelH * _zoom * SensitivityPan, 0), _cameraRotation);
                    else
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * SensitivityPan, io.MouseWheel * _zoom * SensitivityPan, 0), _cameraRotation);

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
                        _lookRot += new Vector2(-io.MouseWheel, -io.MouseWheelH) / SensitivityRotate;
                    else
                        _lookRot += new Vector2(-io.MouseWheelH, -io.MouseWheel) / SensitivityRotate;

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
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheel * _zoom * SensitivityPan, io.MouseWheelH * _zoom * SensitivityPan, 0), _cameraRotation);
                    else
                        _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * SensitivityPan, io.MouseWheel * _zoom * SensitivityPan, 0), _cameraRotation);

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

        public override void OnDetach()
        {
            base.OnDetach();

            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Overlay.Context = null;
            }
        }

        private void AttachPass(Scene scene)
        {
            scene?.World.GetSystem<RenderSystem>().Pipeline.PushBack(_viewportPass);
            for (int i = 0; i < _overlays.Count; i++) 
                if (_overlays[i].Draw)
                    _overlays[i].Overlay.Prepare();
        }

        private void DetachPass(Scene scene)
        {
            for (int i = 0; i < _overlays.Count; i++)
                if (_overlays[i].Draw)
                    _overlays[i].Overlay.Finish();
            scene?.World.GetSystem<RenderSystem>().Pipeline.Remove(_viewportPass);
        }

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
                        var cam = Context.Scene?.World.GetSystem<RenderSystem>().PrimaryCamera;
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
                    
                    ImGui.Separator();
                    
                    if (ImGui.MenuItem("Reset"))
                    {
                        _cameraPosition = Vector3.Zero;
                        _cameraRotation = Quaternion.Identity;
                        _lookRot = Vector2.Zero;
                        _viewDirty = true;
                    }
                    
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Overlays"))
                {
                    if (ImGui.MenuItem("Enable", null, _enableOverlays))
                    {
                        _enableOverlays = !_enableOverlays;
                    }
                    ImGui.Separator();
                    for (int i = 0; i < _overlays.Count; i++)
                    {
                        var (overlay, drawn) = _overlays[i];
                        if (ImGui.BeginMenu(overlay.GetType().Name))
                        {
                            if (ImGui.MenuItem("Enable", "", drawn))
                            {
                                if (drawn) _overlays[i] = (overlay, false);
                                else _overlays[i] = (overlay, true);
                            }
                            
                            ImGui.Separator();
                            
                            InspectDrawer.Inspect(overlay);
                            
                            ImGui.EndMenu();
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("debug"))
                {
                    if (ImGui.MenuItem("gbuffers", null, _showGbuffers))
                        _showGbuffers = !_showGbuffers;
                    if (ImGui.MenuItem("simple move", null, _simpleMove))
                        _simpleMove = !_simpleMove;
                    
                    if (ImGui.BeginMenu("polygon mode"))
                    {
                        if (ImGui.MenuItem("point"))
                            GraphicsContext.Current.Api.SetPolygonMode(PolygonMode.Point);
                        if (ImGui.MenuItem("line"))
                            GraphicsContext.Current.Api.SetPolygonMode(PolygonMode.Line);
                        if (ImGui.MenuItem("fill"))
                            GraphicsContext.Current.Api.SetPolygonMode(PolygonMode.Fill);
                    
                        ImGui.EndMenu();
                    }
                    
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        public override void SaveState(SerializationInfo info)
        {
            base.SaveState(info);
            
            info.AddValue("view.position", _cameraPosition);
            info.AddValue("view.rotation", _cameraRotation.AsVec4());
            info.AddValue("view.zoom", _zoom);
            info.AddValue("view.perspective", _perspective);
            info.AddValue("view.near", _near);
            info.AddValue("view.far", _far);
            info.AddValue("view.fov", _fov);
            info.AddValue("view.enableOverlays", _enableOverlays);
        }

        public override void LoadState(SerializationInfo info)
        {
            base.LoadState(info);

            _cameraPosition = info.GetValue("view.position", _cameraPosition);
            _cameraRotation = info.GetValue("view.rotation", _cameraRotation.AsVec4()).AsQuat();
            _zoom = info.GetValue("view.zoom", _zoom);
            _perspective = info.GetValue("view.perspective", _perspective);
            _near = info.GetValue("view.near", _near);
            _far = info.GetValue("view.far", _far);
            _fov = info.GetValue("view.fov", _fov);
            _enableOverlays = info.GetValue("view.enableOverlays", _enableOverlays);

            _viewDirty = true;
            _projectionDirty = true;
        }

        [Obsolete("no init")]
        private void AddOverlay(IViewportOverlay overlay, bool draw = true)
        {
            overlay.Camera = _editorCamera;
            _overlays.Add((overlay, draw));
        }

        [Obsolete("no init")]
        private void RemoveOverlay(IViewportOverlay overlay)
        {
            _overlays.RemoveAll(tup => tup.Overlay == overlay);
            overlay.Camera = null;
        }

        public override void OnOpen()
        {
            EditorApplication.Service.RendererRequest(() =>
            {
                for (int i = 0; i < _overlays.Count; i++)
                {
                    _overlays[i].Overlay.Init();
                }
            });

            base.OnOpen();
        }
        
        public override void OnClose()
        {
            base.OnClose();

            EditorApplication.Service.RendererRequest(() =>
            {
                for (int i = 0; i < _overlays.Count; i++)
                {
                    _overlays[i].Overlay.Destroy();
                }
            });
        }

        private class ViewportPass : Pass
        {
            public List<(IViewportOverlay Overlay, bool Draw)> Overlays;
            
            public override void Draw()
            {
                var buffer = Pipeline.Buffer.GetValue();
                buffer.Bind();

                for (int i = 0; i < Overlays.Count; i++)
                {
                    var (overlay, draw) = Overlays[i];
                    if (!draw) continue;
                    
                    buffer.EnableColorAttachments(overlay.ModifyAttachments);
                    
                    try
                    {
                        overlay.Draw();
                    }
                    catch (Exception e)
                    {
                        EditorService.Log.Error($"viewport: overlay '{overlay.GetType().FullName}' draw failed:\n{e}");
                    }
                }
            }
        }
    }
}
