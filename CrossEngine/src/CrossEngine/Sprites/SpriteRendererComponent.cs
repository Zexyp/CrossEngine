using System;

using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Utils;
using CrossEngine.Rendering.Textures;
using CrossEngine.Events;
using CrossEngine.ECS;
using CrossEngine.Utils.Editor;
using CrossEngine.ComponentSystems;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Components
{
    //[RequireComponent(typeof(TransformComponent))]
    public class SpriteRendererComponent : Component, ISpriteRenderData
    {
        //public enum TransparencyMode
        //{
        //    None = 0,
        //    Discarding = 1,
        //    Blending = 2
        //}

        [EditorVector4Value]
        public Vector4 DrawOffsets
        {
            get => _drawOffsets;
            set
            {
                if (value == _drawOffsets) return;
                _drawOffsets = value;
                _localOffsetMatrixDirty = true;
            }
        }
        [EditorVector4Value]
        public Vector4 TextureOffsets = new Vector4(0f, 0f, 1f, 1f);

        [EditorColor4Value]
        public Vector4 Color = Vector4.One; // also used for tinting; chooses transparency median
        //[EditorEnumValue]
        //public TransparencyMode TranspMode;

        //[EditorAssetValue(typeof(TextureAsset))]
        public Ref<Texture> Texture;

        // virtual offset
        private Vector4 _drawOffsets = new Vector4(0f, 0f, 1f, 1f);
        private bool _localOffsetMatrixDirty = true;
        private Matrix4x4 _localOffsetMatrix = Matrix4x4.Identity;

        // ISpriteRenderData
        Matrix4x4 IObjectRenderData.Transform => this._localOffsetMatrix * (this.Entity.TryGetComponent(out TransformComponent tc) ? tc.WorldTransformMatrix : Matrix4x4.Identity);
        Vector4 ISpriteRenderData.Color => this.Color;
        Ref<Texture> ISpriteRenderData.Texture => this.Texture;
        int ISpriteRenderData.EntityId => this.Entity.Id;
        Vector4 ISpriteRenderData.TextureOffsets => this.TextureOffsets;

        public SpriteRendererComponent()
        {
            
        }

        public override void Attach()
        {
            SpriteRendererSystem.Instance.Register(this);
        }

        public override void Detach()
        {
            SpriteRendererSystem.Instance.Unregister(this);
        }

        public override void Update()
        {
            if (_localOffsetMatrixDirty)
            {
                _localOffsetMatrixDirty = false;
                _localOffsetMatrix = Matrix4x4.CreateScale(new Vector3(_drawOffsets.Z, _drawOffsets.W, 0.0f)) *
                                    Matrix4x4.CreateTranslation(new Vector3(_drawOffsets.X, _drawOffsets.Y, 0.0f));
            }
        }



        // RIP OnRender method... you were good one... maybe, but you will be remembered
        // update: he is back xDDD
        // second update: he is no longer among us
        // 21.3.2022 - after movement of scene rendering code to separate layer and later SRC component review,
        //             it was concluded that he is useless and has no good power

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

        //public override void OnSerialize(SerializationInfo info)
        //{
        //    info.AddValue("Size", Size);
        //    info.AddValue("DrawOffset", DrawOffset);
        //    info.AddValue("TextureOffsets", TextureOffsets);
        //    info.AddValue("Color", Color);
        //
        //    info.AddValue("TransparencyMode", TranspMode);
        //
        //    info.AddValue("TextureAsset", TextureAsset.Asset?.Name);
        //}
        //
        //public override void OnDeserialize(SerializationInfo info)
        //{
        //    Size = (Vector2)info.GetValue("Size", typeof(Vector2));
        //    DrawOffset = (Vector2)info.GetValue("DrawOffset", typeof(Vector2));
        //    TextureOffsets = (Vector4)info.GetValue("TextureOffsets", typeof(Vector4)); 
        //    Color = (Vector4)info.GetValue("Color", typeof(Vector4));
        //
        //    TranspMode = (TransparencyMode)info.GetValue("TransparencyMode", typeof(TransparencyMode));
        //
        //    TextureAsset = (TextureAsset)info.GetValue("TextureAsset", typeof(TextureAsset));
        //}

        public override object Clone()
        {
            var sr = new SpriteRendererComponent();
            sr.Enabled = this.Enabled;

            sr.DrawOffsets = this.DrawOffsets;
            sr.TextureOffsets = this.TextureOffsets;
            sr.Color = this.Color;

            return sr;
        }
    }
}
