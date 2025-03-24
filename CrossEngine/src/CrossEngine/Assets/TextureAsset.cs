using CrossEngine.Assets;
using CrossEngine.Loaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
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
            using (Stream stream = await context.OpenRelativeStream(RelativePath))
            {
                Texture = TextureLoader.LoadTextureFromStream(stream);
            }
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            TextureLoader.Free(Texture);            
            Texture = null;
        }
    }
}
