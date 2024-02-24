using CrossEngine.Assets;
using CrossEngine.Assets.Loaders;
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
    public class TextureAsset : Asset
    {
        public override bool Loaded { get => _loaded; }

        public WeakReference<Texture> Texture = null;

        [EditorString]
        public string RelativePath;

        //[EditorEnum]
        //[EditorNullable]
        //public ColorFormat? Format = null;

        private bool _loaded = false;

        public override async Task Load(IAssetLoadContext context)
        {
            using (Stream stream = await context.OpenRelativeStream(RelativePath))
            {
                Texture = context.GetLoader<TextureLoader>().ScheduleTextureLoad(stream);
            }
            
            _loaded = true;
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            _loaded = false;

            context.GetLoader<TextureLoader>().ScheduleTextureUnload(Texture);
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
