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
        }

        public override void OnInit()
        {
            base.OnInit();

            Manager.GetService<RenderService>().MainSurface.Update += OnRender;
        }

        public override async void OnStart()
        {
            base.OnStart();

            var entity = scene.CreateEntity();
            entity.AddComponent(new SpriteRendererComponent());
            entity = scene.CreateEntity();
            entity.AddComponent(new CameraComponent()).Primary = true;

            await SceneManager.Push(scene);
            await SceneManager.AttachRenderer(scene, new SceneRenderer() {Pipeline = new DeferredPipeline()});
        }

        private void OnRender(ISurface surface)
        {
            
        }
    }
}
