using ImGuiNET;
using ImGuizmoNET;
using System.Numerics;
using CrossEngine.Entities.Components;
using CrossEngine.Utils;

namespace CrossEngineEditor
{
    class GizmoPanel : EditorPanel
    {
        public GizmoPanel() : base("Gizmo")
        {

        }

        OPERATION currentGizmoOperation = OPERATION.TRANSLATE;
        MODE currentGizmoMode = MODE.WORLD;

        protected override void DrawWindowContent()
        {
            ViewportPanel viewportPanel = EditorLayer.Instance.GetPanel<ViewportPanel>();

            if (viewportPanel == null || !viewportPanel.IsOpen()) return;

            if (EditorLayer.Instance.SelectedEntity != null && EditorLayer.Instance.SelectedEntity.HasComponent<TransformComponent>())
            {
                ImGuiIOPtr io = ImGui.GetIO();
                
                ImGuizmo.SetOrthographic(true);

                if (EditorLayer.Instance.EditorCamera != null)
                {
                    Matrix4x4 cameraView = EditorLayer.Instance.EditorCamera.ViewMatrix;
                    Matrix4x4 cameraProjection = EditorLayer.Instance.EditorCamera.ProjectionMatrix;

                    var tc = EditorLayer.Instance.SelectedEntity.GetComponent<TransformComponent>();
                    Matrix4x4 transformMat = tc.WorldTransformMatrix;

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

                    // fuck it... it will overdraw everything
                    //ImGuizmo.SetDrawlist(ImGui.GetBackgroundDrawList());

                    ImGuizmo.SetRect(viewportPanel.ContentMin.X, viewportPanel.ContentMin.Y, viewportPanel.ContentMax.X - viewportPanel.ContentMin.X, viewportPanel.ContentMax.Y - viewportPanel.ContentMin.Y);
                    //ref *(float*)(void*)null
                    if (ImGuizmo.Manipulate(ref cameraView.M11, ref cameraProjection.M11, currentGizmoOperation, (currentGizmoOperation != OPERATION.SCALE) ? currentGizmoMode : MODE.LOCAL, ref transformMat.M11))
                    {
                        // safety feature
                        if (!Matrix4x4Extension.HasNaNElement(transformMat))
                            EditorLayer.Instance.SelectedEntity.GetComponent<TransformComponent>().SetTransform(transformMat);
                    }
                }
            }
        }
    }
}
