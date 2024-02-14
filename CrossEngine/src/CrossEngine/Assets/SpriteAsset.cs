using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class SpriteAsset : Asset
    {
        public override bool Loaded => Texture?.Loaded == true;

        [EditorDrag]
        public Vector4 TextureOffsets = new(0, 0, 1, 1);

        [EditorAsset]
        public TextureAsset Texture { get; set; }

        private Guid idTexture;

        public override void Load(IAssetLoadContext context)
        {
            Texture = context.LoadChild<TextureAsset>(Texture?.Id ?? idTexture);
        }

        public override void Unload(IAssetLoadContext context)
        {
            context.FreeChild(Texture);
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(TextureOffsets), TextureOffsets);
            info.AddValue(nameof(Texture), Texture?.Id);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            TextureOffsets = info.GetValue(nameof(TextureOffsets), TextureOffsets);
            idTexture = info.GetValue(nameof(Texture), idTexture);
        }
    }
}
