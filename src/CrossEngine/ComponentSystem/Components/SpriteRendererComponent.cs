using System;

using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Utils;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Sprites;

namespace CrossEngine.ComponentSystem.Components
{
    public class SpriteRendererComponent : Component
    {
        public Sprite sprite;
        public Vector2 size = Vector2.One;
        public Vector4 color = Vector4.One; // also used for tinting

        public SpriteRendererComponent(Sprite sprite)
        {
            this.sprite = sprite;
        }

        public override void OnRender()
        {
            //Quaternion asd = new Quaternion(0,0,1,Mathematics.MathExtension.ToRadians(45));
            if(sprite == null || sprite.texture == null)
                SpriteRenderer.DrawPure(entity.transform.Position.XY(), size, color, entity.transform.Rotation.ToEuler().Z);
            else
                SpriteRenderer.DrawSprite(entity.transform.Position.XY(), size, sprite, color, entity.transform.Rotation.ToEuler().Z);
        }
    }
}
