using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Scenes;
using CrossEngine.Systems;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    public class SceneService : Service, IUpdatedService, IScheduledService
    {
        public struct SceneConfig
        {
            public bool Update;
            public bool Render;
            public bool Resize;
        }

        readonly List<(Scene Scene, SceneConfig Config)> _scenes = new();

        readonly SingleThreadedTaskScheduler _scheduler = new SingleThreadedTaskScheduler();

        private WindowService ws;

        public override void OnStart()
        {
            SceneManager.service = this;

            _scheduler.RunOnCurrentThread();
        }

        public override void OnAttach()
        {
            ws = Manager.GetService<WindowService>();
            ws.Execute(() => { ws.Window.Event += OnWindowEvent; OnWindowResize(ws.Window.Width, ws.Window.Height); });
            Manager.GetService<TimeService>().FixedUpdate += OnFixedUpdate;
            Manager.GetService<RenderService>().Frame += OnRender;
        }

        private void OnWindowEvent(Event e)
        {
            if (e is WindowResizeEvent wre)
                OnWindowResize(wre.Width, wre.Height);
        }

        private void OnWindowResize(float width, float height)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                if (_scenes[i].Config.Resize)
                    _scenes[i].Scene.RenderData.PerformResize(width, height);
            }
        }

        public override void OnDetach()
        {
            Manager.GetService<RenderService>().Frame -= OnRender;
            Manager.GetService<TimeService>().FixedUpdate -= OnFixedUpdate;
            ws.Execute(() => { ws.Window.Event -= OnWindowEvent; ws = null; });
        }

        public override void OnDestroy()
        {
            _scheduler.RunOnCurrentThread();

            Debug.Assert(_scenes.Count == 0);
            SceneManager.service = null;
        }

        public ref SceneConfig GetConfig(Scene scene)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                if (scene == _scenes[i].Scene)
                    return ref Unsafe.AsRef(_scenes[i].Config);
            }

            throw new KeyNotFoundException();
        }

        public void Push(Scene scene, SceneConfig? config = null)
        {
            var configValue = config ?? default;
            if (configValue.Resize)
                scene.RenderData.PerformResize(ws.Window.Width, ws.Window.Height);
            _scenes.Add((scene, configValue));
            scene.Load();
        }

        public void Remove(Scene scene)
        {
            scene.Unload();
            _scenes.RemoveAll(e => e.Scene == scene);
        }

        

        public void OnUpdate()
        {
            _scheduler.RunOnCurrentThread();

            for (int i = 0; i < _scenes.Count; i++)
            {
                if (!_scenes[i].Config.Update)
                    continue;

                SceneManager.Current = _scenes[i].Scene;
                SceneManager.Current.Update();
                SceneManager.Current = null;
            }
        }

        private void OnFixedUpdate(TimeService obj)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                if (!_scenes[i].Config.Update)
                    continue;

                SceneManager.Current = _scenes[i].Scene;
                SceneManager.Current.FixedUpdate();
                SceneManager.Current = null;
            }
        }

        public void OnRender(RenderService rs)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                if (!_scenes[i].Config.Render)
                    continue;

                SceneRenderer.DrawScene(_scenes[i].Scene.RenderData, rs.RendererApi);
            }
        }

        public Task Execute(Action action) => _scheduler.Schedule(action);
        public TaskScheduler GetScheduler() => _scheduler;
    }
}
