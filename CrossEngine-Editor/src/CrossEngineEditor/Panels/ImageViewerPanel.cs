using System;
using ImGuiNET;
using ImGuizmoNET;

using System.Numerics;
using System.Text;

using CrossEngine;
using CrossEngine.Assets;
using CrossEngine.Scenes;
using CrossEngine.Logging;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering.Textures;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    class ImageViewerPanel : EditorPanel
    {
        public ImageViewerPanel() : base("Image Viewer")
        {
            this.WindowFlags |= ImGuiWindowFlags.MenuBar;
        }

        AssetCollection<TextureAsset> textureAssets = null;
        AssetCollection<TextureAsset> TextureAssets
        {
            set
            {
                textureAssets = value;
                selectedTextureAsset = null;
            }
        }
        [EditorAsset(typeof(TextureAsset), Name = "")]
        public TextureAsset selectedTextureAsset = null;
        int selectedIndex = 0;

        Vector2 viewPos;
        float viewZoom = 1f;

        #region Prepare Window
        protected override void PrepareWindow()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        protected override void EndPrepareWindow()
        {
            ImGui.PopStyleVar();
        }
        #endregion

        Ref<Texture> selectedBoi;
        private bool moveStarted = false;

        protected override void DrawWindowContent()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Reset"))
                    {
                        ResetView();
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Image", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Load"))
                    {
                        LoadAsset();
                    }
                    if (ImGui.MenuItem("Unload", selectedTextureAsset != null))
                    {
                        UnloadAsset();
                    }
                    ImGui.Separator();

                    ImGui.EndMenu();
                }

                PropertyDrawer.DrawEditorValue(this.GetType().GetField(nameof(selectedTextureAsset)), this);

                ImGui.EndMenuBar();
            }

            if (!Ref.IsNull(selectedBoi))
            {
                var cursorStartPos = ImGui.GetCursorPos();
                ImGui.InvisibleButton("##interactor", WindowContentAreaMax - WindowContentAreaMin);
                bool mouseDrag = ImGui.IsMouseDragging(ImGuiMouseButton.Left) || ImGui.IsMouseDragging(ImGuiMouseButton.Middle);
                if (Focused && ImGui.IsItemHovered())
                    moveStarted = true;
                else if (!mouseDrag)
                    moveStarted = false;
                if (moveStarted)
                {
                    var io = ImGui.GetIO();
                    if (mouseDrag)
                    {
                        viewPos += 1 / viewZoom * io.MouseDelta;
                    }
                    if (io.MouseWheel != 0.0f)
                    {
                        viewZoom += io.MouseWheel * 0.25f * viewZoom;
                        viewZoom = Math.Max(viewZoom, 1.0f / 8);
                    }
                }
                ImGui.SetCursorPos(cursorStartPos);

                var tex = selectedBoi.Value;
                var cursorScreenCenter = (WindowContentAreaMax + WindowContentAreaMin) / 2;
                var imgTransMat = Matrix3x2.CreateTranslation(viewPos) * Matrix3x2.CreateScale(viewZoom) * Matrix3x2.CreateTranslation(cursorScreenCenter);
                var min = Vector2.Transform(-tex.Size / 2, imgTransMat);
                var max = Vector2.Transform( tex.Size / 2, imgTransMat);

                ImGui.GetWindowDrawList().AddImage(new IntPtr(tex.RendererId), min, max, new (0, 1), new (1, 0));
                ImGui.GetWindowDrawList().AddCircle(WindowContentAreaMin, 20, 0xff00ffff);
                ImGui.GetWindowDrawList().AddCircle(WindowContentAreaMax, 20, 0xff00ffff);

                Vector2 bakus = Vector2.Zero;
                if (min.X - WindowContentAreaMin.X < 0)
                    bakus.X += WindowContentAreaMin.X - min.X;
                if (min.Y - WindowContentAreaMin.Y < 0)
                    bakus.Y += WindowContentAreaMin.Y - min.Y;
                if (max.X > WindowContentAreaMax.X)
                    bakus.X += max.X - WindowContentAreaMax.X;
                if (max.Y > WindowContentAreaMax.Y)
                    bakus.Y += max.Y - WindowContentAreaMax.Y;
                if (bakus != Vector2.Zero)
                    bakus += new Vector2(ImGui.GetStyle().ScrollbarSize);

                
                var funnyOrthoViewport = (WindowContentAreaMax - WindowContentAreaMin) / 2 / viewZoom;
                ImGuizmoNET.ImGuizmo.SetOrthographic(true);
                var projMat = Matrix4x4Extension.Ortho(-funnyOrthoViewport.X,
                                                       funnyOrthoViewport.X,
                                                       -funnyOrthoViewport.Y,
                                                       funnyOrthoViewport.Y,
                                                       -1, 1);
                var viewMat = Matrix4x4.CreateTranslation(new Vector3(viewPos.X, -viewPos.Y, 0));
                var modeMat = Matrix4x4.CreateScale(new Vector3(1, 1, 0));
                var identity = Matrix4x4.Identity;
                ImGuizmo.SetRect(WindowContentAreaMin.X,
                                    WindowContentAreaMin.Y,
                                    WindowContentAreaMax.X - WindowContentAreaMin.X,
                                    WindowContentAreaMax.Y - WindowContentAreaMin.Y);
                ImGuizmo.SetDrawlist();
                ImGuizmo.Manipulate(ref viewMat.M11, ref projMat.M11, ImGuizmoNET.OPERATION.TRANSLATE, ImGuizmoNET.MODE.WORLD, ref modeMat.M11);

                //if (ImGui.hovered())
                //{
                //var io = ImGui.GetIO();
                //float my_tex_w = SelectedTextureAsset.Texture.Width;
                //float my_tex_h = SelectedTextureAsset.Texture.Height;
                //Vector2 pos = ImGui.GetCursorScreenPos();
                //ImGui.BeginTooltip();
                //float region_sz = 32.0f;
                //float region_x = io.MousePos.X - pos.X - region_sz * 0.5f;
                //float region_y = io.MousePos.Y - pos.Y - region_sz * 0.5f;
                //float zoom = 4.0f;
                //if (region_x < 0.0f) { region_x = 0.0f; }
                //else if (region_x > my_tex_w - region_sz) { region_x = my_tex_w - region_sz; }
                //if (region_y < 0.0f) { region_y = 0.0f; }
                //else if (region_y > my_tex_h - region_sz) { region_y = my_tex_h - region_sz; }
                //ImGui.Text($"Min: ({region_x:F2}, {region_y:F2})");
                //ImGui.Text($"Max: ({region_x + region_sz:F2}, {region_y + region_sz:F2})");
                //Vector2 uv0 = new Vector2((region_x) / my_tex_w, (region_y) / my_tex_h);
                //Vector2 uv1 = new Vector2((region_x + region_sz) / my_tex_w, (region_y + region_sz) / my_tex_h);
                //ImGui.Image(new IntPtr(SelectedTextureAsset.Texture.ID), new Vector2(region_sz * zoom, region_sz * zoom), uv0, uv1);
                //ImGui.EndTooltip();
                //}

                //if (viewPos.X != ImGui.GetScrollX() + WindowSize.X / 2 || viewPos.Y != ImGui.GetScrollY() + WindowSize.Y / 2)
                //{
                //    viewPos.X = ImGui.GetScrollX() + WindowSize.X / 2;
                //    viewPos.Y = ImGui.GetScrollY() + WindowSize.Y / 2;
                //}
            }
        }

        public override void OnOpen()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                selectedBoi = TextureLoader.LoadTexture(CrossEngine.Properties.Resources.DefaultWindowIcon.ToBitmap());
            });
        }

        public override void OnClose()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                selectedBoi.Value.Dispose();
                selectedBoi = null;
            });
        }

        private void ResetView()
        {
            viewPos = Vector2.Zero;
            viewZoom = 1f;
        }

        private void LoadAsset()
        {
            if (!Dialog.FileOpen(out string path,
                filters: Dialog.Filters.ImageFiles))
                return;

            textureAssets = Context.Scene.AssetRegistry.GetCollection<TextureAsset>();

            selectedTextureAsset = new TextureAsset();
            selectedTextureAsset.RelativePath = path;
            textureAssets.AddAsset(selectedTextureAsset);

            selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(textureAssets.Count - 1, 0));

            ResetView();
        }

        private void UnloadAsset()
        {
            textureAssets.RemoveAsset(selectedTextureAsset);
            selectedTextureAsset.Unload();
            selectedTextureAsset = null;

            selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(textureAssets.Count - 1, 0));

            ResetView();
        }
    }
}
