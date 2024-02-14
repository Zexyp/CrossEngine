using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Services;
using CrossEngine.Utils;
using ImGuiNET;
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
        private float _far = 100f;
        private float _fov = 90f;
        private float _zoom = 10;
        private bool _perspective = false;
        private bool _projectionDirty = true;
        private bool _viewDirty = true;

        public ViewportPanel(RenderService rs) : base(rs)
        {
            WindowName = "Viewport";
            WindowFlags = ImGuiWindowFlags.MenuBar;
        }

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.BeginMenu("Viewport"))
                    {
                        if (ImGui.MenuItem("Top") && (_viewDirty = true)) ;
                        if (ImGui.MenuItem("Bottom") && (_viewDirty = true)) ;
                        ImGui.Separator();
                        if (ImGui.MenuItem("Front") && (_viewDirty = true))
                            _cameraRotation = Quaternion.Identity;
                        if (ImGui.MenuItem("Back") && (_viewDirty = true))
                            _cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI);
                        ImGui.Separator();
                        if (ImGui.MenuItem("Right") && (_viewDirty = true)) ;
                        if (ImGui.MenuItem("Left") && (_viewDirty = true)) ;

                        ImGui.EndMenu();
                    }
                    
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            base.DrawWindowContent();

            var io = ImGui.GetIO();
            
            if (ImGui.IsItemClicked())
            {
                Vector2 winpos = ImGui.GetWindowPos();
                Vector2 texpos = ImGui.GetMousePos() - new Vector2(ImGui.GetWindowContentRegionMin().X + winpos.X, ImGui.GetWindowContentRegionMax().Y + winpos.Y);
                texpos.Y = -texpos.Y;
                var result = Framebuffer.GetValue().ReadPixel(1, (uint)texpos.X, (uint)texpos.Y);
                Context.ActiveEntity = Context.Scene.GetEntity(result);
            }

            else if (ImGui.IsItemHovered() && Focused)
            {
                // zoom
                if (io.KeyCtrl && io.MouseWheel != 0)
                {
                    _zoom -= io.MouseWheel * .5f / (1f / _zoom * 4f);
                    _zoom = Math.Max(_zoom, 0.1f);

                    _projectionDirty = true;
                }

                // pan
                if (io.KeyAlt && (io.MouseWheel != 0 || io.MouseWheelH != 0))
                {
                    _cameraPosition += Vector3.Transform(new Vector3(-io.MouseWheelH * _zoom * .25f, io.MouseWheel * _zoom * .25f, 0), _cameraRotation);

                    _viewDirty = true;
                }
            }

            if (_projectionDirty)
                OnCameraResize();

            if (_viewDirty)
                UpdateView();
        }

        private void UpdateView()
        {
            _editorCamera.ViewMatrix = Matrix4x4.CreateTranslation(-_cameraPosition) * Matrix4x4.CreateFromQuaternion(_cameraRotation);
        }

        protected override void OnCameraResize()
        {
            base.OnCameraResize();

            if (!_perspective)
                _editorCamera.SetOrtho((ViewportSize.X / ViewportSize.Y) * _zoom, 1 * _zoom, -_far, _far);
            else
                throw new NotImplementedException();
            _projectionDirty = false;
        }
    }
}
