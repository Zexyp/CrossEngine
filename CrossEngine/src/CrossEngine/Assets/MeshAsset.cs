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
using System.ComponentModel.DataAnnotations;
using static CrossEngine.Loaders.MeshLoader;

namespace CrossEngine.Assets
{
    public abstract class MeshAsset : Asset
    {
        public abstract IMesh Mesh { get; }
    }

    public class GeneratedMeshAsset : MeshAsset
    {
        public override bool Loaded => _mesh != null;

        public override IMesh Mesh => _mesh;

        [EditorString]
        public string Generate;

        private IMesh _mesh;

        public override async Task Load(IAssetLoadContext context)
        {
            if (Generate?.StartsWith("internal:") == true)
            {
                switch (Generate.RemovePrefix("internal:"))
                {
                    case "cube": _mesh = MeshGenerator.GenerateCube(Vector3.One); break;
                    case "plane": _mesh = MeshGenerator.GenerateGrid(Vector2.One); break;
                }
            }
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            _mesh = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(Generate), Generate);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            Generate = info.GetValue(nameof(Generate), Generate);
        }
    }

    public class ObjMeshReferenceAsset : MeshAsset
    {
        [EditorAsset]
        public ObjModelAsset Parent
        {
            get => parent;
            set => SetChildId(value, ref parent, ref idParent);
        }
        public override IMesh Mesh => parent?.Meshes?.TryGetValue(MeshName, out var m) == true ? m : null;

        public override bool Loaded => parent?.Meshes?.ContainsKey(MeshName) == true;

        [EditorString]
        public string MeshName;

        private ObjModelAsset parent = null;
        private Guid idParent = Guid.Empty;

        public override async Task Load(IAssetLoadContext context)
        {
            parent = context.GetDependency<ObjModelAsset>(idParent);
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            parent = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(Parent), idParent);
            info.AddValue(nameof(MeshName), MeshName);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            idParent = info.GetValue(nameof(Parent), idParent);
            MeshName = info.GetValue(nameof(MeshName), MeshName);
        }
    }
    
    public class ObjModelAsset : FileAsset
    {
        public Dictionary<string, WavefrontMesh> Meshes { get; private set; }

        public override bool Loaded => Meshes != null;

        public ObjModelAsset()
        {

        }

        public ObjModelAsset(Dictionary<string, WavefrontMesh> meshes)
        {
            Meshes = meshes;
        }

        public override async Task Load(IAssetLoadContext context)
        {
            if (Meshes == null)
                using (Stream stream = await context.OpenRelativeStream(RelativePath))
                    Meshes = MeshLoader.ParseObj(stream, out _);
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Meshes = null;
        }
    }
}
