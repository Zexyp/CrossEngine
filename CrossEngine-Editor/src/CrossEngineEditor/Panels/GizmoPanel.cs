using ImGuiNET;
using ImGuizmoNET;

using System.Numerics;

using CrossEngine.Entities.Components;
using CrossEngine.Utils;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    class GizmoPanel : EditorPanel
    {
        ViewportPanel _viewportPanel;
        ViewportPanel ViewportPanel
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
            if (_viewportPanel == null || !_viewportPanel.IsOpen()) return;

            if (ready = (Context.ActiveEntity != null && Context.ActiveEntity.HasComponentOfType<TransformComponent>()))
            {
                ImGuizmo.SetOrthographic(true);

                if (EditorLayer.Instance.EditorCamera != null)
                {
                    cameraView = EditorLayer.Instance.EditorCamera.ViewMatrix;
                    cameraProjection = EditorLayer.Instance.EditorCamera.ProjectionMatrix;

                    var tc = Context.ActiveEntity.GetComponent<TransformComponent>();
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

                        ImGuiUtils.BeginGroupFrame();
                        if (ImGui.RadioButton("Translate", currentGizmoOperation == OPERATION.TRANSLATE))
                            currentGizmoOperation = OPERATION.TRANSLATE;
                        if (ImGui.RadioButton("Rotate", currentGizmoOperation == OPERATION.ROTATE))
                            currentGizmoOperation = OPERATION.ROTATE;
                        if (ImGui.RadioButton("Scale", currentGizmoOperation == OPERATION.SCALE))
                            currentGizmoOperation = OPERATION.SCALE;
                        ImGuiUtils.EndGroupFrame(0xff363636);
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

                    ImGui.Text("Mode");
                    {
                        if (currentGizmoOperation != OPERATION.SCALE)
                        {
                            ImGuiUtils.BeginGroupFrame();
                            if (ImGui.RadioButton("Local", currentGizmoMode == MODE.LOCAL))
                                currentGizmoMode = MODE.LOCAL;
                            ImGui.SameLine();
                            if (ImGui.RadioButton("World", currentGizmoMode == MODE.WORLD))
                                currentGizmoMode = MODE.WORLD;
                            ImGuiUtils.EndGroupFrame(0xff363636);
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

                ImGuizmo.SetRect(_viewportPanel.WindowContentAreaMin.X, _viewportPanel.WindowContentAreaMin.Y, _viewportPanel.WindowContentAreaMax.X - _viewportPanel.WindowContentAreaMin.X, _viewportPanel.WindowContentAreaMax.Y - _viewportPanel.WindowContentAreaMin.Y);
                //ref *(float*)(void*)null

                ImGuizmo.SetDrawlist();
                if (ImGuizmo.Manipulate(ref cameraView.M11, ref cameraProjection.M11, currentGizmoOperation, (currentGizmoOperation != OPERATION.SCALE) ? currentGizmoMode : MODE.LOCAL, ref transformMat.M11))
                {
                    // safety feature
                    if (!Matrix4x4Extension.HasNaNElement(transformMat))
                        Context.ActiveEntity.Transform.SetWorldTransform(transformMat);
                }
            }
        }

        public override void OnAttach()
        {
            ViewportPanel = EditorLayer.Instance.GetPanel<ViewportPanel>();
        }

        public override void OnDetach()
        {
            ViewportPanel = null;
        }
    }
}
