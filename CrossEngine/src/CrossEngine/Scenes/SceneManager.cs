﻿using CrossEngine.Services;
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

        public static void Unload()
        {
            if (Current == null) new InvalidOperationException("Cannot use parameterless 'Unload' outside scene update.");

            Unload(Current);
        }

        public static void Unload(Scene scene)
        {
            service.Execute(() => service.Remove(scene));
        }
    }
}
