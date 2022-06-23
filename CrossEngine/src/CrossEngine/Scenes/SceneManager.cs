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
        public static Scene Current { get; private set; }

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
                Load(SceneLoader.Read(Path.Combine(path, "scene.json")));
                return true;
            }
            return false;
        }

        public static bool Load(Scene scene)
        {
            Current?.Stop();
            Current?.Unload();
            SetCurrentScene(scene);
            Current.Load();
            Current.Start();
            return Current != null;
        }

        public static void Clear()
        {
            Current?.Stop();
            Current?.Unload();
            SetCurrentScene(null);
        }

        public static void Reset()
        {
            _sceneDirs.Clear();
        }

        public static void Update()
        {
            Current?.Update();
        }

        public static void Render()
        {
            if (Current != null)
                SceneRenderer.DrawScene(Current);
        }

        public static void OnEvent(Event e)
        {
            Current?.OnEvent(e);
        }

        public static void End()
        {
            if (Current == null) return;

            Current.Stop();
            Current.Unload();
        }

        static private void SetCurrentScene(Scene scene)
        {
            Current = scene;
            CrossEngine.Assets.AssetManager._ctx = Current?.AssetRegistry;
            //CrossEngine.Physics.PhysicsInterface.SetContext(scene.)
        }
    }
}
