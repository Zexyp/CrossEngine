using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Loaders;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.ImGui;

namespace CrossEngine.Assets
{
    // TODO: material instances
    public class MaterialAsset : Asset
    {
        [EditorInnerDraw]
        public IMaterial Material;

        public override bool Loaded => Material != null;
        public override Task Load(IAssetLoadContext context) => Task.CompletedTask;
        public override Task Unload(IAssetLoadContext context) => Task.CompletedTask;
    }
    
    public class WavefrontMaterialAsset : MaterialAsset {
        [EditorNullable]
        [EditorAsset]
        public TextureAsset MapDiffuse
        {
            get => texture;
            set
            {
                SetChildId(value, ref texture, ref idMapDiffuse);
                ((MeshLoader.WavefrontMaterial)Material).mapDiffuse = texture?.Texture;
            }
        }

        [EditorAsset]
        public ShaderAsset Shader
        {
            get => shader;
            set
            {
                SetChildId(value, ref shader, ref idShader);
                Material.Shader = value?.Shader;
            }
        }

        public override bool Loaded => Material.Shader != null;

        private ShaderAsset shader = null;
        private Guid idShader = Guid.Empty;
        private TextureAsset texture = null;
        private Guid idMapDiffuse = Guid.Empty;
        
        public WavefrontMaterialAsset()
        {
            Material = new MeshLoader.WavefrontMaterial();
        }

        public override async Task Load(IAssetLoadContext context)
        {
            var mat = (MeshLoader.WavefrontMaterial)Material;
            Shader = context.GetDependency<ShaderAsset>(idShader);
            if (idMapDiffuse != Guid.Empty) MapDiffuse = context.GetDependency<TextureAsset>(idMapDiffuse);
        }

        override public async Task Unload(IAssetLoadContext context)
        {
            var mat = (MeshLoader.WavefrontMaterial)Material;
            mat.mapDiffuse = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);
            
            info.AddValue(nameof(Material), Material);
            info.AddValue(nameof(MapDiffuse), idMapDiffuse);
            info.AddValue("Shader", idShader);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            Material = info.GetValue<MeshLoader.WavefrontMaterial>(nameof(Material), (MeshLoader.WavefrontMaterial)Material);
            idMapDiffuse = info.GetValue(nameof(MapDiffuse), idMapDiffuse);
            idShader = info.GetValue("Shader", idShader);
        }
    }

    public class MtlMaterialAsset : MaterialAsset
    {
        [EditorString]
        [EditorNullable]
        public string RelativePath;
        [EditorString]
        public string MaterialName;
        
        [EditorAsset]
        public ShaderAsset Shader
        {
            get => shader;
            set
            {
                SetChildId(value, ref shader, ref idShader);
                if (Material != null)
                    Material.Shader = value?.Shader;
            }
        }

        public override bool Loaded => Material != null && shader?.Loaded == true;

        private ShaderAsset shader;
        private Guid idShader;

        public override async Task Load(IAssetLoadContext context)
        {
            if (RelativePath != null)
                using (var stream = await context.OpenRelativeStream(RelativePath))
                    Material = MeshLoader.ParseMtl(stream)[MaterialName];
            
            Shader = context.GetDependency<ShaderAsset>(idShader);

            var mat = (MeshLoader.WavefrontMaterial)Material;
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            var mat = (MeshLoader.WavefrontMaterial)Material;
            mat.mapDiffuse = null;
            
            Material = null;
            shader = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);
            
            info.AddValue(nameof(RelativePath), RelativePath);
            info.AddValue(nameof(MaterialName), MaterialName);
            info.AddValue("Shader", idShader);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);
            
            RelativePath = info.GetValue(nameof(RelativePath), RelativePath);
            MaterialName = info.GetValue(nameof(MaterialName), MaterialName);
            idShader = info.GetValue("Shader", idShader);
        }
    }
}
