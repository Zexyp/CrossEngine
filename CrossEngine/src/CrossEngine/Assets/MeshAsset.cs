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
using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public abstract class MeshAsset : Asset
    {
        public IMesh Mesh;
    }
    
    public class ObjMeshAsset : MeshAsset
    {
        [EditorString]
        public string RelativePath;
        [EditorString]
        public string MeshName;

        public override bool Loaded => Mesh != null;

        public override async Task Load(IAssetLoadContext context)
        {
            using (Stream stream = await context.OpenRelativeStream(RelativePath))
                Mesh = MeshLoader.ParseObj(stream)[MeshName];
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Mesh = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);
            
            info.AddValue(nameof(RelativePath), RelativePath);
            info.AddValue(nameof(MeshName), MeshName);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);
            
            RelativePath = info.GetValue(nameof(RelativePath), RelativePath);
            MeshName = info.GetValue(nameof(MeshName), MeshName);
        }
    }
}
