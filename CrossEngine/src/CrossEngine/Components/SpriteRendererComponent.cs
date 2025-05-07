using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class SpriteRendererComponent : RendererComponent, ISpriteRenderData
    {
        [SerializeInclude]
        [EditorDrag]
        public Vector4 DrawOffsets { get; set; } = new Vector4(0, 0, 1, 1);
        [SerializeInclude]
        [EditorColor]
        public Vector4 Color { get; set; } = Vector4.One;
        [SerializeInclude]
        [EditorEnum]
        public BlendMode Blend { get; set; } = BlendMode.Opaque;
        [EditorNullable]
        [SerializeInclude]
        [EditorAsset]
        public SpriteAsset Sprite;

        Vector4 ISpriteRenderData.TextureOffsets => Sprite?.TextureOffsets ?? new Vector4(0, 0, 1, 1);
        WeakReference<Texture> ISpriteRenderData.Texture => Sprite?.Atlas?.Texture?.Texture;
        BlendMode ISpriteRenderData.Blend => Blend;
        Vector4 ISpriteRenderData.DrawOffsets => DrawOffsets;
        
        protected override IVolume GetVolume()
        {
            var transform = Entity?.Transform;
            if (transform == null)
                return null;
            
            var matrix = Matrix4x4.CreateScale(new Vector3(DrawOffsets.Z, DrawOffsets.W, 1)) *
                         Matrix4x4.CreateTranslation(new Vector3(DrawOffsets.X, DrawOffsets.Y, 0)) * 
                         transform.GetWorldTransformMatrix();
            return new Sphere(matrix.Translation, transform.WorldScale.Length() * Math.Max(this.DrawOffsets.Z, this.DrawOffsets.W) / 2);
        }

        public override object Clone()
        {
            var comp = new SpriteRendererComponent();
            comp.Color = this.Color;
            comp.Blend = this.Blend;
            comp.Sprite = this.Sprite;
            comp.DrawOffsets = this.DrawOffsets;
            return comp;
        }
    }
}
