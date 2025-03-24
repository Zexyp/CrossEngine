using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Scenes;
using CrossEngine.Core;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CrossEngine.Display;
using CrossEngine.Services;
using CrossEngine.Components;
using static System.Formats.Asn1.AsnWriter;

namespace CrossEngine.Scenes
{
    public class SceneService : Service, IUpdatedService, IScheduledService
    {
        readonly List<Scene> _scenes = new();

        readonly SingleThreadedTaskScheduler _scheduler = new SingleThreadedTaskScheduler();

        static readonly internal Logger Log = new Logger("scene-service");

        public override void OnStart()
        {
            Debug.Assert(SceneManager.service == null);
            SceneManager.service = this;

            _scheduler.RunOnCurrentThread();
        }

        public override void OnAttach()
        {
            Manager.GetService<TimeService>().FixedUpdate += OnFixedUpdate;
        }

        public override void OnDetach()
        {
            Manager.GetService<TimeService>().FixedUpdate -= OnFixedUpdate;
        }

        public override void OnDestroy()
        {
            _scheduler.RunOnCurrentThread();

            while (_scenes.Count > 0)
            {
                var scn = _scenes[0];
                if (scn.Started)
                    Stop(scn);
                Remove(scn);
            }

            Debug.Assert(SceneManager.service == this);
            SceneManager.service = null;
        }

        public void Push(Scene scene)
        {
            _scenes.Add(scene);
            scene.Init();

            AttachRendering(scene);

            Log.Info("scene pushed");
        }

        public void Remove(Scene scene)
        {
            DetachRendering(scene);

            scene.Deinit();
            _scenes.Remove(scene);
            
            Log.Info("scene removed");
        }

        public void OnUpdate()
        {
            _scheduler.RunOnCurrentThread();

            for (int i = 0; i < _scenes.Count; i++)
            {
                if (!_scenes[i].Started)
                    continue;

                SceneManager.Current = _scenes[i];
                SceneManager.Current.Update();
                SceneManager.Current = null;
            }
        }

        public void OnFixedUpdate(TimeService obj)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                if (!_scenes[i].Started)
                    continue;

                SceneManager.Current = _scenes[i];
                SceneManager.Current.FixedUpdate();
                SceneManager.Current = null;
            }
        }

        public void Start(Scene scene)
        {
            SceneManager.Current = scene;
            SceneManager.Current.Start();
            SceneManager.Current = null;
        }

        public void Stop(Scene scene)
        {
            SceneManager.Current = scene;
            SceneManager.Current.Stop();
            SceneManager.Current = null;
        }

        public Task Execute(Action action) => _scheduler.Schedule(action);
        public TaskScheduler GetScheduler() => _scheduler;

        private void AttachRendering(Scene scene)
        {
            Manager.GetService<RenderService>().Execute(() =>
            {
                var rs = scene.World.GetSystem<RenderSystem>();
                rs.rapi = Manager.GetService<RenderService>().RendererApi;
                rs.SetSurface(Manager.GetService<RenderService>().MainSurface);
            });
        }

        private void DetachRendering(Scene scene)
        {
            Manager.GetService<RenderService>().Execute(() =>
            {
                var rs = scene.World.GetSystem<RenderSystem>();
                rs.SetSurface(null);
                rs.rapi = null;
            });
        }
    }
}
