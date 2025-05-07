using CrossEngine.Loaders;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Geometry;
using CrossEngine.Serialization;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Assets
{
    public class MeshAsset : Asset
    {
        public IMesh Mesh;
        public override bool Loaded => Mesh != null;
        
        [EditorString]
        public string RelativePath;

        public override async Task Load(IAssetLoadContext context)
        {
            if (RelativePath.StartsWith("internal:"))
            {
                switch (RelativePath.RemovePrefix("internal:"))
                {
                    case "cube": Mesh = MeshGenerator.GenerateCube(Vector3.One); break;
                    case "plane": Mesh = MeshGenerator.GenerateGrid(Vector2.One); break;
                }
            }
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Mesh = null;
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
    
    public class ObjMeshAsset : MeshAsset
    {
        [EditorString]
        public string MeshName;
        
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
