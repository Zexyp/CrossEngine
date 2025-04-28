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
    public interface IEditorContext
    {
        Scene? Scene { get; }
        Entity? ActiveEntity { get; set; }
        AssetList? Assets { get; set; }
    }

    class EditorContext : IEditorContext
    {
        public Scene Scene
        {
            get => _scene;
            set
            {
                if (value == _scene) return;
                var old = _scene;
                _scene = value;

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
        public AssetList Assets
        {
            get => _assets;
            set
            {
                if (value == _assets) return;
                var old = _assets;
                _assets = value;

                AssetsChanged?.Invoke(old);
            }
        }

        private Entity _activeEntity = null;
        private Scene _scene = null;
        private AssetList _assets = null;
        // private GraphicsContext Graphics;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only and only one
        public event Action<Entity> ActiveEntityChanged;
        public event Action<Scene> SceneChanged;
        public event Action<AssetList> AssetsChanged;

        public void Clear()
        {
            ActiveEntity = null;
            Scene = null;
            Assets = null;
        }
    }
}