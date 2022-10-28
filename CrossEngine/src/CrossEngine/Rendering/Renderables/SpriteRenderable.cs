using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Components;
using CrossEngine.Utils;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Assets;

namespace CrossEngine.Rendering.Renderables
{
    interface ISpriteRenderData : IObjectRenderData
    {
        Vector4 Color { get; }
        virtual Sphere? Bounds => null;
        virtual Vector4 TextureOffsets => new Vector4(0, 0, 1, 1);
        virtual TextureAsset Texture => null;
    }

    class SpriteRenderable : Renderable<ISpriteRenderData>
    {
        Camera camera;
        public override void Begin(Camera camera)
        {
            Application.Instance.RendererAPI.SetBlendFunc(BlendFunc.OneMinusSrcAlpha);
            this.camera = camera;
        }

        public override void Submit(ISpriteRenderData data)
        {
            Vector4 c = data.Color;
            //var bounds = data.Bounds;
            //if (bounds != null)
            //{
            //    var boundsval = (Sphere)bounds;
            //    switch (camera.Frustum.IsSphereIn(boundsval.center, boundsval.radius))
            //    {
            //        case Halfspace.Outside: return;
            //        case Halfspace.Intersect:
            //            {
            //                c = new Vector4(1, 0, 0, 1);
            //                if (camera.Frustum.IsPointIn(boundsval.center) == Halfspace.Inside)
            //                    c = new Vector4(0, 0, 1, 1);
            //            }
            //            break;
            //    }
            //    LineRenderer.DrawSphere(Matrix4x4.CreateTranslation(boundsval.center), Vector4.One, 16, boundsval.radius);
            //}

            if (data.Texture == null)
                Renderer2D.DrawQuad(data.Transform, c/*data.Color*/, data.EntityId);
            else
                Renderer2D.DrawTexturedQuad(data.Transform, data.Texture.Texture, c/*data.Color*/, data.TextureOffsets, data.EntityId);
        }
    }
}
