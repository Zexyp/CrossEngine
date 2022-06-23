using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CrossEngine.Scenes;
using CrossEngine.ECS;
using CrossEngine.Utils;

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

        private Entity _activeEntity = null;
        private Scene _scene = null;

        // will we ever get here??
        //public readonly List<Entity> SelectedEntities = new List<Entity>();

        // no sender parameter since editor context is read-only and only one
        public event Action OnActiveEntityChanged;
        public event Action OnSceneChanged;
    }

    public class EditorProject
    {
        public string DescriptorPath;
        public string Dir;
        public string AssembliesDir;
        public string ScenesDir;
        public string EntryScene;
    
        public static EditorProject Create(string inipath)
        {
            var project = new EditorProject();
            
            project.DescriptorPath = inipath;
            project.Dir = Path.GetDirectoryName(inipath);

            project.AssembliesDir = Path.Combine(project.Dir, "./assemblies");
            project.ScenesDir = Path.Combine(project.Dir, "./scenes");

            project.Save();
            return project;
        }

        public void Save()
        {
            var ini = new IniFile(DescriptorPath, true);
            ini.Write("", "Assemblies", Path.GetRelativePath(Dir, AssembliesDir));
            ini.Write("", "Scenes", Path.GetRelativePath(Dir, ScenesDir));
            ini.Write("", "EntryScene", EntryScene);
            Directory.CreateDirectory(AssembliesDir);
            Directory.CreateDirectory(ScenesDir);
        }

        public Scene CreateScene(string name)
        {
            var scene = new Scene();
            var sceneDir = Path.Combine(ScenesDir, name);
            scene.AssetRegistry.HomeDirectory = sceneDir;
            Directory.CreateDirectory(sceneDir);
            return scene;
        }

        public void SaveScene(Scene scene)
        {
            EditorApplication.Log.Warn("lazy testing implementation!!!");
            SceneLoader.Write(scene, Path.Combine(scene.AssetRegistry.HomeDirectory, "scene.json"));
        }
    
        public static EditorProject Read(string inipath)
        {
            var project = new EditorProject();
            var ini = new IniFile(inipath, true);
            project.DescriptorPath = inipath;
            project.Dir = Path.GetDirectoryName(inipath);
            project.AssembliesDir = Path.Combine(project.Dir, ini.Read("", "Assemblies"));
            project.ScenesDir = Path.Combine(project.Dir, ini.Read("", "Scenes"));
            project.EntryScene = ini.Read("", "EntryScene");
            return project;
        }
        //
        //public static void Write(EditorProject project)
        //{
        //    var ini = new IniFile(project.Path, true);
        //    ini.Write("", "Scenes", System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(project.Scenes[0])));
        //    ini.Write("", "Assemblies", project.Assemblies);
        //}
    }
}
