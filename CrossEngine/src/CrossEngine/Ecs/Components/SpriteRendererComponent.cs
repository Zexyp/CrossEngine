using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Textures;
using CrossEngine.Assets;
using CrossEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components
{
    public class SpriteRendererComponent : Component, ISpriteRenderData
    {
        public Vector4 Color { get; set; } = Vector4.One;
        public BlendMode Blend { get; set; } = BlendMode.Blend;
        public SpriteAsset Sprite;

        Matrix4x4 IObjectRenderData.Transform => Entity.Transform?.WorldTransformMatrix ?? Matrix4x4.Identity;
        int ISpriteRenderData.EntityId => Entity?.Id.GetHashCode() ?? 0;
        Vector4 ISpriteRenderData.TextureOffsets => Sprite?.TextureOffsets ?? new Vector4(0, 0, 1, 1);
        WeakReference<Texture> ISpriteRenderData.Texture => Sprite?.Texture?.Texture;
        BlendMode ISpriteRenderData.Blend => Blend;

        protected override Component CreateClone()
        {
            var comp = new SpriteRendererComponent();
            comp.Color = this.Color;
            return comp;
        }

        protected internal override void OnSerialize(SerializationInfo info)
        {
            info.AddValue(nameof(Color), Color);
            info.AddValue(nameof(Sprite), Sprite);
        }

        protected internal override void OnDeserialize(SerializationInfo info)
        {
            Color = info.GetValue(nameof(Color), Color);
            Sprite = info.GetValue<SpriteAsset>(nameof(Sprite));
        }
    }
}
