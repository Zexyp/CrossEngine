using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Scenes;
using CrossEngine.Entities.Components;

namespace CrossEngine.Rendering.Passes
{
    public class Renderer2DPass : RenderPass
    {
        List<ComponentGroup<TransformComponent, SpriteRendererComponent>> bucket = new List<ComponentGroup<TransformComponent, SpriteRendererComponent>>();

        public override void GatherData(Scene scene)
        {
            var registry = scene.Registry;

            var srcCollection = new List<SpriteRendererComponent>();
            registry.GetComponents(srcCollection);
            registry.GetComponentsGroup(srcCollection, bucket);
        }

        public override void Clear()
        {
            bucket.Clear();
        }

        public override void Draw(Scene scene, Matrix4x4 viewProjectionMatrix, Framebuffer framebuffer = null)
        {
            if (framebuffer != null)
            {
                framebuffer.EnableColorDrawBuffer(scene.Pipeline.FbStructureIndex.Color, true);
                framebuffer.EnableColorDrawBuffer(scene.Pipeline.FbStructureIndex.Id, true);
            }

            // TODO: add transparency
            Renderer2D.BeginScene(viewProjectionMatrix);
            {
                if (bucket != null) foreach (var spEnt in bucket)
                {
                    SpriteRendererComponent src = spEnt.Item2;
                    // check if enabled
                    if (!src.Usable) continue;
                    TransformComponent trans = spEnt.Item1;

                    // forced z index
                    //if (!ForceZIndex)
                    //else

                    Matrix4x4 matrix = Matrix4x4.CreateScale(new Vector3(src.Size, 1.0f)) * Matrix4x4.CreateTranslation(new Vector3(src.DrawOffset, 0.0f)) * trans.WorldTransformMatrix;

                    //                          small check can be implemented later
                    if (src.TextureAsset == null) Renderer2D.DrawQuad(matrix, src.Color, spEnt.CommonEntity.UID);
                    else if (src.TextureAsset.IsLoaded) Renderer2D.DrawQuad(matrix, src.TextureAsset.Texture, src.Color, src.TextureOffsets, spEnt.CommonEntity.UID);
                }

                scene.OnRender(new SpriteRenderEvent());
            }
            Renderer2D.EndScene();
        }
    }
}
