using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        public override void OnAttach()
        {
            IniFile ini = new IniFile("ce");
            var assembliesDir = ini.Read("", "Assemblies");
            var sceneDir = ini.Read("", "Scenes");

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
                    SceneManager.Add(scenes[i]);
                }
            }
            catch (Exception ex)
            {
                Log.App.Fatal("content structure is incorrect!\n" + ex.ToString());
                return;
            }

            string entry = ini.Read("", "EntryScene");
            if (!SceneManager.Load(entry))
                Log.App.Fatal("entry scene not found!");
        }

        public override void OnDetach()
        {
            SceneManager.End();

            AssemblyLoader.UnloadAll();
        }

        public override void OnUpdate()
        {
            SceneManager.Update();
        }

        public override void OnRender()
        {
            SceneManager.Render();
        }

        public override void OnEvent(Event e)
        {
            SceneManager.OnEvent(e);
        }
    }
}
