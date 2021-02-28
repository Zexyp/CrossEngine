using System;
using static OpenGL.GL;

using System.Numerics;

using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Cameras;
using CrossEngine.MainLoop;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Sprites
{
    public class SpriteRenderer
    {
        // have in mind that the indices might not work so don't be mad!!!

        struct SpriteVertex
        {
            public Vector3 position;
            public Vector4 color;
            public Vector2 texCoords;
            public float texIndex;
        }

        // constansts
        const uint maxQuadCount = 1000;
        const uint maxVertexCount = maxQuadCount * 4;
        const uint maxIndexCount = maxQuadCount * 6;
        const uint maxTextures = 8;

        #region Renderer Data
        // buffers
        static VertexArray quadVA = null;
        static VertexBuffer quadVB = null;
        static IndexBuffer quadIB = null;

        static uint indexCount = 0;

        static SpriteVertex[] quadBuffer = null;
        static int workingPos = 0;

        // texture stuff
        static uint[] textureSlots = null;
        static uint textureSlotIndex = 1;
        static Texture whiteTexture;

        // stats
        static RendererStats rendererStats; // debug thing
        #endregion

        static Shader quadShader;

        static public unsafe void Init()
        {
            quadShader = AssetManager.Shaders.GetShader("shaders/batch/batch.shader");

            quadShader.Use();
            int[] samplers = new int[maxTextures];
            for (int i = 0; i < maxTextures; i++)
                samplers[i] = i;
            quadShader.SetIntVec("uTextures", samplers);

            quadBuffer = new SpriteVertex[maxVertexCount];

            // gpu side stuff
            quadVA = new VertexArray();
            quadVA.Bind(); // needs to be bound!!!

            quadVB = new VertexBuffer((void*)null, (int)maxVertexCount * sizeof(SpriteVertex), true); // limited

            VertexBufferLayout layout = new VertexBufferLayout();
            layout.Add(typeof(SpriteVertex));
            quadVA.AddBuffer(quadVB, layout);

            uint[] indices = new uint[maxIndexCount];
            uint offset = 0;

            for (int i = 0; i < maxIndexCount; i += 6)
            {
                indices[i + 0] = 0 + offset;
                indices[i + 1] = 2 + offset;
                indices[i + 2] = 1 + offset;

                indices[i + 3] = 3 + offset;
                indices[i + 4] = 2 + offset;
                indices[i + 5] = 0 + offset;

                offset += 4;
            }

            fixed (uint* indicesp = &indices[0])
                quadIB = new IndexBuffer(indicesp, (int)maxIndexCount); // limited


            whiteTexture = new Texture(0xffffffff);

            textureSlots = new uint[maxTextures];
            textureSlots[0] = whiteTexture.id;

            for (int i = 1; i < maxTextures; i++)
                textureSlots[i] = 0;
        }

        static public void Shutdown()
        {
            quadVA.Dispose();
            quadVB.Dispose();
            quadIB.Dispose();

            whiteTexture.Dispose();

            quadBuffer = null; // idk if it helps
        }

        static public unsafe void BeginBatch()
        {
            workingPos = 0;
        }

        static public unsafe void EndBatch()
        {
            fixed (SpriteVertex* quadBufferp = &quadBuffer[0])
            {
                int size = workingPos * sizeof(SpriteVertex);

                quadVB.SetData(quadBufferp, size);
            }
        }

        static public unsafe void Flush()
        {
            if (workingPos > 0)
            {
                quadShader.Use();

                quadShader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                quadShader.SetMat4("view", ActiveCamera.camera.ViewMatrix);
                quadShader.SetMat4("model", Matrix4x4.Identity);

                // texture magic (not really; there is nothing magical about this)
                for (int i = 0; i < textureSlotIndex; i++)
                {
                    glActiveTexture(GL_TEXTURE0 + i);
                    glBindTexture(GL_TEXTURE_2D, textureSlots[i]);
                }

                quadVA.Bind();
                quadIB.Bind();

                glDrawElements(GL_TRIANGLES, (int)indexCount, GL_UNSIGNED_INT, (void*)0);

                indexCount = 0;
                textureSlotIndex = 1; // must be 1 because the one pixel white texture

                //stats
                rendererStats.drawCount++;
            }
        }

        static public void DrawPure(Vector2 position, Vector2 size, Vector4 color, float rotation = 0.0f)
        {
            if (indexCount >= maxIndexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            float textureIndex = 0.0f;

            Transform transform = new Transform(new Vector3(position, 0.0f), Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, rotation), new Vector3(size, 0.0f));

            SetupQuad(ref quadBuffer, ref workingPos, transform.TransformMatrix, color, textureIndex);

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        static public void DrawSprite(Vector2 position, Vector2 size, Sprite sprite, Vector4 tintColor, float rotation = 0.0f)
        {
            if (indexCount >= maxIndexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            float textureIndex = 0.0f;
            for (int i = 1; i < textureSlotIndex; i++)
            {
                if (textureSlots[i] == sprite.texture.id)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                textureIndex = textureSlotIndex;
                textureSlots[textureSlotIndex] = sprite.texture.id;
                textureSlotIndex++;
            }

            Transform transform = new Transform(new Vector3(position, 0.0f), Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, rotation), new Vector3(size * sprite.size, 0.0f));

            SetupSpriteQuad(ref quadBuffer, ref workingPos, sprite.texCoords, transform.TransformMatrix, tintColor, textureIndex);

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        static void SetupQuad(ref SpriteVertex[] quads, ref int index, Matrix4x4 matrix, Vector4 color, float textureIndex)
        {
            Vector3 position;

            //position = position * transform.scale;
            //position += transform.position;

            position = new Vector3(-0.5f, -0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(0.0f, 0.0f);
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(0.5f, -0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(1.0f, 0.0f);
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(0.5f, 0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(1.0f, 1.0f);
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(-0.5f, 0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(0.0f, 1.0f);
            quads[index].texIndex = textureIndex;
            index++;
        }

        static void SetupSpriteQuad(ref SpriteVertex[] quads, ref int index, Vector2[] texCoords, Matrix4x4 matrix, Vector4 color, float textureIndex)
        {
            Vector3 position;

            position = new Vector3(-0.5f, -0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = texCoords[0];
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(0.5f, -0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = texCoords[1];
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(0.5f, 0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = texCoords[2];
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(-0.5f, 0.5f, 0);
            position = Vector3.Transform(position, matrix);
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = texCoords[3];
            quads[index].texIndex = textureIndex;
            index++;
        }

        public static RendererStats GetStats()
        {
            return rendererStats;
        }

        public static void ResetStats()
        {
            RendererStats cleanStats;
            cleanStats.itemCount = 0;
            cleanStats.drawCount = 0;
            rendererStats = cleanStats;
        }
    }
}
