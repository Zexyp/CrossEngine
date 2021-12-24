using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public class SpriteAsset : Asset
    {
        public override bool IsLoaded { get => Texture.IsLoaded; }

        public string TextureAssetName;
        public TextureAsset Texture;
        // cw
        public Vector2[] TexCoords = new Vector2[4] {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
        };

        public SpriteAsset()
        {
            throw new NotImplementedException();
        }

        public SpriteAsset(TextureAsset texture)
        {
            throw new NotImplementedException();

            Texture = texture;
            Name = System.IO.Path.GetFileNameWithoutExtension(texture.Path);
        }

        public override void Load()
        {
            Texture = ParentPool.Get<TextureAsset>(TextureAssetName);
        }

        public override void Unload()
        {
            Texture = null;
        }

        public override void OnSerialize(SerializationInfo info)
        {
            base.OnSerialize(info);
            info.AddValue("TexCoords", TexCoords);
            info.AddValue("TextureAssetName", TextureAssetName);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            base.OnDeserialize(info);
            TexCoords = info.GetValue<Vector2[]>("TexCoords");
            TextureAssetName = info.GetValue<string>("TextureAssetName");
        }
    }
}
