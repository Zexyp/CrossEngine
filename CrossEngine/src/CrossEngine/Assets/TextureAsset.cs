using CrossEngine.Assets.Loaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class TextureAsset : Asset
    {
        public override bool Loaded => Texture?.GetValue() != null;

        public WeakReference<Texture> Texture = null;

        public string RelativePath;

        public override async void Load(IAssetLoadContext context)
        {
            using (Stream stream = await context.OpenStream(context.GetFullPath(RelativePath)))
            {
                Texture = TextureLoader.LoadTexture(stream);
            }
        }

        public override void Unload(IAssetLoadContext context)
        {
            Texture.GetValue().Dispose();
            Texture = null;
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
