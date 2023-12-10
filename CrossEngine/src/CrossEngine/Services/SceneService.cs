using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Scenes;
using CrossEngine.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CrossEngine.Services
{
    public class SceneService : Service, IUpdatedService, IQueuedService
    {
        readonly Queue<Action> _actions = new Queue<Action>();
        readonly List<Scene> _scenes = new List<Scene>();

        public override void OnStart()
        {
            SceneManager.service = this;

            ExecuteQueued();
        }

        public override void OnAttach()
        {
            Manager.GetService<TimeService>().FixedUpdate += OnFixedUpdate;
            Manager.GetService<RenderService>().Frame += OnRender;
        }

        public override void OnDetach()
        {
            Manager.GetService<RenderService>().Frame -= OnRender;
            Manager.GetService<TimeService>().FixedUpdate -= OnFixedUpdate;
        }

        public override void OnDestroy()
        {
            ExecuteQueued();

            Debug.Assert(_scenes.Count == 0);
            SceneManager.service = null;
        }

        public void Load(Scene scene)
        {
            _scenes.Add(scene);
            scene.World.GetSystem<RenderSystem>().Window = Manager.GetService<WindowService>().Window;
            scene.Load();
            scene.Start();
        }

        public void Unload(Scene scene)
        {
            scene.Unload();
            scene.Stop();
            scene.World.GetSystem<RenderSystem>().Window = null;
            _scenes.Remove(scene);
        }

        private void ExecuteQueued()
        {
            while (_actions.TryDequeue(out var action))
                action.Invoke();
        }

        public void OnUpdate()
        {
            ExecuteQueued();

            for (int i = 0; i < _scenes.Count; i++)
            {
                SceneManager.Current = _scenes[i];
                SceneManager.Current.Update();
                SceneManager.Current = null;
            }
        }

        private void OnFixedUpdate(TimeService obj)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                SceneManager.Current = _scenes[i];
                SceneManager.Current.FixedUpdate();
                SceneManager.Current = null;
            }
        }

        public void OnRender(RenderService rs)
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                SceneRenderer.DrawScene(_scenes[i].RenderData, rs.RendererApi);
            }
        }

        public void Execute(Action action)
        {
            _actions.Enqueue(action);
        }
    }
}
