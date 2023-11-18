using CrossEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Scenes
{
    internal static class SceneManager
    {
        internal static SceneService service;

        public static void Load(Scene scene)
        {
            service.Execute(() => service.Load(scene));
        }
    }
}
