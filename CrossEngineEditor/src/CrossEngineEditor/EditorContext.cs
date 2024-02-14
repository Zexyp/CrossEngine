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
                _scene = value;

                ActiveEntity = null;

                SceneChanged?.Invoke();
            }
        }
        public Entity ActiveEntity
        {
            get => _activeEntity;
            set
            {
                if (value == _activeEntity) return;
                _activeEntity = value;

                ActiveEntityChanged?.Invoke();
            }
        }
        AssetPool Assets;

        private Entity _activeEntity = null;
        private Scene _scene = null;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only and only one
        public event Action ActiveEntityChanged;
        public event Action SceneChanged;
    }
}