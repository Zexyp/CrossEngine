using CrossEngine.Scenes;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class SceneAsset : FileAsset
    {
        public override bool Loaded => Scene != null;

        public Scene Scene { get; internal set; }

        public override async Task Load(IAssetLoadContext context)
        {
            using (Stream stream = await context.OpenRelativeStream(RelativePath))
            {
                Scene = SceneSerializer.DeserializeJson(stream);
            }
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Scene = null;
        }
    }
}
