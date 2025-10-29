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
        readonly Dictionary<Scene, SceneRenderer> _drawScenes = new();
        private RenderService _rs;

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
            _rs = Manager.GetService<RenderService>();
            
            Manager.GetService<TimeService>().FixedUpdate += OnFixedUpdate;
            Manager.GetService<RenderService>().MainSurface.Update += OnRender;
        }

        public override void OnDetach()
        {
            Manager.GetService<RenderService>().MainSurface.Update -= OnRender;
            Manager.GetService<TimeService>().FixedUpdate -= OnFixedUpdate;
            
            while (_scenes.Count > 0)
            {
                var scn = _scenes[0];
                if (scn.IsStarted)
                    Stop(scn);
                Remove(scn);
            }
            
            _rs = null;
        }

        public override void OnDestroy()
        {
            _scheduler.RunOnCurrentThread();

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
            Debug.Assert(!_scenes.Contains(scene));
            
            Log.Debug("scene push started");
            
            scene.Init();
            
            _scenes.Add(scene);
        }

        public void Remove(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            
            Log.Debug("scene remove started");
            
            scene.Deinit();
            
            if (_drawScenes.ContainsKey(scene))
                DetachRenderer(scene);
            _scenes.Remove(scene);
            Log.Info("scene removed");
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
            foreach (var pair in _drawScenes)
            {
                pair.Value.Render(pair.Key.World.GetSystem<RenderSystem>(), surface);
            }
        }
        
        public Task AttachRenderer(Scene scene, SceneRenderer renderer)
        {
            var rs = scene.World.GetSystem<RenderSystem>();
            
            return _rs.Execute(() =>
            {
                scene.World.GetSystem<RenderSystem>().Graphics = _rs.MainSurface.Context;
                renderer.Init();
                foreach (var rndrbl in renderer._renderables.Values)
                {
                    rndrbl.Init();
                }
            }).ContinueWith(t => _drawScenes.Add(scene, renderer));
        }

        public Task DetachRenderer(Scene scene)
        {
            var renderer = _drawScenes[scene];
            _drawScenes.Remove(scene);
            
            return _rs.Execute(() =>
            {
                foreach (var rndrbl in renderer._renderables.Values)
                {
                    rndrbl.Destroy();
                }
                renderer.Destroy();
                scene.World.GetSystem<RenderSystem>().Graphics = null;
            });
        }
    }
}
