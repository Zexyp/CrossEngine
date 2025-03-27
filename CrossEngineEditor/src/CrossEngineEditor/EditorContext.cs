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
        public enum Playmode
        {
            None,
            Stopped,
            Playing,
            Paused,
        }

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
        public AssetList Assets
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
        public Playmode Mode
        {
            get => _mode;
            set
            {
                if (value == _mode) return;
                _mode = value;

                ModeChanged?.Invoke();
            }
        }

        private Entity _activeEntity = null;
        private Scene _scene = null;
        private AssetList _assets = null;
        private Playmode _mode = Playmode.None;
        // private GraphicsContext Graphics;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only and only one
        public event Action<Entity> ActiveEntityChanged;
        public event Action<Scene> SceneChanged;
        public event Action<AssetList> AssetsChanged;
        public event Action ModeChanged;

        public void Clear()
        {
            ActiveEntity = null;
            Scene = null;
            Assets = null;
        }
    }
}