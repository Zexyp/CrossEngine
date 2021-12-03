using System;
using System.Collections.Generic;

using CrossEngine.Scenes;
using CrossEngine.Entities;

namespace CrossEngineEditor
{
    public class EditorContext
    {
        private Entity _activeEntity = null;
        private Scene _scene = null;

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

        public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only
        public event Action OnActiveEntityChanged;
        public event Action OnSceneChanged;
    }
}
