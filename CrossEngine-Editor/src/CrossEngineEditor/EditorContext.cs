using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CrossEngine.Scenes;
using CrossEngine.ECS;
using CrossEngine.Utils;

using CrossEngineEditor.UndoRedo;

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

                ActiveEntity = null;

                Operations = _scene == null ? null : new OperationStack();
                OnSceneChanged?.Invoke();
            }
        }
        public Entity ActiveEntity
        {
            get => _activeEntity;
            set
            {
                if (value == _activeEntity) return;
                _activeEntity = value;

                OnActiveEntityChanged?.Invoke();
            }
        }
        public EditorProject Project;
        public OperationStack Operations { get; private set; }

        // no sender parameter since editor context is read-only and only one
        public event Action OnActiveEntityChanged;
        public event Action OnSceneChanged;

        private Entity _activeEntity = null;
        private Scene _scene = null;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();
    }
}
