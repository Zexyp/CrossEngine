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
        public override void Draw(Scene scene, Matrix4x4 viewProjectionMatrix, Framebuffer framebuffer = null)
        {
            if (framebuffer != null)
            {
                framebuffer.EnableColorDrawBuffer(scene.Pipeline.FBStructureIndex.Color, true);
                framebuffer.EnableColorDrawBuffer(scene.Pipeline.FBStructureIndex.ID, true);
            }

            Renderer2D.BeginScene(viewProjectionMatrix);
            {
                var registry = scene.Registry;
                var spriteEnts = registry.GetComponentsGroup<TransformComponent, SpriteRendererComponent>(registry.GetComponentsCollection<SpriteRendererComponent>());
                
                if (spriteEnts != null) foreach (var spEnt in spriteEnts)
                {
                    SpriteRendererComponent src = spEnt.Item2;
                    // check if enabled
                    if (!src.Enabled || !src.Valid) continue;
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
