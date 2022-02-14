using System;
using ImGuiNET;

using System.Numerics;
using System.Text;

using CrossEngine.Assets;
using CrossEngine.Scenes;
using CrossEngine.Logging;
using CrossEngine.Utils.Editor;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    class ImageViewerPanel : EditorPanel
    {
        public ImageViewerPanel() : base("Image Viewer")
        {
            this.WindowFlags |= ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.HorizontalScrollbar;
        }

        AssetCollection<TextureAsset> textureAssets = null;
        AssetCollection<TextureAsset> TextureAssets
        {
            set
            {
                textureAssets = value;
                SelectedTextureAsset = null;
            }
        }
        [EditorAssetValue(typeof(TextureAsset))]
        public TextureAsset SelectedTextureAsset = null;
        int selectedIndex = 0;
        Vector2 viewPos;
        float zoom = 1f;

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

        protected override void DrawWindowContent()
        {
            bool resetView = false;
            bool openRename = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Reset"))
                    {
                        resetView = true;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Image", Context.Scene != null))
                {
                    if (ImGui.MenuItem("Load"))
                    {
                        LoadAsset();
                    }
                    if (ImGui.MenuItem("Unload", SelectedTextureAsset != null))
                    {
                        UnloadAsset();
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Rename", SelectedTextureAsset != null))
                    {
                        openRename = true;
                    }

                    ImGui.EndMenu();
                }

                ImGui.PushItemWidth(256);
                PropertyDrawer.DrawEditorValue(this.GetType().GetField(nameof(SelectedTextureAsset)), this);
                ImGui.PopItemWidth();


                ImGui.EndMenuBar();
            }


            if (openRename) ImGui.OpenPopup("renameasset");

            if (ImGui.BeginPopup("renameasset"))
            {
                byte[] bytes = new byte[256];
                Encoding.UTF8.GetBytes(SelectedTextureAsset.Name).CopyTo(bytes, 0);
                ImGui.Text("Rename");
                if (ImGui.InputText("New name", bytes, (uint)bytes.Length)) ;
                {
                    // ! always trim end null bytes!!!
                    string text = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                    SelectedTextureAsset.Name = text;
                }

                ImGui.EndPopup();
            }


            if (SelectedTextureAsset != null)
            {
                //ImGui.SetCursorPos(WindowSize / 2 - currentTexture.Texture.Size / 2);

                var zoomedImageSize = SelectedTextureAsset.Texture.Size * zoom;

                if (zoomedImageSize.X < WindowSize.X && zoomedImageSize.Y < WindowSize.Y)
                    ImGui.SetCursorPos((WindowSize - zoomedImageSize) * 0.5f);

                ImGui.PushStyleColor(ImGuiCol.Button, 0xff000000);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xff000000);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xff000000);
                ImGui.ImageButton(new IntPtr(SelectedTextureAsset.Texture.ID), SelectedTextureAsset.Texture.Size * zoom, new Vector2(0, 1), new Vector2(1, 0), 0);
                ImGui.PopStyleColor(3);

                bool imageHovered = ImGui.IsItemHovered();

                if (resetView) ResetView();

                if (viewPos.X != ImGui.GetScrollX() + WindowSize.X / 2 || viewPos.Y != ImGui.GetScrollY() + WindowSize.Y / 2)
                {
                    viewPos.X = ImGui.GetScrollX() + WindowSize.X / 2;
                    viewPos.Y = ImGui.GetScrollY() + WindowSize.Y / 2;
                }

                if (imageHovered)
                {
                    MouseMove();

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
                }
                else
                {
                    // correct image pos
                    ImGui.SetScrollX(viewPos.X - WindowSize.X / 2);
                    ImGui.SetScrollY(viewPos.Y - WindowSize.Y / 2);
                }

                if (ImGui.IsWindowHovered())
                {
                    MouseZoom();
                }
            }
        }

        private void MouseMove()
        {
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Middle))
            {
                viewPos -= ImGui.GetMouseDragDelta();
                viewPos = Vector2.Clamp(viewPos, WindowSize * 0.5f / zoom, WindowSize * 2 * zoom);
            }
            if ((ImGui.IsMouseDragging(ImGuiMouseButton.Left) || ImGui.IsMouseDragging(ImGuiMouseButton.Middle)) && Focused)
            {
                viewPos = viewPos - ImGui.GetMouseDragDelta();
                ImGui.SetScrollX(viewPos.X - WindowSize.X / 2);
                ImGui.SetScrollY(viewPos.Y - WindowSize.Y / 2);
                ImGui.ResetMouseDragDelta();
            }
        }

        private void MouseZoom()
        {
            if (ImGui.GetIO().MouseWheel != 0.0f && ImGui.GetIO().KeyCtrl)
            {
                zoom += ImGui.GetIO().MouseWheel * 0.25f * zoom;
                zoom = Math.Clamp(zoom, 1.0f / 8, 10);
            }
        }

        private void ResetView()
        {
            if (SelectedTextureAsset != null)
            {
                viewPos = SelectedTextureAsset.Texture.Size / 2;
                zoom = 1.0f;
            }
        }

        private void LoadAsset()
        {
            if (!FileDialog.Open(out string path,
                filter:
                "All Image Files (*.bmp; *.jpg; *.jpeg; *.png; *.tif; *.tiff)\0*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff\0" +
                "PNG (*.png)\0*.png\0" +
                "JPG (*.jpg; *.jpeg)\0*.jpg;*.jpeg\0" +
                "BMP (*.bmp)\0*.bmp\0" +
                "GIF (*.gif)\0*.gif\0" +
                "TIFF (*.tif; *.tiff)\0*.tif;*.tiff\0" +
                "EXIF (*.exif)\0*.exif\0" +
                "All Files (*.*)\0*.*\0"))
                return;

            SelectedTextureAsset = new TextureAsset(path);
            textureAssets.Add(SelectedTextureAsset);

            selectedIndex = textureAssets.GetAll().Count - 1;
            selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(textureAssets.GetAll().Count - 1, 0));

            ResetView();
        }

        private void UnloadAsset()
        {
            textureAssets.Remove(SelectedTextureAsset);
            SelectedTextureAsset.Unload();
            SelectedTextureAsset = null;

            selectedIndex = textureAssets.GetAll().Count - 1;
            selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(textureAssets.GetAll().Count - 1, 0));

            ResetView();
        }

        public override void OnAttach()
        {
            Context.OnSceneChanged += OnContextSceneChanged;
            textureAssets = Context.Scene?.AssetPool.GetCollection<TextureAsset>();
        }
        public override void OnDetach()
        {
            Context.OnSceneChanged -= OnContextSceneChanged;
            textureAssets = null;
        }

        private void OnContextSceneChanged()
        {
            if (textureAssets != null) textureAssets.OnAssetAdded -= OnTextureAssetsAdded;
            TextureAssets = Context.Scene?.AssetPool.GetCollection<TextureAsset>();
            if (textureAssets != null) textureAssets.OnAssetAdded += OnTextureAssetsAdded;
        }

        public override void OnOpen()
        {
            if (textureAssets != null)
                textureAssets.OnAssetAdded += OnTextureAssetsAdded;
        }
        public override void OnClose()
        {
            if (textureAssets != null)
                textureAssets.OnAssetAdded -= OnTextureAssetsAdded;
        }

        private void OnTextureAssetsAdded(IAssetCollection collection, Asset asset)
        {
            
        }
    }
}
