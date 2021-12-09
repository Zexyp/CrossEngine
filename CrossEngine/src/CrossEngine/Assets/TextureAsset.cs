using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public class TextureAsset : Asset
    {
        public override bool IsLoaded { get; protected set; } = false;

        public Texture Texture;

        public TextureAsset()
        {

        }

        public TextureAsset(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public override void Load()
        {
            Texture = AssetLoader.LoadTexture(Path);
            IsLoaded = true;
        }

        public override void Unload()
        {
            Texture.Dispose();
            Texture = null;
            IsLoaded = false;
        }
    }
}
