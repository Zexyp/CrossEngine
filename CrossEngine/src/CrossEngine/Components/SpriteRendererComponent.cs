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
        [Serialize]
        [EditorDrag]
        public Vector4 DrawOffsets { get; set; } = new Vector4(0, 0, 1, 1);
        [Serialize]
        [EditorColor]
        public Vector4 Color { get; set; } = Vector4.One;
        [Serialize]
        [EditorEnum]
        public BlendMode Blend { get; set; } = BlendMode.Blend;
        [EditorNullable]
        [Serialize]
        [EditorAsset]
        public SpriteAsset Sprite;

        Vector4 ISpriteRenderData.TextureOffsets => Sprite?.TextureOffsets ?? new Vector4(0, 0, 1, 1);
        WeakReference<Texture> ISpriteRenderData.Texture => Sprite?.Texture?.Texture;
        BlendMode ISpriteRenderData.Blend => Blend;
        Vector4 ISpriteRenderData.DrawOffsets => DrawOffsets;

        protected override IVolume GetVolume()
        {
            var transform = Entity?.Transform;
            if (transform == null)
                return null;
            
            return new Sphere(transform.WorldPosition, transform.Scale.Length() / 2);
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
