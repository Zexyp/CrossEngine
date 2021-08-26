using ImGuiNET;
using ImGuizmoNET;
using System.Numerics;
using CrossEngine.Entities.Components;
using CrossEngine.Utils;

namespace CrossEngineEditor.Panels
{
    class GizmoPanel : EditorPanel
    {
        ViewportPanel _viewportPanel;
        public ViewportPanel ViewportPanel
        {
            set
            {
                if (value == _viewportPanel) return;

                if (_viewportPanel != null) _viewportPanel.InnerAfterDrawCallback -= DrawGizmo;
                _viewportPanel = value;
                if (_viewportPanel != null) _viewportPanel.InnerAfterDrawCallback += DrawGizmo;
            }
        }

        public GizmoPanel() : base("Gizmo")
        {

        }

        OPERATION currentGizmoOperation = OPERATION.TRANSLATE;
        MODE currentGizmoMode = MODE.WORLD;

        Matrix4x4 cameraView;
        Matrix4x4 cameraProjection;

        Matrix4x4 transformMat;

        bool ready;

        protected override void DrawWindowContent()
        {
            ViewportPanel = EditorLayer.Instance.GetPanel<ViewportPanel>();

            if (_viewportPanel == null || !_viewportPanel.IsOpen()) return;

            if (ready = (EditorLayer.Instance.SelectedEntity != null && EditorLayer.Instance.SelectedEntity.HasComponent<TransformComponent>()))
            {
                ImGuiIOPtr io = ImGui.GetIO();
                
                ImGuizmo.SetOrthographic(true);

                if (EditorLayer.Instance.EditorCamera != null)
                {
                    cameraView = EditorLayer.Instance.EditorCamera.ViewMatrix;
                    cameraProjection = EditorLayer.Instance.EditorCamera.ProjectionMatrix;

                    var tc = EditorLayer.Instance.SelectedEntity.GetComponent<TransformComponent>();
                    transformMat = tc.WorldTransformMatrix;

                    ImGui.Text("Operation");
                    {
                        //if (ImGui.IsKeyDown('G') && io.KeyShift)
                        //    currentGizmoOperation = OPERATION.TRANSLATE;
                        //if (ImGui.IsKeyPressed('R') && io.KeyShift)
                        //    currentGizmoOperation = OPERATION.ROTATE;
                        //if (ImGui.IsKeyPressed('S') && io.KeyShift)
                        //    currentGizmoOperation = OPERATION.SCALE;
                        //if (ImGui.IsItemHovered())
                        //{
                        //    ImGui.BeginTooltip();
                        //    ImGui.TextUnformatted("shortcut");
                        //    ImGui.EndTooltip();
                        //}

                        if (ImGui.RadioButton("Translate", currentGizmoOperation == OPERATION.TRANSLATE))
                            currentGizmoOperation = OPERATION.TRANSLATE;
                        if (ImGui.RadioButton("Rotate", currentGizmoOperation == OPERATION.ROTATE))
                            currentGizmoOperation = OPERATION.ROTATE;
                        if (ImGui.RadioButton("Scale", currentGizmoOperation == OPERATION.SCALE))
                            currentGizmoOperation = OPERATION.SCALE;
                    }

                    //ImGui.Separator();
                    //
                    //ImGui.Text("Item transform");
                    //{
                    //    Vector3 translation = new Vector3();
                    //    Vector3 rotation = new Vector3();
                    //    Vector3 scale = new Vector3();
                    //
                    //    bool d = false;
                    //
                    //    ImGuizmo.DecomposeMatrixToComponents(ref transformMat.M11, ref translation.X, ref rotation.X, ref scale.X);
                    //    d |= ImGui.DragFloat3("Translation", ref translation);
                    //    d |= ImGui.DragFloat3("Rotation", ref rotation);
                    //    d |= ImGui.DragFloat3("Scale", ref scale);
                    //
                    //    if (d)
                    //    {
                    //        ImGuizmo.RecomposeMatrixFromComponents(ref translation.X, ref rotation.X, ref scale.X, ref transformMat.M11);
                    //        EditorLayer.Instance.SelectedEntity.GetComponent<TransformComponent>().SetTransformUseEuler(transformMat);
                    //    }
                    //}

                    ImGui.Separator();

                    ImGui.Text("Mode");
                    {
                        if (currentGizmoOperation != OPERATION.SCALE)
                        {
                            if (ImGui.RadioButton("Local", currentGizmoMode == MODE.LOCAL))
                                currentGizmoMode = MODE.LOCAL;
                            if (ImGui.RadioButton("World", currentGizmoMode == MODE.WORLD))
                                currentGizmoMode = MODE.WORLD;
                        }
                    }

                    // drawing of the gizmo is when the viewport draws
                }
            }
        }

        private void DrawGizmo(EditorPanel sender)
        {
            if (ready)
            {
                _viewportPanel.EnableSelect = !ImGuizmo.IsOver();

                ImGuizmo.SetRect(_viewportPanel.ContentMin.X, _viewportPanel.ContentMin.Y, _viewportPanel.ContentMax.X - _viewportPanel.ContentMin.X, _viewportPanel.ContentMax.Y - _viewportPanel.ContentMin.Y);
                //ref *(float*)(void*)null

                ImGuizmo.SetDrawlist();
                if (ImGuizmo.Manipulate(ref cameraView.M11, ref cameraProjection.M11, currentGizmoOperation, (currentGizmoOperation != OPERATION.SCALE) ? currentGizmoMode : MODE.LOCAL, ref transformMat.M11))
                {
                    // safety feature
                    if (!Matrix4x4Extension.HasNaNElement(transformMat))
                        EditorLayer.Instance.SelectedEntity.GetComponent<TransformComponent>().SetTransform(transformMat);
                }
            }
        }

        public override void OnDetach()
        {
            ViewportPanel = null;
        }
    }
}
