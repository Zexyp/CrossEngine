using CrossEngine.Assemblies;
using CrossEngine.Assets;
using CrossEngine.Utils.Editor;
using CrossEngineEditor.Utils;
using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class AssetListPanel : EditorPanel
    {
        JsonSerializerOptions jso;

        public AssetListPanel() : base("Asset List")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;


            jso = new JsonSerializerOptions();
            foreach (var item in CrossEngine.Serialization.SceneSerializer.BaseConverters)
            {
                jso.Converters.Add(item);
            }
        }

        protected override void DrawWindowContent()
        {
            const string createPopup = "Create Asset";
            var openCreatePopup = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        SetAssetPool(new AssetPool());
                    }
                    if (ImGui.MenuItem("Load..."))
                    {
                        var filepath = ShellFileDialogs.FileOpenDialog.ShowSingleSelectDialog(0, null, null, null, null, null);
                        if (filepath != null)
                            using (Stream stream = File.OpenRead(filepath))
                            {
                                SetAssetPool(JsonSerializer.Deserialize<AssetPool>(stream, jso));
                            }
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save", AssetManager.Current != null)) ;
                    if (ImGui.MenuItem("Save As...", AssetManager.Current != null))
                    {
                        var filepath = ShellFileDialogs.FileSaveDialog.ShowDialog(0, null, null, null, null);
                        if (filepath != null)
                            using (Stream stream = File.OpenWrite(filepath))
                            {
                                JsonSerializer.Serialize(stream, AssetManager.Current, jso);
                            }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Asset", AssetManager.Current != null))
                {
                    if (ImGui.MenuItem("Create..."))
                    {
                        openCreatePopup = true;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Reload All", AssetManager.Current != null))
                    {
                        AssetManager.Unload();
                        AssetManager.Load();
                        if (Context.Scene != null)
                        {
                            Context.Scene.Unload();
                            Context.Scene.Load();
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (openCreatePopup)
                ImGui.OpenPopup(createPopup);

            if (ImGui.BeginPopup(createPopup))
            {
                CreateAssetPopup();

                ImGui.EndPopup();
            }

            if (AssetManager.Current == null)
                return;

            InspectDrawer.Inspect(AssetManager.Current);

            if (ImGui.BeginTable("Assets", 2, ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoBordersInBody))
            {
                ImGui.TableSetupColumn("Identifier", ImGuiTableColumnFlags.NoHide);
                ImGui.TableSetupColumn("Data", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableHeadersRow();

                foreach ((Type Type, IEnumerable<Asset> EnumerateMore) item in AssetManager.Current.Enumerate())
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var treeOpen = ImGui.TreeNodeEx(item.Type.FullName);
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled("-");

                    if (treeOpen)
                    {
                        foreach (var asset in item.EnumerateMore)
                        {
                            ImGui.PushID(asset.GetHashCode());

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            var innerOpen = ImGui.TreeNodeEx(asset.Id.ToString());
                            ImGui.TableNextColumn();
                            if (innerOpen)
                            {

                                InspectDrawer.Inspect(asset);

                                ImGui.TreePop();
                            }
                            else
                                ImGui.TextDisabled("-");

                            ImGui.PopID();
                        }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndTable();
            }
        }

        private void SetAssetPool(AssetPool pool)
        {
            if (AssetManager.Current != null) AssetManager.Unload();

            AssetManager.Bind(pool);

            AssetManager.Load();
        }

        private void CreateAssetPopup()
        {
            // TODO: consider cashing this

            var type = typeof(Asset);
            foreach (var assembly in AssemblyManager.Loaded)
            {
                ImGui.SeparatorText(assembly.GetName().Name);

                var types = assembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    var t = types[i];

                    if (t.IsPublic && !t.IsAbstract && t.IsSubclassOf(type))
                    {
                        if (ImGui.Selectable(t.FullName))
                        {
                            AssetManager.Current.Add((Asset)Activator.CreateInstance(t));
                        }
                    }
                }
            }
        }
    }
}
