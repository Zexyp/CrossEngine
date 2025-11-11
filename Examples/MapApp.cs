using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Numerics;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Debugging;
using CrossEngine.Display;
using CrossEngine.Inputs;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Utils.ImGui;
using CrossEngine.Utils.Rendering;
using CrossEngine.Utils.Structs;
using ImGuiNET;
using Mapper;

namespace Examples;

public class MapApp : Application
{
    AssetList alist;
    SceneRenderer renderer;
    FreeCamera camera = new FreeCamera() {Position = Vector3.UnitZ * 10};
    RenderData rdata;
    private Logger mapLog = new Logger("MapMess");

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
            
            LineRenderer.DrawAxes(Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(camera.Rotation)) * Matrix4x4.CreateScale(new Vector3(size, -size, 1)) * Matrix4x4.CreateTranslation(new (Size.X - size * 2, Size.Y - size * 2, 0)));
        }
    }

    class RenderData : ISceneRenderData
    {
        public IList<IObjectRenderData> Objects { get; set; }
        public IList<ILightRenderData> Lights { get; set; } = [];
        public ISkyboxRenderData Skybox { get; set; }
        public ICamera Camera { get; set; }
        public Vector4? ClearColor { get; } = VecColor.Gray;
        
        public Vector2 ViewportSize
        {
            get => throw new Exception();
            set
            {
                if (Camera is IResizableCamera rescam)
                    rescam.Resize(value.X, value.Y);
            }
        }
        
        public class Spinner : ISpriteRenderData
        {
            public Matrix4x4 Transform => Matrix4x4.CreateRotationZ(rotation);
            public bool IsVisible { get; set; } = true;
            public IVolume GetVolume() => null;
            public BlendMode Blend => BlendMode.Blend;

            public Vector4 Color => VecColor.White;

            public float rotation;
        }

        public Spinner spinner = new Spinner();
        
        public RenderData()
        {
            Objects = [spinner];
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
                    lastError += $"{m}\n";
                    break;
                case Mapper.Log.Level.Communication:
                default:
                    mapLog.Info(m);
                    break;
            }
        });
        
        base.OnInit();
        
        rdata = new RenderData();
        rdata.Camera = camera;
    }

    public override async void OnStart()
    {
        base.OnStart();

        alist = new AssetList();
        await AssetManager.Load(alist);
        
        var rs = Manager.GetService<RenderService>();
        rs.Execute(() =>
        {
            renderer = new SceneRenderer() { Pipeline = new DeferredPipeline() };
            renderer.Init();
            rs.MainSurface.Update += OnRender;
        });
    }

    public override async void OnEnd()
    {
        base.OnEnd();
        
        Manager.GetService<RenderService>().Execute(() =>
        {
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
                    camera.Rotation = Quaternion.Identity;
                    camera.LookRot = Vector2.Zero;
                    camera.Position = Vector3.UnitZ * 10;
                    camera.Speed = 1;
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
            if (block) ImGui.BeginDisabled();
            
            ImGui.InputText("Config URL", ref configUrl, 255);
            if (ImGui.Button("Load"))
            {
                lastError = null;
                mapSurf = null;
                downloadTask = Task.Run(() =>
                {
                    return mapCfg = MapDowloader.GetConfig(configUrl);
                }).ContinueWith(t => downloadTask = null);
            }
            
            if (block) ImGui.EndDisabled();
            
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

            if (ImGui.Button("Download"))
            {
                lastError = null;
                downloadTask = Task.Run(() =>
                {
                    MapDowloader.GetMesh(lastCfg, mapSurf, lod, tileIdx.X, tileIdx.Y);
                    MapDowloader.GetMapTexture(lastCfg, mapSurf, lod, tileIdx.X, tileIdx.Y);
                }).ContinueWith(t => downloadTask = null);
            }
            
            if (lastCfg == null || mapSurf == null || block) ImGui.EndDisabled();
            
            ImGui.Separator();

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
}