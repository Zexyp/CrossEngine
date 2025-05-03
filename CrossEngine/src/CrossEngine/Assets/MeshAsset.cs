using CrossEngine.Loaders;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Geometry;

namespace CrossEngine.Assets
{
    public class MeshAsset : FileAsset
    {
        public IMesh Mesh;

        public override bool Loaded => Mesh != null;

        public override async Task Load(IAssetLoadContext context)
        {
            using (Stream stream = await context.OpenRelativeStream(RelativePath))
                Mesh = MeshLoader.LoadObj(stream);
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Mesh = null;
        }
    }
}
