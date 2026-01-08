using CrossEngine.Assets;
using CrossEngine.Components;
using CrossEngine.Core;
using CrossEngine.Debugging;
using CrossEngine.Display;
using CrossEngine.Inputs;
using CrossEngine.Loaders;
using CrossEngine.Rendering;
using CrossEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Examples
{
    class SimpleApp : Application
    {
        AssetList alist;
        Scene scene = new();

        public SimpleApp()
        {
            RenderService rs;

            Manager.Register(new TimeService());
            Manager.Register(new ConsoleInputService());
            Manager.Register(new WindowService(WindowService.Mode.Sync));
            Manager.Register(rs = new RenderService());
            Manager.Register(new InputService());
            Manager.Register(new OverlayService(new MetricsOverlay(rs)));
            Manager.Register(new SceneService());
            Manager.Register(new AssetService());
        }

        public override void OnInit()
        {
            base.OnInit();

            Manager.GetService<RenderService>().MainSurface.Update += OnRender;
        }

        public override async void OnStart()
        {
            base.OnStart();

            var texture = new TextureAsset() { RelativePath = "logo.png" };
            var atlas = new TextureAtlasAsset() { Texture = texture, TextureOffsets = [new(0, 0, 1, 1)] };
            var sprite = new SpriteAsset() { Atlas = atlas, OffsetIndex = 0 };
            alist = new AssetList();
            alist.Add(texture);
            alist.Add(atlas);
            alist.Add(sprite);
            var entity = scene.CreateEntity();
            entity.AddComponent(new SpriteRendererComponent() { Sprite = sprite, Blend = BlendMode.Blend });
            entity = scene.CreateEntity();
            entity.AddComponent(new OrthographicCameraComponent()).Primary = true;

            await AssetManager.Load(alist);
            await SceneManager.Push(scene);
            await SceneManager.AttachRenderer(scene, new SceneRenderer() {Pipeline = new DeferredPipeline()});
        }

        public override async void OnEnd()
        {
            base.OnEnd();
            
            await AssetManager.Unload(alist);
        }

        private void OnRender(ISurface surface)
        {
            
        }
    }
}
