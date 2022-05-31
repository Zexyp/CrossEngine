using System;
using System.Collections.Generic;

using CrossEngine.Scenes;
using CrossEngine.ECS;

namespace CrossEngineEditor
{
    public class EditorContext
    {
        public bool Playmode = false;
        public Scene Scene
        {
            get => _scene;
            set
            {
                if (value == _scene) return;
                _scene = value;
                OnSceneChanged?.Invoke();
            }
        }
        public Entity ActiveEntity
        {
            get => _activeEntity;
            set
            {
                if (value == _activeEntity) return;
                var before = _activeEntity;
                _activeEntity = value;
                OnActiveEntityChanged?.Invoke();
            }
        }

        private Entity _activeEntity = null;
        private Scene _scene = null;

        // will we ever get here??
        public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only and only one
        public event Action OnActiveEntityChanged;
        public event Action OnSceneChanged;
    }
}
