using System;
using System.Linq;

using CrossEngine.Assets;
using CrossEngine.Core;
using CrossEngine.Debugging;
using CrossEngine.Events;
using CrossEngine.Scenes;
using CrossEngine.Services;

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
                WindowService.Mode.Sync
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

        public override void OnInit()
        {
            base.OnInit();

            AssetManager.Bind(AssetManager.ReadFile("./assets.json"));
            AssetManager.Load();

            var scenes = AssetManager.Current.GetCollection<SceneAsset>();
            if (scenes == null)
                throw new Exception("no scenes?");

            scene = scenes.First().Scene;
            SceneManager.Load(scene);
            SceneManager.Start(scene);
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