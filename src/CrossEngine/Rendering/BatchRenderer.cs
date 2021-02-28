using System;
using static OpenGL.GL;

using System.Numerics;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Shading;
using CrossEngine.Utils;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering
{
    public struct RendererStats
    {
        public uint drawCount;
        public uint itemCount;

        public void Reset()
        {
            drawCount = 0;
            itemCount = 0;
        }

        public override string ToString()
        {
            return "draw count: " + drawCount + "; item count: " + itemCount;
        }
    }

    public class BatchRenderer
    {
        // have in mind that the indices might not work so don't be mad!!!

        struct BatchVertex
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

        static BatchVertex[] quadBuffer = null;
        static int workingPos = 0;
        //static Vertex* quadBufferPtr = (Vertex*)0;

        // texture stuff
        static uint[] textureSlots = null;
        static uint textureSlotIndex = 1;
        static Texture whiteTexture;
        //static uint whiteTextureSlot = 0;

        // stats
        static RendererStats rendererStats; // mostly debug thing
        #endregion

        static Shader quadShader;

        static public unsafe void Init()
        {
            quadShader = AssetManager.Shaders.GetShader("shaders/batch/batch.shader"); // kinda hot fix

            quadShader.Use();
            int[] samplers = new int[maxTextures];
            for (int i = 0; i < maxTextures; i++)
                samplers[i] = i;
            quadShader.SetIntVec("uTextures", samplers);

            quadBuffer = new BatchVertex[maxVertexCount];

            quadVA = new VertexArray();
            quadVA.Bind(); // needs to be bound!!!

            quadVB = new VertexBuffer((void*)null, (int)maxVertexCount * sizeof(BatchVertex), true); // limited

            VertexBufferLayout layout = new VertexBufferLayout();
            layout.Add(typeof(BatchVertex));
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

            //fixed (Vertex* quadBufferp = &quadBuffer[0])
            //    quadBufferPtr = quadBufferp;

            //Log.Debug("begin batch: " + ((int)quadBufferPtr).ToString());
        }

        static public unsafe void EndBatch()
        {
            fixed (BatchVertex* quadBufferp = &quadBuffer[0])
            {
                //int size = (byte)quadBufferPtr - (byte)quadBufferp;
                int size = workingPos * sizeof(BatchVertex);

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

                for (int i = 0; i < textureSlotIndex; i++)
                {
                    glActiveTexture(GL_TEXTURE0 + i);
                    glBindTexture(GL_TEXTURE_2D, textureSlots[i]);
                }

                //Renderer.Draw(quadVA, quadIB, quadShader, );
                quadVA.Bind();
                quadIB.Bind();

                glDrawElements(GL_TRIANGLES, (int)indexCount, GL_UNSIGNED_INT, (void*)0);

                indexCount = 0;
                textureSlotIndex = 1; // must be 1 because one pixel white texture

                //stats
                rendererStats.drawCount++;
            }
        }

        static public void DrawQuad(Vector2 position, Vector2 size, Vector4 color, float rotation = 0.0f)
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

            //quadBuffer[workingPos].position = new Vector3(position - size / 2, 0.0f);
            //quadBuffer[workingPos].color = color;
            //quadBuffer[workingPos].texCoords = new Vector2(0.0f, 0.0f);
            //quadBuffer[workingPos].texIndex = textureIndex;
            //workingPos++;
            //
            //quadBuffer[workingPos].position = new Vector3(position.X + size.X / 2, position.Y - size.Y / 2, 0.0f);
            //quadBuffer[workingPos].color = color;
            //quadBuffer[workingPos].texCoords = new Vector2(1.0f, 0.0f);
            //quadBuffer[workingPos].texIndex = textureIndex;
            //workingPos++;
            //
            //quadBuffer[workingPos].position = new Vector3((size / 2) + position, 0.0f);
            //quadBuffer[workingPos].color = color;
            //quadBuffer[workingPos].texCoords = new Vector2(1.0f, 1.0f);
            //quadBuffer[workingPos].texIndex = textureIndex;
            //workingPos++;
            //
            //quadBuffer[workingPos].position = new Vector3(position.X - size.X / 2, position.Y + size.Y / 2, 0.0f);
            //quadBuffer[workingPos].color = color;
            //quadBuffer[workingPos].texCoords = new Vector2(0.0f, 1.0f);
            //quadBuffer[workingPos].texIndex = textureIndex;
            //workingPos++;

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        static public void DrawQuad(Vector2 position, Vector2 size, Texture texture, float rotation = 0.0f)
        {
            if (indexCount >= maxIndexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            Vector4 color = Vector4.One;

            float textureIndex = 0.0f;
            for (int i = 1; i < textureSlotIndex; i++)
            {
                if (textureSlots[i] == texture.id)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                textureIndex = textureSlotIndex;
                textureSlots[textureSlotIndex] = texture.id;
                textureSlotIndex++;
            }

            Transform transform = new Transform(new Vector3(position, 0.0f), Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, rotation), new Vector3(size, 0.0f));

            SetupQuad(ref quadBuffer, ref workingPos, transform.TransformMatrix, color, textureIndex);

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        static public void DrawQuad(Vector2 position, Vector2 size, Texture texture, Vector4 tintColor, float rotation = 0.0f)
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
                if (textureSlots[i] == texture.id)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                textureIndex = textureSlotIndex;
                textureSlots[textureSlotIndex] = texture.id;
                textureSlotIndex++;
            }

            Transform transform = new Transform(new Vector3(position, 0.0f), Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, rotation), new Vector3(size, 0.0f));

            SetupQuad(ref quadBuffer, ref workingPos, transform.TransformMatrix, tintColor, textureIndex);

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        //####################################################################################################
        //####################################################################################################

        static public void DrawBillboard(Vector3 position, Vector2 size, Vector4 color, float rotation = 0.0f)
        {
            if (indexCount >= maxIndexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            //Matrix4x4.CreateBillboard(position, Scene.camera.Position, Scene.camera.Up, Scene.camera.Front)

            Matrix4x4 matrix = Matrix4x4.CreateRotationZ(rotation) * Matrix4x4Extension.CreateBillboard(ActiveCamera.camera.Right, ActiveCamera.camera.Up, ActiveCamera.camera.Front, position);
            matrix = Matrix4x4.CreateScale(new Vector3(new Vector2(size.Y, size.X), 1)) * matrix;

            SetupQuad(ref quadBuffer, ref workingPos, matrix, color, 0.0f);

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        static public void DrawBillboard(Vector3 position, Vector2 size, Texture texture, float rotation = 0.0f)
        {
            if (indexCount >= maxIndexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            Vector4 color = Vector4.One;

            float textureIndex = 0.0f;
            for (int i = 1; i < textureSlotIndex; i++)
            {
                if (textureSlots[i] == texture.id)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                textureIndex = textureSlotIndex;
                textureSlots[textureSlotIndex] = texture.id;
                textureSlotIndex++;
            }

            Matrix4x4 matrix = Matrix4x4.CreateRotationZ(rotation) * Matrix4x4Extension.CreateBillboard(ActiveCamera.camera.Right, ActiveCamera.camera.Up, ActiveCamera.camera.Front, position);
            matrix = Matrix4x4.CreateScale(new Vector3(new Vector2(size.Y, size.X), 1)) * matrix;

            SetupQuad(ref quadBuffer, ref workingPos, matrix, color, textureIndex);

            indexCount += 6;

            // statistics
            rendererStats.itemCount++;
        }

        /*
        static public void DrawParticle(Vector3 position, float size, Texture texture, Vector4 color, float rotation = 0.0f) // drawing of tinted billboard
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
                if (textureSlots[i] == texture.id)
                {
                    textureIndex = i;
                    break;
                }
            }

            if (textureIndex == 0.0f)
            {
                textureIndex = textureSlotIndex;
                textureSlots[textureSlotIndex] = texture.id;
                textureSlotIndex++;
            }

            Matrix4x4 matrix = Matrix4x4.CreateRotationZ(rotation) * Matrix4x4Extension.CreateBillboard(Scene.camera.right, Scene.camera.up, Scene.camera.front, position);
            matrix = Matrix4x4.CreateScale(size) * matrix;

            SetupQuad(ref quadBuffer, ref workingPos, matrix, color, textureIndex);

            indexCount += 6;

            // statistics
            rendererStats.quadCount++;
        }

        */
        static void SetupQuad(ref BatchVertex[] quads, ref int index, Matrix4x4 matrix, Vector4 color, float textureIndex)
        {
            Vector3 position;

            position = new Vector3(-0.5f, -0.5f, 0);
            //position = position * transform.scale;
            position = Vector3.Transform(position, matrix);
            //position += transform.position;
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(0.0f, 0.0f);
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(0.5f, -0.5f, 0);
            //position = position * transform.scale;
            position = Vector3.Transform(position, matrix);
            //position += transform.position;
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(1.0f, 0.0f);
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(0.5f, 0.5f, 0);
            //position = position * transform.scale;
            position = Vector3.Transform(position, matrix);
            //position += transform.position;
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(1.0f, 1.0f);
            quads[index].texIndex = textureIndex;
            index++;

            position = new Vector3(-0.5f, 0.5f, 0);
            //position = position * transform.scale;
            position = Vector3.Transform(position, matrix);
            //position += transform.position;
            quads[index].position = position;
            quads[index].color = color;
            quads[index].texCoords = new Vector2(0.0f, 1.0f);
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
