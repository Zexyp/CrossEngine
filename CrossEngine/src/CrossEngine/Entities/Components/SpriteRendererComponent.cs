using System;

using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Utils;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering.Sprites;
using CrossEngine.Serialization.Json;
using CrossEngine.Events;
using CrossEngine.Rendering.Passes;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Entities.Components
{
    public enum TransparencyMode
    {
        None = 0,
        Discarding = 1,
        Blending = 2
    }

    [RequireComponent(typeof(TransformComponent))]
    public class SpriteRendererComponent : Component, ISerializable
    {
        public Sprite Sprite;
        [EditorVector2Value("Size")]
        public Vector2 Size = Vector2.One;
        [EditorColor4Value("Color")]
        public Vector4 Color = Vector4.One; // also used for tinting
        [EditorEnumValue]
        public TransparencyMode TransparencyMode;

        //[EditorBooleanValue]
        //public bool ForceZIndex;
        //[EditorSingleValue]
        //public float ZIndex;

        public SpriteRendererComponent()
        {
            
        }

        // RIP OnRender method... you were good one... maybe, but you will be remembered
        // update: he is back xDDD

        public override void OnRender(RenderEvent re)
        {
            if (re is SpriteRenderPassEvent)
                OnSpriteRenderPassEvent((SpriteRenderPassEvent)re);
        }

        void OnSpriteRenderPassEvent(SpriteRenderPassEvent e)
        {
            if (e.TransparencyMode == TransparencyMode)
            {
                if (Sprite == null || Sprite.Texture == null)
                {
                    //if (!ForceZIndex)
                    Renderer2D.DrawQuad(Matrix4x4.CreateScale(new Vector3(Size, 1.0f)) * Entity.Transform.WorldTransformMatrix, Color, Entity.UID);
                    //else
                    //    Renderer2D.DrawRotatedQuad(new Vector3(Entity.Transform.Position.XY(), ZIndex),  Size * Entity.Transform.Scale.XY(), Entity.Transform.Rotation, Color);
                }
                else
                {
                    //if (!ForceZIndex)
                    Renderer2D.DrawSprite(Matrix4x4.CreateScale(new Vector3(Size, 1.0f)) * Entity.Transform.WorldTransformMatrix, Sprite, Color, Entity.UID);
                    //else
                    //    Renderer2D.DrawRotatedSprite(new Vector3(Entity.Transform.Position.XY(), ZIndex), Size * Entity.Transform.Scale.XY(), Entity.Transform.Rotation, Sprite, Color);
                }
            }
        }

        #region ISerializable
        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue("Size", Size);
            info.AddValue("Color", Color);
            info.AddValue("Sprite", Sprite);
            info.AddValue("TransparencyMode", TransparencyMode);
        }

        public SpriteRendererComponent(DeserializationInfo info)
        {
            Size = (Vector2)info.GetValue("Size", typeof(Vector2));
            Color = (Vector4)info.GetValue("Color", typeof(Vector4));
            Sprite = (Sprite)info.GetRefValue("Sprite", typeof(Sprite), typeof(SpriteRendererComponent).GetMember(nameof(SpriteRendererComponent.Sprite))[0], this);
            TransparencyMode = (TransparencyMode)info.GetValue("TransparencyMode", typeof(TransparencyMode));
        }
        #endregion
    }
}
