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

        [EditorAsset]
        public TextureAsset Texture
        {
            get => texture;
            set => SetChildId(value, ref texture, ref idTexture);
        }

        [EditorDrag]
        public Vector4 TextureOffsets = new(0, 0, 1, 1);

        private TextureAsset texture = null;
        private Guid idTexture = Guid.Empty;

        public override void Load(IAssetLoadContext context)
        {
            context.LoadChild(idTexture, out texture);
        }

        public override void Unload(IAssetLoadContext context)
        {
            context.FreeChild(Texture);
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(TextureOffsets), TextureOffsets);
            info.AddValue(nameof(Texture), idTexture);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            TextureOffsets = info.GetValue(nameof(TextureOffsets), TextureOffsets);
            idTexture = info.GetValue(nameof(Texture), idTexture);
        }
    }
}
