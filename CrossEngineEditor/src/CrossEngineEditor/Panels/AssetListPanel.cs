using CrossEngine.Assemblies;
using CrossEngine.Assets;
using CrossEngine.Logging;
using CrossEngine.Serialization;
using CrossEngine.Services;
using CrossEngine.Utils.Editor;
using CrossEngineEditor.Platform;
using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.UI;
using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CrossEngine.Scenes;

namespace CrossEngineEditor.Panels
{
    internal class AssetListPanel : EditorPanel
    {
        public AssetListPanel() : base("Asset List")
        {
            WindowFlags |= ImGuiWindowFlags.MenuBar;

            typeSelect = new TypeSelectPopup(typeof(Asset)) { Callback = (sender, type) => { Context.Assets.Add((Asset)Activator.CreateInstance(type)); } };
        }

        TypeSelectPopup typeSelect;

        protected override void DrawWindowContent()
        {
            var openCreatePopup = false;

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        void CreateAssets() => Context.Assets = new AssetList();

                        EditorApplication.Service.DestructiveDialog(CreateAssets, Context.Assets != null);
                    }
                    if (ImGui.MenuItem("Load..."))
                    {
                        void LoadAssets()
                        {
                            EditorApplication.Service.DialogFileOpen().ContinueWith(t =>
                            {
                                var filepath = t.Result;
                                if (filepath != null)
                                {
                                    var task = AssetManager.ReadFile(filepath);
                                    Context.Assets = task.Result;
                                }
                            });
                        }
                        EditorApplication.Service.DestructiveDialog(LoadAssets, Context.Assets != null);
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Save As...", Context.Assets != null))
                    {
                        EditorApplication.Service.DialogFileSave().ContinueWith(t =>
                        {
                            var filepath = t.Result;
                            if (filepath != null)
                                AssetManager.WriteFile(Context.Assets, filepath);
                        });
                    }
                    if (ImGui.MenuItem("Dump", Context.Assets != null))
                    {
                        using (var stream = Console.OpenStandardOutput())
                            AssetManager.Write(Context.Assets, stream);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Asset", Context.Assets != null))
                {
                    if (ImGui.MenuItem("Create..."))
                    {
                        openCreatePopup = true;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Reload All", Context.Assets != null))
                    {
                        void ReloadAssets()
                        {
                            var last = Context.Assets;
                            Context.Assets = null;
                            Context.Assets = last;
                        }
                        EditorApplication.Service.DestructiveDialog(ReloadAssets);
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            if (openCreatePopup)
                typeSelect.Open();

            typeSelect.Draw();

            if (Context.Assets == null)
                return;

            InspectDrawer.Inspect(Context.Assets);

            if (ImGui.BeginTable("Assets", 2, ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoBordersInBody))
            {
                ImGui.TableSetupColumn("Identifier", ImGuiTableColumnFlags.NoHide);
                ImGui.TableSetupColumn("Data", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableHeadersRow();

                foreach ((Type Type, IEnumerable<Asset> EnumerateMore) item in Context.Assets.Enumerate())
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var treeOpen = ImGui.TreeNodeEx($"{item.Type.FullName}[]");
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled("-");

                    if (treeOpen)
                    {
                        foreach (var asset in item.EnumerateMore)
                        {
                            ImGui.PushID(asset.GetHashCode());

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            
                            if (!asset.Loaded) ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000ff);
                            var innerOpen = ImGui.TreeNodeEx(asset.Id.ToString());
                            if (!asset.Loaded) ImGui.PopStyleColor();
                            
                            ImGui.TableNextColumn();
                            if (innerOpen)
                            {
                                if (ImGui.Button("×"))
                                    Context.Assets.Remove(asset);
                                ImGui.BeginDisabled();
                                var loaded = asset.Loaded;
                                ImGui.Checkbox("Loaded", ref loaded);
                                ImGui.EndDisabled();
                                ImGui.SameLine();
                                if (ImGui.Button("Reload"))
                                {
                                    if (asset.Loaded) Context.Assets.UnloadAsset(asset);
                                    if (!asset.Loaded) Context.Assets.LoadAsset(asset);
                                }
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
    }
}
