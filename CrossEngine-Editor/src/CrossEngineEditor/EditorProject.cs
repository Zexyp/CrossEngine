using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using CrossEngine.Utils;
using CrossEngine.Scenes;
using CrossEngine;

namespace CrossEngineEditor
{
    using IODirectory = System.IO.Directory;

    public class EditorProject
    {
        public string Directory;
        public string EntryScene;
        public readonly ReadOnlyCollection<string> SceneNames;
        
        private IniFile _ini;
        private readonly List<string> _sceneNames = new List<string>();

        private EditorProject(string directory)
        {
            Directory = directory;
            _ini = new IniFile(Path.Combine(Directory, "project"));
            SceneNames = _sceneNames.AsReadOnly();
        }

        public static EditorProject Create(string directory)
        {
            var project = new EditorProject(directory);
            project.Write();
            return project;
        }

        public void Write()
        {
            IODirectory.CreateDirectory(Directory);
            IODirectory.CreateDirectory(Path.Combine(Directory, "scenes"));
            IODirectory.CreateDirectory(Path.Combine(Directory, "assemblies"));
            _ini.Write("", "EntryScene", EntryScene);
        }

        public Scene CreateScene(string name)
        {
            var scene = new Scene();
            var sceneDir = Path.Combine(Directory, "scenes", name);
            scene.AssetRegistry.HomeDirectory = Path.Combine(sceneDir, "assets");
            IODirectory.CreateDirectory(sceneDir);
            IODirectory.CreateDirectory(scene.AssetRegistry.HomeDirectory);
            _sceneNames.Add(name);
            SceneLoader.Write(scene, Path.Combine(sceneDir, "scene.json"));
            return scene;
        }

        public void WriteScene(Scene scene)
        {
            Application.Log.Warn("lazy testing implementation!!!");
            SceneLoader.Write(scene, Path.Combine(scene.AssetRegistry.HomeDirectory, "scene.json"));
        }

        public static EditorProject Read(string directory)
        {
            var project = new EditorProject(directory);
            project.EntryScene = project._ini.Read("", "EntryScene");
            project._sceneNames.AddRange(IODirectory.GetDirectories(Path.Combine(directory, "scenes")).Select(p => Path.GetFileName(p)));
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
