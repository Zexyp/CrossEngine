using System.Numerics;
using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Debugging;
using CrossEngine.Display;
using CrossEngine.Inputs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Utils.ImGui;
using CrossEngine.Utils.Rendering;
using ImGuiNET;
using Mapper;

namespace Examples;

public class MapApp : Application
{
    AssetList alist;
    SceneRenderer renderer;
    FreeCamera camera = new FreeCamera() {Position = Vector3.UnitZ * 10};
    RenderData rdata;

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
        Manager.Register(new OverlayService(new MetricsOverlay(rs)));
        Manager.Register(new SceneService());
        Manager.Register(new AssetService());
    }

    public override void OnInit()
    {
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
    private void OnRender(ISurface surface)
    {
        renderer.Render(rdata, surface);
        ImGui.Begin("yeet");
        ImGui.InputText("Config URL", ref configUrl, 255);
        if (ImGui.Button("chudli button"))
            MapDowloader.GetConfig(configUrl);
        ImGui.End();
    }
}