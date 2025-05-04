using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Loaders;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Assets
{
    public abstract class MaterialAsset : Asset
    {
        public IMaterial Material;
    }

    public class MtlMaterialAsset : MaterialAsset
    {
        [EditorString]
        public string RelativePath;
        [EditorString]
        public string MaterialName;

        public override bool Loaded => Material != null;

        public override async Task Load(IAssetLoadContext context)
        {
            using (var stream = await context.OpenRelativeStream(RelativePath))
                Material = MeshLoader.ParseMtl(stream)[MaterialName];
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Material = null;
        }
    }
}
