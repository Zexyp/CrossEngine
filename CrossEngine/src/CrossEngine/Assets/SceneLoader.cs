using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CrossEngine.Serialization;
using CrossEngine.Serialization.Json;
using CrossEngine.Scenes;
using CrossEngine.Assemblies;
using CrossEngine.Logging;

namespace CrossEngine.Assets
{
    public struct SceneFileEnvironment : ISerializable
    {
        public string AssembliesList;
        public string Scene;

        public void OnDeserialize(SerializationInfo info)
        {
            AssembliesList = info.GetValue<string>("AssembliesList");
            Scene = info.GetValue<string>("Scenes");
        }

        public void OnSerialize(SerializationInfo info)
        {
            info.AddValue("AssembliesList", AssembliesList);
            info.AddValue("Scenes", Scene);
        }
    }

    public class SceneLoader
    {
        public static void Save(string path, Scene scene, SceneFileEnvironment sfe)
        {
            // save all other files
            File.WriteAllText(sfe.AssembliesList, String.Join("\n", AssemblyLoader.LoadedAssemblies.Select(pair => pair.Path)));
            File.WriteAllText(sfe.Scene, SceneSerializer.SertializeJson(scene));

            JsonSerializer serializer = new JsonSerializer(JsonSerializerSettings.Default);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(sfe, stream);
            }

            Log.Core.Info("saved scene");
        }

        public static Scene Load(string path)
        {
            SceneFileEnvironment sfe;

            JsonSerializer serializer = new JsonSerializer(JsonSerializerSettings.Default);
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                sfe = (SceneFileEnvironment)serializer.Deserialize(stream, typeof(SceneFileEnvironment));
            }

            string directory = Path.GetDirectoryName(path);

            {
                string[] assembliesPaths = Array.ConvertAll(File.ReadAllText(directory + sfe.AssembliesList)
                        .Trim('\n', ' ').Split("\n"),
                        str => str.Trim(' '))
                    .Where(str => !string.IsNullOrWhiteSpace(str))
                    .ToArray();
                for (int i = 0; i < assembliesPaths.Length; i++)
                {
                    AssemblyLoader.Load(assembliesPaths[i]);
                }
            }

            Scene scene;
            {
                scene = SceneSerializer.DeserializeJson(File.ReadAllText(directory + sfe.Scene));
            }

            Log.Core.Info("loaded scene");

            return scene;
        }
    }
}
