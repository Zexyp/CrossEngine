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
using CrossEngine.Core.Services;
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

        public override void OnInit()
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
        
        public Task Push(Scene scene)
        {
            return PushBackground(scene).ContinueWith(t => AttachRendering(scene)).Unwrap();
        }

        public Task PushBackground(Scene scene)
        {
            Debug.Assert(!_scenes.Contains(scene));
            
            Log.Debug("scene push started");
            
            return PrepareRendering(scene)
            .ContinueWith(t =>
            {
                scene.Init();
            
                _scenes.Add(scene);
            
                Log.Info("scene pushed");
            });
        }

        public Task Remove(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            
            Log.Debug("scene remove started");
            
            _scenes.Remove(scene);

            return FinishRendering(scene).ContinueWith(t =>
            {
                scene.Deinit();
                
                var task = DetachRendering(scene);
                
                Log.Info("scene removed");
                
                return task;
            }).Unwrap();
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
        public Task<TResult> Execute<TResult>(Func<TResult> func) => _scheduler.Schedule(func);
        public TaskScheduler GetScheduler() => _scheduler;

        public void OnRender(ISurface surface)
        {
            for (int i = 0; i < _drawScenes.Count; i++)
            {
                SceneRenderer.Render(_drawScenes[i], surface);
            }
        }
        
        // TODO: order of operations in detach and attach and prepare and finish is inconsistent: ServiceRequest
        
        private Task AttachRendering(Scene scene)
        {
            var rs = scene.World.GetSystem<RenderSystem>();
            var service = Manager.GetService<RenderService>();
            
            return service.Execute(() =>
            {
                var sur = (ISurface)Manager.GetService<RenderService>().MainSurface;
                
                sur.Resize += rs.OnSurfaceResize;
                rs.OnSurfaceResize(sur, sur.Width, sur.Height);
                
            }).ContinueWith(t => _drawScenes.Add(scene));
        }

        private Task DetachRendering(Scene scene)
        {
            _drawScenes.Remove(scene);
            
            var rs = scene.World.GetSystem<RenderSystem>();
            var service = Manager.GetService<RenderService>();
            
            return service.Execute(() =>
            {
                var sur = Manager.GetService<RenderService>().MainSurface;
                sur.Resize -= rs.OnSurfaceResize;

                rs.RendererRequest = null;
            });
        }

        private Task PrepareRendering(Scene scene)
        {
            var rs = scene.World.GetSystem<RenderSystem>();
            var service = Manager.GetService<RenderService>();

            return service.Execute(() =>
            {
                rs.RendererRequest = service.Execute;
                rs.GraphicsInit();
            });
        }
        
        private Task FinishRendering(Scene scene)
        {
            var rs = scene.World.GetSystem<RenderSystem>();
            var service = Manager.GetService<RenderService>();

            return service.Execute(() =>
            {
                rs.GraphicsDestroy();
            });
        }
    }
}
