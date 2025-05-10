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
using CrossEngine.Rendering.Buffers;
using System.IO;
using CrossEngine.Rendering.Textures;
using static CrossEngine.Loaders.MeshLoader;
using CrossEngine.Geometry;
using System.Diagnostics;

namespace CrossEngine.Assets
{
    public abstract class MaterialAsset : Asset
    {
        public abstract IMaterial Material { get; }
    }
    
    public class WavefrontMaterialAsset : MaterialAsset {
        [EditorAsset]
        public ShaderAsset Shader
        {
            get => shader;
            set
            {
                SetChildId(value, ref shader, ref idShader);
                _material.Shader = value?.Shader;
            }
        }

        [EditorNullable]
        [EditorAsset]
        public TextureAsset MapDiffuse
        {
            get => textureDiffuse;
            set
            {
                SetChildId(value, ref textureDiffuse, ref idTextureDiffuse);
                _material.mapDiffuse = textureDiffuse?.Texture;
            }
        }
        [EditorNullable]
        [EditorAsset]
        public TextureAsset MapSpecular
        {
            get => textureSpecular;
            set
            {
                SetChildId(value, ref textureSpecular, ref idTextureSpecular);
                _material.mapSpecular = textureSpecular?.Texture;
            }
        }
        [EditorNullable]
        [EditorAsset]
        public TextureAsset MapSpecularHighlight
        {
            get => textureSpecularHighlight;
            set
            {
                SetChildId(value, ref textureSpecularHighlight, ref idTextureSpecularHighlight);
                _material.mapSpecularHighlight = textureSpecularHighlight?.Texture;
            }
        }
        [EditorNullable]
        [EditorAsset]
        public TextureAsset MapNormal
        {
            get => textureNormal;
            set
            {
                SetChildId(value, ref textureNormal, ref idTextureNormal);
                _material.mapNormal = textureNormal?.Texture;
            }
        }

        public override bool Loaded => Material.Shader != null;

        [EditorInnerDraw]
        public override IMaterial Material => _material;

        private WavefrontMaterial _material;
        private ShaderAsset shader = null;
        private Guid idShader = Guid.Empty;

        private TextureAsset textureDiffuse = null;
        private TextureAsset textureNormal = null;
        private TextureAsset textureSpecular = null;
        private TextureAsset textureSpecularHighlight = null;
        private Guid idTextureDiffuse = Guid.Empty;
        private Guid idTextureNormal = Guid.Empty;
        private Guid idTextureSpecular = Guid.Empty;
        private Guid idTextureSpecularHighlight = Guid.Empty;
        
        public WavefrontMaterialAsset()
        {
            _material = new WavefrontMaterial();
        }

        public override async Task Load(IAssetLoadContext context)
        {
            Shader = context.GetDependency<ShaderAsset>(idShader);

            if (idTextureDiffuse != Guid.Empty) MapDiffuse = context.GetDependency<TextureAsset>(idTextureDiffuse);
            if (idTextureNormal != Guid.Empty) MapNormal = context.GetDependency<TextureAsset>(idTextureNormal);
            if (idTextureSpecular != Guid.Empty) MapSpecular = context.GetDependency<TextureAsset>(idTextureSpecular);
            if (idTextureSpecularHighlight != Guid.Empty) MapSpecularHighlight = context.GetDependency<TextureAsset>(idTextureSpecularHighlight);
        }

        override public async Task Unload(IAssetLoadContext context)
        {
            _material.mapDiffuse = null;
            _material.mapNormal = null;
            _material.mapSpecular = null;
            _material.mapSpecularHighlight = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);
            
            info.AddValue("Material", _material);
            info.AddValue("Shader", idShader);

            info.AddValue(nameof(MapDiffuse), idTextureDiffuse);
            info.AddValue(nameof(MapNormal), idTextureNormal);
            info.AddValue(nameof(MapSpecular), idTextureSpecular);
            info.AddValue(nameof(MapSpecularHighlight), idTextureSpecularHighlight);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            _material = info.GetValue("Material", _material);
            idShader = info.GetValue("Shader", idShader);

