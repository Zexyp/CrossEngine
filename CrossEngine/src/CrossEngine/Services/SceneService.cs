using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Scenes;
using CrossEngine.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CrossEngine.Services
{
    class SceneService : Service, IUpdatedService, IQueuedService
    {
        readonly Queue<Action> _actions = new Queue<Action>();
        Scene _currentScene;

        public override void OnStart()
        {
            Manager.GetService<TimeService>().FixedUpdate += OnFixedUpdate;
            Manager.GetService<RenderService>().Frame += OnRender;
            SceneManager.service = this;
        }

        public override void OnDestroy()
        {
            Debug.Assert(_currentScene == null);
            SceneManager.service = null;
            Manager.GetService<RenderService>().Frame -= OnRender;
            Manager.GetService<TimeService>().FixedUpdate -= OnFixedUpdate;
        }

        public void Load(Scene scene)
        {
            _currentScene = scene;
            _currentScene.World.GetSystem<RenderSystem>().Window = Manager.GetService<WindowService>().Window;
            _currentScene.Load();
            _currentScene.Start();
        }

        public void Unload()
        {
            _currentScene.Unload();
            _currentScene.Stop();
            _currentScene.World.GetSystem<RenderSystem>().Window = null;
            _currentScene = null;
        }

        public void OnUpdate()
        {
            while (_actions.TryDequeue(out var action))
                action.Invoke();

            if (_currentScene != null)
                _currentScene.Update();
        }

        private void OnFixedUpdate(TimeService obj)
        {
            if (_currentScene != null)
                _currentScene.FixedUpdate();
        }

        public void OnRender(RenderService rs)
        {
            if (_currentScene != null)
                SceneRenderer.DrawScene(_currentScene.RenderData, rs.RendererApi);
        }

        public void Execute(Action action)
        {
            _actions.Enqueue(action);
        }
    }
}
