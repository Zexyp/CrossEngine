using System.IO;

using CrossEngine.Serialization;

namespace CrossEngine.Scenes
{
    public static class SceneLoader
    {
        public static Scene Read(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                var scene = SceneSerializer.DeserializeJson(stream);
                scene.AssetRegistry.HomeDirectory = Path.GetDirectoryName(path);
                return scene;
            }
        }

        public static void Write(Scene scene, string path)
        {
            using (FileStream stream = File.OpenWrite(path))
            {
                SceneSerializer.SerializeJson(scene, stream);
            }
        }
    }
}
