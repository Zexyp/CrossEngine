using CrossEngine.Assets;
using CrossEngine.Loaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class TextureAsset : FileAsset
    {
        public WeakReference<Texture> Texture = null;

        public override bool Loaded => Texture != null;

        //[EditorEnum]
        //[EditorNullable]
        //public ColorFormat? Format = null;

        public override async Task Load(IAssetLoadContext context)
        {
            if (RelativePath?.StartsWith("internal:") != true)
                using (Stream stream = await context.OpenRelativeStream(RelativePath))
                {
                    Texture = TextureLoader.LoadTextureFromStream(stream);
                }
            else
                switch (RelativePath.RemovePrefix("internal:"))
                {
                    case "default": Texture = TextureLoader.DefaultTexture; break;
                    case "white": Texture = TextureLoader.WhiteTexture; break;
                    case "black": Texture = TextureLoader.BlackTexture; break;
                    case "normal": Texture = TextureLoader.NormalTexture; break;
                }
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            TextureLoader.Free(Texture);
            Texture = null;
        }
    }

    public class SkyboxAsset : TextureAsset
    {
        public override bool Loaded => Texture != null;

        //[EditorEnum]
        //[EditorNullable]
        //public ColorFormat? Format = null;

        private static string[] fixes = new[] { "px", "nx", "py", "ny", "pz", "nz" };

        public override async Task Load(IAssetLoadContext context)
        {
            var streams = fixes.Select(fix => context.OpenRelativeStream(Path.Join(Path.GetDirectoryName(RelativePath), Path.GetFileNameWithoutExtension(RelativePath) + $".{fix}" + Path.GetExtension(RelativePath))).Result).ToArray();
            Texture = TextureLoader.LoadCubemap(streams);
            streams.ToList().ForEach(s => s.Dispose());
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            TextureLoader.Free(Texture);            
            Texture = null;
        }
    }
}
