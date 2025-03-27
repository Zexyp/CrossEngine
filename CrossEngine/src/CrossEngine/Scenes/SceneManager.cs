using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Scenes
{
    public static class SceneManager
    {
        public static Scene Current { get => _current; internal set => _current = value; }

        internal static SceneService service;

        [ThreadStatic]
        private static Scene _current;

        public static void Push(Scene scene) => service.Execute(() => service.Push(scene));
        public static void PushBackground(Scene scene) => service.Execute(() => service.PushBackground(scene));
        public static void Remove(Scene scene) => service.Execute(() => service.Remove(scene));
        public static void Start(Scene scene) => service.Execute(() => service.Start(scene));
        public static void Stop(Scene scene) => service.Execute(() => service.Stop(scene));
    }
}
