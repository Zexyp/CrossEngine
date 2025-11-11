using System.Collections.Concurrent;
using System.Numerics;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Debugging;
using CrossEngine.Display;
using CrossEngine.Geometry;
using CrossEngine.Inputs;
using CrossEngine.Loaders;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Textures;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Utils.ImGui;
using CrossEngine.Utils.Rendering;
using CrossEngine.Utils.Structs;
using ImGuiNET;
using Mapper;
using Examples.Maps;

namespace Examples;

public class MapApp : Application
{
    AssetList alist;
    SceneRenderer renderer;
    FreeCamera camera = new FreeCamera() {Position = Vector3.UnitZ * 10};
    RenderData rdata;
    Logger mapLog = new Logger("MapMess");
    IMesh tileMesh;

    class AxesOverlay : HudOverlay
    {
        const float size = 32;
        FreeCamera camera;

        public AxesOverlay(FreeCamera camera)
        {
            this.camera = camera;
        }

        protected override void Content()
        {
            base.Content();
            
            LineRenderer.SetLineWidth(4);
            LineRenderer.DrawAxes(Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(camera.Rotation)) * Matrix4x4.CreateScale(new Vector3(size, -size, 1)) * Matrix4x4.CreateTranslation(new (Size.X - size * 2, Size.Y - size * 2, 0)));
        }
    }

    public MapApp()
    {
        RenderService rs;

        Manager.Register(new TimeService());
        Manager.Register(new ConsoleInputService());
        Manager.Register(new WindowService(WindowService.Mode.Sync));
        Manager.Register(rs = new RenderService());
        Manager.Register(new InputService());
        Manager.Register(new ImGuiService());
        Manager.Register(new OverlayService(new MetricsOverlay(rs), new AxesOverlay(camera)));
        Manager.Register(new AssetService());
    }

    public override void OnInit()
    {
        Mapper.Log.SetMessageCallback((m, l) =>
        {
            switch (l)
            {
                case Mapper.Log.Level.Error:
                    mapLog.Error(m);
                    if (lastError == null)
                        lastError = "";
                    lastError += $"Error: {m}\n";
                    break;
                case Mapper.Log.Level.Communication:
                default:
                    mapLog.Info(m);
                    break;
            }
        });
        
        base.OnInit();

        ResetCam();
        rdata = new RenderData();
        rdata.Camera = camera;
    }

    public override async void OnStart()
    {
        base.OnStart();

        var tex = new TextureAsset() {RelativePath = "logo.png"};
        alist = new AssetList();
        alist.Add(tex);
        await AssetManager.Load(alist);
        
        var rs = Manager.GetService<RenderService>();
        rs.Execute(() =>
        {
            renderer = new SceneRenderer() { Pipeline = new DeferredPipeline() };
            renderer.Init();
            rs.MainSurface.Update += OnRender;
            rdata.spinner.enabled = true;
            var sp = ShaderPreprocessor.CreateProgramFromString(@"
#type vertex
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat4 uViewProjection = mat4(1);
uniform mat4 uModel = mat4(1);
void main() {
    vTexCoord = aTexCoord;
    gl_Position = uViewProjection * uModel * vec4(aPosition, 1.0);
}

#type fragment
#version 330 core

in vec2 vTexCoord;

uniform sampler2D uTexture;

layout(location = 0) out vec4 oColor;
void main() {
    oColor = vec4(texture(uTexture, vTexCoord).xyz, 1); // gl_FragColor no workie in es
}
");
            rdata.tile.material = new DynamicMaterial(sp);
            
            tex.Texture.SetFilterParameter(FilterParameter.Nearest);
            rdata.spinner.texture = tex.Texture;
        });
    }

    public override async void OnEnd()
    {
        base.OnEnd();
        
        Manager.GetService<RenderService>().Execute(() =>
        {
            if (rdata.tile.material.Samplers.ContainsKey("uTexture"))
                rdata.tile.material.Samplers["uTexture"].Dispose();
            rdata.tile.material.Shader.Dispose();
            
            rdata.tile.renderer?.Dispose();
            renderer.Destroy();
            renderer = null;
        });
        
        await AssetManager.Unload(alist);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        var io = ImGui.GetIO();
        if (!io.WantCaptureKeyboard && !io.WantCaptureMouse)
            camera.Update();
        
        rdata.spinner.rotation += Time.DeltaF;
    }

    string configUrl = "https://mapserver-3d.mapy.cz/scenes/latest/mapConfig.json";
    Task downloadTask;
    MapConfig mapCfg;
    MapConfigSurface mapSurf;
    int lod = 19;
    IntVec2 tileIdx = new(142606, 88677);
    string lastError = null;
    private void OnRender(ISurface surface)
    {
        renderer.Render(rdata, surface);

        if (tileMesh != null)
        {
            surface.Context.Api.SetDepthFunc(DepthFunc.Default);
            LineRenderer.SetLineWidth(1);
            LineRenderer.BeginScene(((ICamera)camera).GetViewProjectionMatrix());
            LineRenderer.DrawBox(tileMesh.Bounds.Min, tileMesh.Bounds.Max, ColorHelper.U32ToVec4(0xffFFA500));
            LineRenderer.EndScene();
        }
        
        DrawGui();
    }

    bool helpOpen = false;
    private void DrawGui()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Reset"))
                {
                    ResetCam();
                }
                ImGui.EndMenu();
            }
            if (ImGui.MenuItem("Help", "", helpOpen))
            {
                helpOpen = !helpOpen;
            }
            
            ImGui.EndMainMenuBar();
        }

        if (ImGui.Begin("Controls"))
        {
            bool block = downloadTask != null;
            
            // ---- config
            
            if (block) ImGui.BeginDisabled();
            
            ImGui.InputText("Config URL", ref configUrl, 255);
            if (ImGuiUtil.FullWidthButton("Load"))
            {
                lastError = null;
                mapSurf = null;
                downloadTask = Task.Run(() =>
                {
                    return mapCfg = MapDowloader.GetConfig(configUrl);
                }).ContinueWith(t => downloadTask = null);
            }
            
            if (block) ImGui.EndDisabled();
            
            // ---- tile
            
            ImGui.Separator();
            
            var lastCfg = mapCfg;
            
            if (lastCfg == null || block) ImGui.BeginDisabled();

            if (ImGui.BeginCombo("Surface", mapSurf != null ? mapSurf.Id : ""))
            {
                for (int i = 0; i < lastCfg.Surfaces.Length; i++)
                {
                    var s = lastCfg.Surfaces[i];
                    bool isSelected = s == mapSurf;
                    if (ImGui.Selectable(s.Id, isSelected))
                    {
                        mapSurf = s;
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            
            if (mapSurf != null) ImGui.Text($"LOD range: {mapSurf.LodRange[0]} - {mapSurf.LodRange[1]}");
            ImGui.InputInt("LOD", ref lod);
            if (mapSurf != null) lod = Math.Clamp(lod, mapSurf.LodRange[0], mapSurf.LodRange[1]);
            
            if (mapSurf != null) ImGui.Text($"Tile ranges:\n    X: {mapSurf.TileRange[0][0]} - {mapSurf.TileRange[0][1]}\n    Y: {mapSurf.TileRange[1][0]} - {mapSurf.TileRange[1][0]}");
            ImGui.InputInt2("Tile", ref tileIdx.X);
            
            if (lastCfg == null || block) ImGui.EndDisabled();
            
            if (lastCfg == null || mapSurf == null || block) ImGui.BeginDisabled();

            if (ImGuiUtil.FullWidthButton("Download"))
            {
                LoadTile(lastCfg);
            }

            if (lastCfg == null || mapSurf == null || block) ImGui.EndDisabled();

            // ---- arrows
            
            ImGui.Separator();

            var dis = lastCfg == null || mapSurf == null || block;
            
            DisabledSquare();
            ImGui.SameLine();
            if (DisableableButon(dis, "#up", ImGuiDir.Up)) 
            {
                tileIdx.Y++;
                LoadTile(lastCfg);
            }
            ImGui.SameLine();
            DisabledSquare();

            if (DisableableButon(dis, "#left", ImGuiDir.Left)) 
            {
                tileIdx.X--;
                LoadTile(lastCfg);
            }
            ImGui.SameLine();
            DisabledSquare();
            ImGui.SameLine();
            if (DisableableButon(dis, "#right", ImGuiDir.Right)) 
            {
                tileIdx.X++;
                LoadTile(lastCfg);
            }
            
            DisabledSquare();
            ImGui.SameLine();
            if (DisableableButon(dis, "#down", ImGuiDir.Down)) 
            {
                tileIdx.Y--;
                LoadTile(lastCfg);
            }
            ImGui.SameLine();
            DisabledSquare();
            
            // ---- error
            
            ImGui.Separator();

            ImGui.Text(downloadTask != null ? "Working..." : "Ready");
            
            if (lastError != null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, VecColor.Red);
                ImGui.TextWrapped(lastError);
                ImGui.PopStyleColor();
            }
            
            ImGui.End();
        }
        
        if (helpOpen && ImGui.Begin("Help"))
        {
            ImGui.SeparatorText("Usage");
            ImGui.Text("1) Load a suitable config from url");
            ImGui.Text("2) Choose a surface");
            ImGui.Text("3) Download a tile (voids are present in tile ranges)");
            ImGui.SeparatorText("Viewport");
            ImGui.Text("Movement: WASD + QE");
            ImGui.Text("Look around: mouse left button + drag");
            ImGui.Text("Movement speed: mouse scroll wheel");
            
            ImGui.End();
        }
        
        
    }

    void DisabledSquare()
    {
        ImGui.BeginDisabled();
        ImGuiUtil.SquareButton("");
        ImGui.EndDisabled();
    }

    bool DisableableButon(bool disable, string label, ImGuiDir dir)
    {
        if (disable) ImGui.BeginDisabled();
        
        bool result = ImGui.ArrowButton(label, dir);
        
        if (disable) ImGui.EndDisabled();
        
        return result;
    }
    
    void LoadTile(MapConfig lastCfg)
    {
        lastError = null;
        
        rdata.spinner.enabled = true;
        rdata.tile.renderer?.Dispose();
        rdata.tile.renderer = null;
        tileMesh = null;
        
        downloadTask = Task.Run(() =>
                {
                    return (
                        MapDowloader.GetMesh(lastCfg, mapSurf, lod, tileIdx.X, tileIdx.Y),
                        MapDowloader.GetMapTexture(lastCfg, mapSurf, lod, tileIdx.X, tileIdx.Y)
                    );
                }).ContinueWith(new Action<Task<(Stream, Stream)>>(t =>
                {
                    Manager.GetService<RenderService>().Execute(() =>
                    {
                        var (streamMesh, streamTexture) = t.Result;
                        
                        bool SetMesh()
                        {
                            var mapMesh = MapParser.ParseMesh(streamMesh);
                            if (mapMesh == null)
                                return false;
                            var verts = new List<MapVert>();
                            var inds = new List<uint>();
                            for (int si = 0; si < mapMesh.Submeshes.Length; si++)
                            {
                                var submesh = mapMesh.Submeshes[si];
                                var typedVerts = ((UInt16[])submesh.Vertices);
                                var typedUVs = ((UInt16[])submesh.InternalUVs);
                                var bbmin = new Vector3((float)submesh.BBoxMin[0], (float)submesh.BBoxMin[1], (float)submesh.BBoxMin[2]);
                                var bbmax = new Vector3((float)submesh.BBoxMax[0], (float)submesh.BBoxMax[1], (float)submesh.BBoxMax[2]);
                                var diff = bbmax - bbmin;
                                for (int i = 0; i < typedVerts.Length / 3; i++)
                                {
                                    var pos = new Vector3(typedVerts[i * 3], typedVerts[i * 3 + 1], typedVerts[i * 3 + 2]) / ushort.MaxValue * diff - diff / 2;
                                    var uv = new Vector2(typedUVs[i * 2], typedUVs[i * 2 + 1]) / ushort.MaxValue;
                                    uv.Y = 1 - uv.Y;
                                    verts.Add(new MapVert() { Position = pos, UV = uv });
                                }

                                var typedInds = ((UInt16[])submesh.Indices);
                                for (int i = 0; i < typedInds.Length; i++)
                                {
                                    inds.Add(typedInds[i]);
                                }
                            }

                            tileMesh = new IndexedMesh<MapVert>(verts.ToArray(), inds.ToArray());
                            var meshr = new MeshRenderer();
                            meshr.Setup(tileMesh);
                        
                            rdata.tile.renderer = meshr;
                            return true;
                        }

                        bool SetTexture()
                        {
                            if (rdata.tile.material.Samplers.ContainsKey("uTexture"))
                                rdata.tile.material.Samplers["uTexture"].Dispose();
                            
                            var tex = TextureLoader.LoadTextureFromStream(streamTexture);
                            rdata.tile.material.Samplers["uTexture"] = tex;
                            return true;
                        }
                        
                        rdata.spinner.enabled = !SetMesh() || !SetTexture();
                    });
                })).ContinueWith(t =>
                {
                    downloadTask = null;
                });
    }

    private void ResetCam()
    {
        camera.Rotation = Quaternion.Identity;
        camera.LookRot = Vector2.Zero;
        camera.Position = Vector3.UnitZ * 10;
        camera.Speed = 2;
        camera.Far = 1000;
    }

    struct MapVert : IVertex
    {
        public Vector3 Position;
        public Vector2 UV;
        
        Vector3 IVertex.Position => Position;
    }
}