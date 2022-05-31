using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using CrossEngine.Assemblies;
using CrossEngine.Serialization;
using CrossEngine.Events;
using CrossEngine.Rendering;

namespace CrossEngine.Scenes
{
    public static class SceneManager
    {
        static readonly Dictionary<string, string> _sceneDirs = new Dictionary<string, string>();
        static Scene _current;

        public static void Add(string path)
        {
            _sceneDirs.Add(Path.GetFileName(path), path);
        }

        public static void Remove(string path)
        {
            _sceneDirs.Remove(Path.GetFileName(path));
        }

        public static bool Load(string name)
        {
            if (_sceneDirs.ContainsKey(name))
            {
                var path = _sceneDirs[name];
                string json = File.ReadAllText(Path.Combine(path, "scene.json"));
                Load(SceneSerializer.DeserializeJson(json));
                return true;
            }
            return false;
        }

        public static bool Load(Scene scene)
        {
            _current?.Stop();
            _current?.Unload();
            SetCurrentScene(scene);
            _current.Load();
            _current.Start();
            return _current != null;
        }

        public static void Clear()
        {
            _current?.Stop();
            _current?.Unload();
            SetCurrentScene(null);
        }

        public static void Reset()
        {
            _sceneDirs.Clear();
        }

        public static void Update()
        {
            _current?.Update();
        }

        public static void Render()
        {
            if (_current != null)
                SceneRenderer.DrawScene(_current);
        }

        public static void OnEvent(Event e)
        {
            _current?.OnEvent(e);
        }

        public static void End()
        {
            if (_current == null) return;

            _current.Stop();
            _current.Unload();
        }

        static private void SetCurrentScene(Scene scene)
        {
            _current = scene;
            CrossEngine.Assets.AssetManager._ctx = _current?.AssetRegistry;
            //CrossEngine.Physics.PhysicsInterface.SetContext(scene.)
        }
    }
}
