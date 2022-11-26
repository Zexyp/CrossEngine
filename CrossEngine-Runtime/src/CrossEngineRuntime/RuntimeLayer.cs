using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CrossEngine;
using CrossEngine.Layers;
using CrossEngine.Utils;
using CrossEngine.Assemblies;
using CrossEngine.Scenes;
using CrossEngine.Events;
using CrossEngine.Logging;

namespace CrossEngineRuntime
{
    class RuntimeLayer : Layer
    {
        protected override void Attach()
        {
            IniFile ini = new IniFile("ce.ini");
            var assembliesDir = ini["Assemblies"];
            var sceneDir = ini["Scenes"];

            try
            {
                string[] assemlies = Directory.GetFiles(assembliesDir);
                for (int i = 0; i < assemlies.Length; i++)
                {
                    AssemblyLoader.Load(assemlies[i]);
                }

                string[] scenes = Directory.GetDirectories(sceneDir);
                for (int i = 0; i < scenes.Length; i++)
                {
                    SceneManager.Add(scenes[i], Path.GetFileName(scenes[i]));
                }
            }
            catch (Exception ex)
            {
                Application.Log.Fatal("content structure is incorrect!\n" + ex.ToString());
                return;
            }

            string entry = ini["EntryScene"];
            if (!SceneManager.Load(entry))
                Application.Log.Fatal("entry scene not found!");
        }

        protected override void Detach()
        {
            SceneManager.End();

            AssemblyLoader.UnloadAll();
        }

        protected override void Update()
        {
            SceneManager.Update();
        }

        protected override void Render()
        {
            SceneManager.Render();
        }

        protected override void Event(Event e)
        {
            SceneManager.OnEvent(e);
        }
    }
}
