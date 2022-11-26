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
using CrossEngine.Serialization;
using CrossEngine.Rendering.Culling;
using CrossEngine.Assets;

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

        [EditorDrag]
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
        [EditorDrag]
        public Vector4 TextureOffsets = new Vector4(0f, 0f, 1f, 1f);

        [EditorColor]
        public Vector4 Color = Vector4.One; // also used for tinting; chooses transparency median
        //[EditorEnumValue]
        //public TransparencyMode TranspMode;

        private TextureAsset _texture;
        [EditorAsset(typeof(TextureAsset))]
        public TextureAsset Texture
        {
            get => _texture;
            set
            {
                if (value == _texture) return;
                _texture?.Unlock();
                _texture = value;
                _texture?.Lock();
            }
        }

        // virtual offset
        private Vector4 _drawOffsets = new Vector4(0f, 0f, 1f, 1f);
        private bool _localOffsetMatrixDirty = true;
        private Matrix4x4 _localOffsetMatrix = Matrix4x4.Identity;
        private Matrix4x4 LocalOffsetMatrix
        {
            get
            {
                if (_localOffsetMatrixDirty)
                {
                    _localOffsetMatrixDirty = false;
                    _localOffsetMatrix = Matrix4x4.CreateScale(new Vector3(_drawOffsets.Z, _drawOffsets.W, 0.0f)) *
                                        Matrix4x4.CreateTranslation(new Vector3(_drawOffsets.X, _drawOffsets.Y, 0.0f));
                }
                return _localOffsetMatrix;
            }
        }

        // ISpriteRenderData
        Matrix4x4 IObjectRenderData.Transform => this.LocalOffsetMatrix * (this.Entity.Transform?.WorldTransformMatrix ?? Matrix4x4.Identity);
        Vector4 ISpriteRenderData.Color => this.Color;
        int IObjectRenderData.EntityId => this.Entity.Id.GetHashCode();
        Vector4 ISpriteRenderData.TextureOffsets => this.TextureOffsets;
        Sphere? ISpriteRenderData.Bounds
        {
            get
            {
                var ws = (this.Entity.Transform?.WorldScale ?? Vector3.One);
                var sz = ws * new Vector3(_drawOffsets.Z, _drawOffsets.W, 0);
                var r = sz.Length() / 2 + (new Vector2(_drawOffsets.X, _drawOffsets.Y) * ws.XY()).Length();
                return new Sphere((this.Entity.Transform?.WorldPosition ?? Vector3.Zero), r);
            }
        }
        TextureAsset ISpriteRenderData.Texture => Texture;

        public SpriteRendererComponent()
        {
            
        }

        internal protected override void Attach(World world)
        {
            world.GetSystem<SpriteRendererSystem>().Register(this);
        }

        internal protected override void Detach(World world)
        {
            world.GetSystem<SpriteRendererSystem>().Unregister(this);
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

        protected internal override void Serialize(SerializationInfo info)
        {
            info.AddValue(nameof(DrawOffsets), DrawOffsets);
            info.AddValue(nameof(TextureOffsets), TextureOffsets);
            info.AddValue(nameof(Color), Color);
            info.AddValue(nameof(Texture), Texture);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            DrawOffsets = info.GetValue(nameof(DrawOffsets), DrawOffsets);
            TextureOffsets = info.GetValue(nameof(TextureOffsets), TextureOffsets);
            Color = info.GetValue(nameof(Color), Color);
            Texture = info.GetValue(nameof(Texture), Texture);
        }

        protected override Component CreateClone()
        {
            var sr = new SpriteRendererComponent();

            sr.DrawOffsets = this.DrawOffsets;
            sr.TextureOffsets = this.TextureOffsets;
            sr.Color = this.Color;

            return sr;
        }
    }
}
