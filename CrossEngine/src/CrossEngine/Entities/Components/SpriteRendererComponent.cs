using System;

using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Utils;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering.Sprites;
using CrossEngine.Serialization;
using CrossEngine.Events;
using CrossEngine.Utils.Editor;
using CrossEngine.Assets;

namespace CrossEngine.Entities.Components
{
    [RequireComponent(typeof(TransformComponent))]
    public class SpriteRendererComponent : Component
    {
        public enum TransparencyMode
        {
            None = 0,
            Discarding = 1,
            Blending = 2
        }

        [EditorVector2Value]
        public Vector2 Size = Vector2.One;
        [EditorVector2Value]
        public Vector2 DrawOffset = Vector2.Zero;
        [EditorVector4Value]
        public Vector4 TextureOffsets = new Vector4(0f, 0f, 1f, 1f);

        [EditorColor4Value]
        public Vector4 Color = Vector4.One; // also used for tinting; chooses transparency median
        [EditorEnumValue]
        public TransparencyMode TranspMode;

        TextureAsset _textureAsset = null;

        [EditorAssetValue(typeof(TextureAsset))]
        public TextureAsset TextureAsset
        {
            get
            {
                if (_textureAsset != null && _textureAsset.IsValid) return _textureAsset;
                else return _textureAsset = null;
            }
            set => _textureAsset = value;
        }

        //[EditorBooleanValue]
        //public bool ForceZIndex;
        //[EditorSingleValue]
        //public float ZIndex;

        public SpriteRendererComponent()
        {
            
        }

        // RIP OnRender method... you were good one... maybe, but you will be remembered
        // update: he is back xDDD

        //public override void OnRender(RenderEvent re)
        //{
        //    //if (re is SpriteRenderEvent)
        //    //    OnSpriteRenderEvent((SpriteRenderEvent)re);
        //}

        //void OnSpriteRenderEvent(SpriteRenderEvent e)
        //{
        //    Matrix4x4 matrix = Matrix4x4.CreateScale(new Vector3(Size, 1.0f)) * Matrix4x4.CreateTranslation(new Vector3(DrawOffset, 1.0f)) * Entity.Transform.WorldTransformMatrix;
        //
        //    //                          small check can be implemented later
        //    if (TextureAsset == null/* || !src.TextureAsset.IsLoaded*/) Renderer2D.DrawQuad(matrix, Color, Entity.UID);
        //    else Renderer2D.DrawQuad(matrix, TextureAsset.Texture, Color, TextureOffsets, Entity.UID);
        //}

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Size", Size);
            info.AddValue("DrawOffset", DrawOffset);
            info.AddValue("TextureOffsets", TextureOffsets);
            info.AddValue("Color", Color);

            info.AddValue("TransparencyMode", TranspMode);

            info.AddValue("TextureAsset", TextureAsset);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Size = (Vector2)info.GetValue("Size", typeof(Vector2));
            DrawOffset = (Vector2)info.GetValue("DrawOffset", typeof(Vector2));
            TextureOffsets = (Vector4)info.GetValue("TextureOffsets", typeof(Vector4)); 
            Color = (Vector4)info.GetValue("Color", typeof(Vector4));

            TranspMode = (TransparencyMode)info.GetValue("TransparencyMode", typeof(TransparencyMode));

            TextureAsset = (TextureAsset)info.GetValue("TextureAsset", typeof(TextureAsset));
        }
    }
}
