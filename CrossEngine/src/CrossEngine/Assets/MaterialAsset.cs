using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class MaterialAsset : Asset
    {
        public Material Material;

        public override bool Loaded => Material != null;

        public override Task Load(IAssetLoadContext context)
        {
            throw new NotImplementedException();
        }

        public override Task Unload(IAssetLoadContext context)
        {
            throw new NotImplementedException();
        }
    }
}
