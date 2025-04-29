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
        readonly List<Scene> _drawScenes = new();

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
            Manager.GetService<RenderService>().MainSurface.Update += OnRender;
        }

        public override void OnDetach()
        {
            Manager.GetService<RenderService>().MainSurface.Update -= OnRender;
            Manager.GetService<TimeService>().FixedUpdate -= OnFixedUpdate;
        }

        public override void OnDestroy()
        {
            _scheduler.RunOnCurrentThread();

            while (_scenes.Count > 0)
            {
                var scn = _scenes[0];
                if (scn.IsStarted)
                    Stop(scn);
                Remove(scn);
            }

            Debug.Assert(SceneManager.service == this);
            SceneManager.service = null;
        }

        public void OnUpdate()
        {
            _scheduler.RunOnCurrentThread();

            for (int i = 0; i < _scenes.Count; i++)
            {
                if (!_scenes[i].IsStarted)
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
                if (!_scenes[i].IsStarted)
                    continue;

                SceneManager.Current = _scenes[i];
                SceneManager.Current.FixedUpdate();
                SceneManager.Current = null;
            }
        }
        
        public void Push(Scene scene)
        {
            PushBackground(scene);

            AttachRendering(scene);
        }

        public void PushBackground(Scene scene)
        {
            lock (scene)
            {
                _scenes.Add(scene);
                scene.Init();

                PrepareRendering(scene);
                
                Log.Info("scene pushed");
            }
        }

        public void Remove(Scene scene)
        {
            lock (scene)
            {
                FinishRendering(scene);
            
                DetachRendering(scene);

                scene.Deinit();
                _scenes.Remove(scene);
            
                Log.Info("scene removed");
            }
        }

        public void Start(Scene scene)
        {
            SceneManager.Current = scene;
            SceneManager.Current.Start();
            SceneManager.Current = null;
            Log.Info("scene started");
        }

        public void Stop(Scene scene)
        {
            SceneManager.Current = scene;
            SceneManager.Current.Stop();
            SceneManager.Current = null;
            Log.Info("scene stopped");
        }

        public Task Execute(Action action) => _scheduler.Schedule(action);
        public TaskScheduler GetScheduler() => _scheduler;

        public void OnRender(ISurface surface)
        {
            for (int i = 0; i < _drawScenes.Count; i++)
            {
                SceneRenderer.Render(_drawScenes[i], surface);
            }
        }
        
        private void AttachRendering(Scene scene)
        {
            Manager.GetService<RenderService>().Execute(() =>
            {
                var rs = scene.World.GetSystem<RenderSystem>();
                var sur = (ISurface)Manager.GetService<RenderService>().MainSurface;
                
                sur.Resize += rs.OnSurfaceResize;
                rs.OnSurfaceResize(sur, sur.Width, sur.Height);
            }).ContinueWith(t => _drawScenes.Add(scene));
        }

        private void DetachRendering(Scene scene)
        {
            _drawScenes.Remove(scene);
            Manager.GetService<RenderService>().Execute(() =>
            {
                var rs = scene.World.GetSystem<RenderSystem>();
                var sur = Manager.GetService<RenderService>().MainSurface;
                
                sur.Resize -= rs.OnSurfaceResize;
            });
        }

        private void PrepareRendering(Scene scene)
        {
            Manager.GetService<RenderService>().Execute(() =>
            {
                var rs = scene.World.GetSystem<RenderSystem>();
                rs.GraphicsInit();
            });
        }
        
        private void FinishRendering(Scene scene)
        {
            Manager.GetService<RenderService>().Execute(() =>
            {
                var rs = scene.World.GetSystem<RenderSystem>();
                rs.GraphicsDestroy();
            });
        }
    }
}
