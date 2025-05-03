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

        public static Task Push(Scene scene)
        {
            return service.Execute(() => service.Push(scene)).Unwrap();
        }

        public static Task PushBackground(Scene scene)
        {
            return service.Execute(() => service.PushBackground(scene)).Unwrap();
        }

        public static Task Remove(Scene scene)
        {
            return service.Execute(() => service.Remove(scene)).Unwrap();
        }
        
        public static Task Start(Scene scene) => service.Execute(() => service.Start(scene));
        public static Task Stop(Scene scene) => service.Execute(() => service.Stop(scene));
    }
}
