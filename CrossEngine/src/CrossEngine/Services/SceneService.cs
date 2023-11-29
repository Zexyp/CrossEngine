using CrossEngine.Rendering;
using CrossEngine.Scenes;
using CrossEngine.Systems;
using System;
using System.Collections.Generic;

namespace CrossEngine.Services
{
    class SceneService : Service, IUpdatedService, IMessagableService
    {
        readonly Queue<Action> _actions = new Queue<Action>();
        Scene _currentScene;

        public override void OnStart()
        {
            Manager.GetService<RenderService>().Frame += OnRender;
            SceneManager.service = this;
        }

        public override void OnDestroy()
        {
            SceneManager.service = null;
            Manager.GetService<RenderService>().Frame -= OnRender;
        }

        public void Load(Scene scene)
        {
            _currentScene = scene;
            _currentScene.Load();
            _currentScene.Start();
        }

        public void Unload()
        {
            _currentScene.Unload();
            _currentScene.Stop();
            _currentScene = null;
        }

        public void OnUpdate()
        {
            while (_actions.TryDequeue(out var action))
                action.Invoke();
            _currentScene.Update();
        }

        public void OnRender(RenderService rs)
        {
            SceneRenderer.DrawScene(_currentScene.RenderData, rs.RendererApi);
        }

        public void Execute(Action action)
        {
            _actions.Enqueue(action);
        }
    }
}
