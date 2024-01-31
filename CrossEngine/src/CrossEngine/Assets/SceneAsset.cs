using CrossEngine.Scenes;
using CrossEngine.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    internal class SceneAsset : Asset
    {
        public override bool Loaded => Scene != null;

        public Scene Scene { get; internal set; }
        public string RelativePath;

        public override async void Load(IAssetLoadContext context)
        {
            using (Stream stream = await context.OpenRelativeStream(RelativePath))
            {
                Scene = SceneSerializer.Deserialize(stream);
            }
        }

        public override void Unload(IAssetLoadContext context)
        {
            Scene = null;
        }
    }
}