            idTextureDiffuse = info.GetValue(nameof(MapDiffuse), idTextureDiffuse);
            idTextureNormal = info.GetValue(nameof(MapNormal), idTextureNormal);
            idTextureSpecular = info.GetValue(nameof(MapSpecular), idTextureSpecular);
            idTextureSpecularHighlight = info.GetValue(nameof(MapSpecularHighlight), idTextureSpecularHighlight);   
        }
    }

    public class MtlMaterialReferenceAsset : MaterialAsset
    {
        [EditorString]
        public string MaterialName;
        
        [EditorAsset]
        public MtlMaterialLibraryAsset Parent
        {
            get => parent;
            set => SetChildId(value, ref parent, ref idParent);
        }

        public override bool Loaded => Parent != null && Parent.Shader?.Loaded == true;
        public override IMaterial Material => parent?.Materials?.TryGetValue(MaterialName, out var m) == true ? m : null;

        private MtlMaterialLibraryAsset parent;
        private TextureAsset textureDiffuse = null;
        private TextureAsset textureNormal = null;
        private TextureAsset textureSpecular = null;
        private TextureAsset textureSpecularHighlight = null;
        private Guid idParent;

        public override async Task Load(IAssetLoadContext context)
        {
            Parent = context.GetDependency<MtlMaterialLibraryAsset>(idParent);

            var mat = (WavefrontMaterial)Material;

            LoadTexture(context, mat.texturePathDiffuse,            ref textureDiffuse,             ref mat.mapDiffuse);
            LoadTexture(context, mat.texturePathNormal,             ref textureNormal,              ref mat.mapNormal);
            LoadTexture(context, mat.texturePathSpecular,           ref textureSpecular,            ref mat.mapSpecular);
            LoadTexture(context, mat.texturePathSpecularHighlight,  ref textureSpecularHighlight,   ref mat.mapSpecularHighlight);
        }

        public override async Task Unload(IAssetLoadContext context)
        {
            var mat = (WavefrontMaterial)Material;

            mat.mapDiffuse = null;
            mat.mapNormal = null;
            mat.mapSpecular = null;
            mat.mapSpecularHighlight = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(Parent), idParent);
            info.AddValue(nameof(MaterialName), MaterialName);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            idParent = info.GetValue(nameof(Parent), idParent);
            MaterialName = info.GetValue(nameof(MaterialName), MaterialName);
        }

        private void LoadTexture(IAssetLoadContext context, string file, ref TextureAsset slot, ref WeakReference<Texture> texture)
        {
            if (file == null) return;
            slot = context.GetFileAsset<TextureAsset>(Path.Join(Path.GetDirectoryName(parent.RelativePath), file));
            texture = slot?.Texture;
            Debug.Assert(slot?.Loaded != false);
        }
    }

    public class MtlMaterialLibraryAsset : FileAsset
    {
        [EditorAsset]
        public ShaderAsset Shader
        {
            get => shader;
            set
            {
                SetChildId(value, ref shader, ref idShader);
                if (Materials != null)
                    foreach(var mat in Materials.Values)
                        mat.Shader = value?.Shader;
            }
        }

        public override bool Loaded => Materials != null && Shader?.Loaded == true;

        public Dictionary<string, WavefrontMaterial> Materials;
        private ShaderAsset shader;
        private Guid idShader;

        public MtlMaterialLibraryAsset()
        {

        }

        public MtlMaterialLibraryAsset(Dictionary<string, WavefrontMaterial> materials)
        {
            Materials = materials;
        }

        public override async Task Load(IAssetLoadContext context)
        {
            if (Materials == null)
                using (var stream = await context.OpenRelativeStream(RelativePath))
                    Materials = MeshLoader.ParseMtl(stream);

            Shader = context.GetDependency<ShaderAsset>(idShader);

        }

        public override async Task Unload(IAssetLoadContext context)
        {
            Materials = null;
            shader = null;
        }

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue("Shader", idShader);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            idShader = info.GetValue("Shader", idShader);
        }
    }
}
