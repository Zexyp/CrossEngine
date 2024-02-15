using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CrossEngine.Scenes;
using CrossEngine.Ecs;
using CrossEngine.Utils;
using CrossEngine.Assets;

namespace CrossEngineEditor
{
    public class EditorContext
    {
        public Scene Scene
        {
            get => _scene;
            set
            {
                if (value == _scene) return;
                var old = _scene;
                _scene = value;

                ActiveEntity = null;

                SceneChanged?.Invoke(old);
            }
        }
        public Entity ActiveEntity
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
        public AssetPool Assets
        {
            get => _assets;
            set
            {
                if (value == _assets) return;
                var old = _assets;
                _assets = value;

                Scene = null;

                AssetsChanged?.Invoke(old);
            }
        }

        private Entity _activeEntity = null;
        private Scene _scene = null;
        private AssetPool _assets = null;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only and only one
        public event Action<Entity> ActiveEntityChanged;
        public event Action<Scene> SceneChanged;
        public event Action<AssetPool> AssetsChanged;

        public void Clear()
        {
            ActiveEntity = null;
            Scene = null;
            Assets = null;
        }
    }
}