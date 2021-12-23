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
        private bool _loaded = false;
        public override bool IsLoaded { get => _loaded; }

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
            _loaded = true;
        }

        public override void Unload()
        {
            Texture.Dispose();
            Texture = null;
            _loaded = false;
        }
    }
}
