using CrossEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Scenes
{
    public static class SceneManager
    {
        internal static SceneService service;

        [ThreadStatic]
        static Scene _current;

        public static Scene Current { get => _current; internal set => _current = value; }

        public static void Load(Scene scene, SceneService.SceneConfig? config = null)
        {
            service.Execute(() => service.Push(scene, config ?? new SceneService.SceneConfig() { Render = true, Update = true, Resize = true }));
        }

        public static void Unload(Scene scene)
        {
            service.Execute(() => service.Remove(scene));
        }

        public static void Start(Scene scene)
        {
            service.Execute(() => service.Start(scene));
        }

        public static void Stop(Scene scene)
        {
            service.Execute(() => service.Stop(scene));
        }

        public static void Configure(Scene scene, SceneService.SceneConfig config)
        {
            service.Execute(() => service.SetConfig(scene, config));
        }
    }
}
