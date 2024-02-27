using System;
using System.Linq;

using CrossEngine.Assets;
using CrossEngine.Core;
using CrossEngine.Debugging;
using CrossEngine.Ecs;
using CrossEngine.Events;
using CrossEngine.Scenes;
using CrossEngine.Services;
using static System.Formats.Asn1.AsnWriter;

namespace CrossEngineRuntime
{
    public class RuntimeApplication : Application
    {
        public RuntimeApplication()
        {
            // register
            Manager.Register(new TimeService());
            Manager.Register(new InputService());
            Manager.Register(new WindowService(
#if WASM
                WindowService.Mode.Sync
#else
                WindowService.Mode.ThreadLoop
#endif
                ));
            Manager.Register(new RenderService(
#if WASM
                ) { IgnoreRefresh = true });
#else
                ));
#endif
#if WINDOWS
            Manager.Register(new CrossEngine.Utils.ImGui.ImGuiService());
#endif
            Manager.Register(new SceneService());
            Manager.Register(new AssetService());

            Manager.Register(new OverlayService());

            // configure
            Manager.GetService<WindowService>().WindowEvent += OnEvent;

#if WASM
            Manager.GetService<WindowService>().WindowEvent += (ws, e) =>
            {
                if (e is WindowRefreshEvent)
                    OnUpdate();
            };
#endif
        }

        Scene scene;

        public override async void OnInit()
        {
            base.OnInit();

            AssetManager.Bind(await AssetManager.ReadFile("./assets.json"));
            // do NOT ask me why
            AssetManager.LoadAsync().ContinueWith(t =>
            {
                var scenes = AssetManager.Current.GetCollection<SceneAsset>();
                if (scenes == null)
                    throw new Exception("no scenes?");

                var sceneAsset = scenes.First();
                if (!sceneAsset.Loaded)
                    throw new Exception("scene not loaded");

                scene = sceneAsset.Scene;

                var msg = "scene\n";
                foreach (var entity in scene.Entities)
                {
                    msg += $"    entity '{entity.Id}'\n";
                    foreach (var component in entity.Components)
                    {
                        msg += $"        component '{component.GetType().FullName}'\n";
                    }
                }
                Console.WriteLine(msg);

                SceneManager.Load(scene);
                SceneManager.Start(scene);
            });
        }

        public override void OnDestroy()
        {
            SceneManager.Stop(scene);
            SceneManager.Unload(scene);

            AssetManager.Unload();

            base.OnDestroy();
        }

        protected virtual void OnEvent(WindowService service, Event e)
        {
            if (e is WindowCloseEvent)
            {
                Close();
            }
        }
    }
}