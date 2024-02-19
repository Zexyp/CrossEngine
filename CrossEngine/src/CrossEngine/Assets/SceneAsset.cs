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
    [DependantAsset]
    public class SceneAsset : Asset
    {
        public override bool Loaded => Scene != null;

        public Scene Scene { get; internal set; }
        [EditorString]
        public string RelativePath;

        public override void Load(IAssetLoadContext context)
        {
            using (Stream stream = context.OpenRelativeStream(RelativePath))
            {
                Scene = SceneSerializer.DeserializeJson(stream);
            }
        }

        public override void Unload(IAssetLoadContext context)
        {
            Scene = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(RelativePath), RelativePath);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            RelativePath = info.GetValue(nameof(RelativePath), RelativePath);
        }
    }
}
