using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossEngine.Scenes;
using CrossEngine.Ecs;
using CrossEngine.Utils;
using CrossEngine.Assets;

namespace CrossEngineEditor
{
    public interface IEditorContext
    {
        Scene? Scene { get; }
        Entity? ActiveEntity { get; set; }
        AssetList? Assets { get; }

        Task SetScene(Scene scene);
        Task SetAssets(AssetList assets);
    }

    class EditorContext : IEditorContext
    {
        public Scene? Scene => _scene;
        public AssetList? Assets => _assets;
        public Entity? ActiveEntity
        {
            get => _activeEntity;
            set
            {
                if (value == _activeEntity) return;
                var old = _activeEntity;
                _activeEntity = value;

                ActiveEntityChanged?.Invoke(old);
            }
        }

        // no sender parameter since editor context is read-only and only one
        public event Action<Entity> ActiveEntityChanged;
        public event Action<Scene> SceneChanged;
        public event Action<AssetList> AssetsChanged;
        
        private Entity _activeEntity = null;
        private Scene _scene = null;
        private AssetList _assets = null;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();

        private Action<string> block;
        private Action unblock;

        public EditorContext(Action<string> block, Action unblock)
        {
            this.block = block;
            this.unblock = unblock;
        }

        public Task SetScene(Scene scene)
        {
            block.Invoke("Loading Scene...");
            
            var old = _scene;

            _scene = null;

            var task = Task.CompletedTask;
            
            if (old != null)
                task = task.ContinueWith(t => SceneManager.Remove(old)).Unwrap();

            if (scene != null)
                task = task.ContinueWith(t => SceneManager.PushBackground(scene)).Unwrap();

            task = task.ContinueWith(t =>
            {
                _scene = scene;
                SceneChanged?.Invoke(old);
                
                unblock.Invoke();
            });

            return task;
        }

        public Task SetAssets(AssetList assets)
        {
            block.Invoke("Loading Assets...");

            var old = _assets;

            _assets = null;

            var task = Task.CompletedTask;

            task = SetScene(null);

            if (old?.IsLoaded == true)
                task = task.ContinueWith(t => AssetManager.Unload(old)).Unwrap();

            task = task.ContinueWith(t => AssetManager.Bind(assets));
                
            if (assets?.IsLoaded == false)
                task = task.ContinueWith(t => AssetManager.Load(assets)).Unwrap();
                
            task = task.ContinueWith(t =>
            {
                _assets = assets;
                AssetsChanged?.Invoke(old);
            });
            
            unblock.Invoke();

            return task;
        }

        public Task Clear()
        {
            ActiveEntity = null;
            var task = Task.CompletedTask;
            task = task.ContinueWith(t => SetScene(null)).Unwrap();
            task = task.ContinueWith(t => SetAssets(null)).Unwrap();
            return task;
        }
    }
}