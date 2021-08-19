using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Numerics;

//using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;
//using CrossEngine.Rendering.Sprites;
using CrossEngine.Utils;
//using CrossEngine.Rendering.Text;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Scenes;
using CrossEngine.Rendering.Passes;

//using CrossEngine.FX.Particles;

namespace CrossEngine.Rendering
{
    public readonly struct SceneData
    {
        public readonly Scene Scene;
        public readonly Matrix4x4 ViewProjectionMatrix;

        public SceneData(Scene scene, Matrix4x4 viewProjectionMatrix)
        {
            Scene = scene;
            ViewProjectionMatrix = viewProjectionMatrix;
        }
    }

    public struct RendererStats
    {
        public uint drawCalls;
        public uint itemCount;

        public void Reset()
        {
            drawCalls = 0;
            itemCount = 0;
        }

        public override string ToString()
        {
            return "Draw count: " + drawCalls + "; Item count: " + itemCount;
        }
    }

    public static class Renderer
    {
        static void Init()
        {

        }

        static void Shutdown()
        {

        }

        static public void Clear()
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }

        static public void SetClearColor(Vector4 color)
        {
            glClearColor(color.X, color.Y, color.Z, color.W);
        }

        static public void SetClearColor(float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 1.0f)
        {
            glClearColor(r, g, b, a);
        }

        //static void SetViewport(int x, int y, int width, int height)
        //{
        //    glViewport(x, y, width, height);
        //}

        //public static void DrawIndexed(VertexArray va) => DrawIndexed(va, (int)va.GetIndexBuffer().GetCount());
        public static unsafe void DrawIndexed(DrawMode mode, VertexArray va, int? verticesCount = null)
        {
            va.Bind();
            if (verticesCount == null)
                glDrawElements((int)mode, (int)va.GetIndexBuffer().GetCount(), (int)va.GetIndexBuffer().GetIndexDataType(), null);
            else
                glDrawElements((int)mode, (int)verticesCount, (int)va.GetIndexBuffer().GetIndexDataType(), null);
        }

        public static unsafe void DrawArrays(DrawMode mode, VertexArray va, int verticesCount)
        {
            va.Bind();
            glDrawArrays((int)mode, 0, verticesCount);
        }

        static List<RenderPass> passes = new List<RenderPass>();

        public static void RegisterPass(RenderPass pass)
        {
            passes.Add(pass);
        }

        public static void Render(Scene scene, Matrix4x4 viewProjectionMatrix)
        {
            SceneData data = new SceneData(scene, viewProjectionMatrix);
            for (int i = 0; i < passes.Count; i++)
            {
                passes[i].Render(data);
            }
        }

        public static void EnableBlending(bool enable, BlendFunc? func = null)
        {
            if (enable)
                glEnable(GL_BLEND);
            else
                glDisable(GL_BLEND);

            if (func != null)
                glBlendFunc(GL_SRC_ALPHA, (int)func);
        }

        public static void EnableDepthTest(bool enable)
        {
            if (enable)
                glEnable(GL_DEPTH_TEST);
            else
                glDisable(GL_DEPTH_TEST);
        }

        public static void SetDepthFunc(DepthFunc func)
        {
            glDepthFunc((int)func);
        }

        public static void SetPolygonMode(PolygonMode polygonMode)
        {
            glPolygonMode(GL_FRONT_AND_BACK, (int)polygonMode);
        }

        /*

        public static void Init()
        {
            initialized = true;

            Line.Init();
            Batch.Init();
            Text.Init();
            InstanceRenderer.Init();
        }

        public static void Shutdown()
        {
            initialized = false;

            Line.Shutdown();
            Batch.Shutdown();
            Text.Shutdown();
            InstanceRenderer.Shutdown();
        }

        struct RendererData
        {
            public VertexArray va;
            public VertexBuffer vb;
            public IndexBuffer ib;

            public uint vertexCount;

            public object localVertexBuffer;

            public RendererStats rendererStats;

            public void Dispose()
            {
                if (va != null) va.Dispose();
                if (vb != null) vb.Dispose();
                if (ib != null) ib.Dispose();

                localVertexBuffer = null;
            }
        }

        public static void BeginFrame()
        {
            Batch.BeginBatch();
        }

        public static void EndFrame()
        {
            Batch.EndBatch();
            Batch.Flush();
        }

        private static bool initialized = false;

        public class MeshRenderer
        {
            static List<Model> registeredOpaque = new List<Model> { };
            static List<Model> registeredTransparent = new List<Model> { };

            public static void RegisterModel(Model model, bool transparent = false)
            {
                if (!transparent)
                    registeredOpaque.Add(model);
                else
                    registeredTransparent.Add(model);
            }
            public static void UnregisterMesh(Model model)
            {
                if (!registeredOpaque.Contains(model) || !registeredTransparent.Contains(model))
                    throw new Exception("mesh instance not registered!");

                if (registeredOpaque.Contains(model))
                    registeredOpaque.Remove(model);
                if (registeredTransparent.Contains(model))
                    registeredTransparent.Remove(model);
            }

            static public unsafe void DrawMesh(CrossEngine.Rendering.Geometry.Mesh mesh, Camera camera, Transform transform, Material material)
            {
                material.Shader.Use();

                material.Bind(); // binds custom variables

                material.Shader.SetMat4("projection", camera.ProjectionMatrix);
                material.Shader.SetMat4("view", camera.ViewMatrix);
                material.Shader.SetMat4("model", transform.TransformMatrix);

                mesh.va.Bind();
                mesh.ib.Bind();

                glDrawElements(GL_TRIANGLES, mesh.ib.GetCount(), GL_UNSIGNED_INT, (void*)0);
            }

            public static void Render()
            {
                
            }
        }

        

        public class Batch
        {
            #region Batch/Sprite
            struct BatchVertex
            {
                public Vector3 position;
                public Vector4 color;
                public Vector2 texCoords;
                public float texIndex;
            }

            // constansts
            const uint batchMaxQuads = 1000;
            const uint batchMaxVertices = batchMaxQuads * 4;
            const uint batchMaxIndices = batchMaxQuads * 6;
            const uint batchMaxTextures = 8;

            static RendererData batchRendererData;

            static Shader batchShader;
            static uint[] batchTextureSlots = null;
            static uint batchTextureSlotIndex = 1;
            static Texture batchWhiteTexture;

            public static unsafe void Init()
            {
                // cpu side
                batchShader = AssetManager.Shaders.GetShader("shaders/batch/batch.shader");

                batchShader.Use();
                int[] samplers = new int[batchMaxTextures];
                for (int i = 0; i < batchMaxTextures; i++)
                    samplers[i] = i;
                batchShader.SetIntVec("uTextures", samplers);

                batchRendererData.localVertexBuffer = new BatchVertex[batchMaxVertices];

                // gpu side
                batchRendererData.va = new VertexArray();
                batchRendererData.va.Bind(); // needs to be bound!!!

                batchRendererData.vb = new VertexBuffer((void*)null, (int)batchMaxVertices * sizeof(BatchVertex), BufferUsage.DynamicDraw);
                VertexBufferLayout layout = new VertexBufferLayout();
                layout.Add(typeof(BatchVertex));
                batchRendererData.va.AddBuffer(batchRendererData.vb, layout);

                uint[] indices = new uint[batchMaxIndices];
                uint offset = 0;
                for (int i = 0; i < batchMaxIndices; i += 6)
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
                    batchRendererData.ib = new IndexBuffer(indicesp, (int)batchMaxIndices);

                batchWhiteTexture = new Texture(0xffffffff);

                batchTextureSlots = new uint[batchMaxTextures];
                batchTextureSlots[0] = batchWhiteTexture.id;

                for (int i = 1; i < batchMaxTextures; i++)
                    batchTextureSlots[i] = 0;
            }

            public static void Shutdown()
            {
                batchRendererData.va.Dispose();
                batchRendererData.vb.Dispose();
                batchRendererData.ib.Dispose();

                batchWhiteTexture.Dispose();

                batchRendererData.localVertexBuffer = null;
            }

            public static void BeginBatch()
            {

            }

            public static unsafe void EndBatch()
            {
                fixed (BatchVertex* quadBufferp = &((BatchVertex[])batchRendererData.localVertexBuffer)[0])
                {
                    batchRendererData.vb.SetData(quadBufferp, sizeof(BatchVertex) * (int)batchRendererData.vertexCount);
                }
            }

            public static unsafe void Flush()
            {
                if (batchRendererData.vertexCount > 0)
                {
                    batchShader.Use();

                    batchShader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                    batchShader.SetMat4("view", ActiveCamera.camera.ViewMatrix);
                    batchShader.SetMat4("model", Matrix4x4.Identity);

                    for (int i = 0; i < batchTextureSlotIndex; i++)
                    {
                        glActiveTexture(GL_TEXTURE0 + i);
                        glBindTexture(GL_TEXTURE_2D, batchTextureSlots[i]);
                    }

                    //Renderer.Draw(quadVA, quadIB, quadShader, );
                    batchRendererData.va.Bind();
                    batchRendererData.ib.Bind();

                    glDrawElements(GL_TRIANGLES, (int)batchRendererData.vertexCount / 4 * 6, GL_UNSIGNED_INT, (void*)0);

                    batchRendererData.vertexCount = 0;
                    batchTextureSlotIndex = 1; // must be 1 because one pixel white texture

                    //stats
                    batchRendererData.rendererStats.drawCount++;
                }
            }

            #region Draw Methods
            static void SetupQuad(BatchVertex[] quads, ref uint index, Matrix4x4 matrix, Vector4 color, float textureIndex)
            {
                Vector3 position;

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

            static void SetupSpriteQuad(BatchVertex[] quads, ref uint index, Vector2[] texCoords, Matrix4x4 matrix, Vector4 color, float textureIndex)
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

            static public void DrawQuad(Vector3 position, Vector2 size, Vector4 color, Quaternion? rotation = null)
            {
                if (batchRendererData.vertexCount >= batchMaxVertices)
                {
                    EndBatch();
                    Flush();
                    BeginBatch();
                }

                Matrix4x4 transform = Matrix4x4.Identity;
                if (rotation != null)
                    transform = Matrix4x4.CreateFromQuaternion((Quaternion)rotation);
                transform = Matrix4x4.CreateScale(new Vector3(size, 1.0f)) * transform * Matrix4x4.CreateTranslation(position);

                SetupQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, transform, color, 0.0f);

                // statistics
                batchRendererData.rendererStats.itemCount++;
            }

            static public void DrawTexturedQuad(Vector3 position, Vector2 size, Texture texture, Vector4? tintColor = null, Quaternion? rotation = null)
            {
                if (batchRendererData.vertexCount >= batchMaxVertices)
                {
                    EndBatch();
                    Flush();
                    BeginBatch();
                }

                float textureIndex = 0.0f;
                for (int i = 1; i < batchTextureSlotIndex; i++)
                {
                    if (batchTextureSlots[i] == texture.id)
                    {
                        textureIndex = i;
                        break;
                    }
                }

                if (textureIndex == 0.0f)
                {
                    textureIndex = batchTextureSlotIndex;
                    batchTextureSlots[batchTextureSlotIndex] = texture.id;
                    batchTextureSlotIndex++;
                }

                Matrix4x4 transform = Matrix4x4.Identity;
                if (rotation != null)
                    transform = Matrix4x4.CreateFromQuaternion((Quaternion)rotation);
                transform = Matrix4x4.CreateScale(new Vector3(size, 1.0f)) * transform * Matrix4x4.CreateTranslation(position);

                if (tintColor != null)
                    SetupQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, transform, (Vector4)tintColor, textureIndex);
                else
                    SetupQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, transform, new Vector4(1), textureIndex);

                // statistics
                batchRendererData.rendererStats.itemCount++;
            }

            static public void DrawBillboard(Vector3 position, Vector2 size, Vector4 color, Quaternion? rotation = null)
            {
                if (batchRendererData.vertexCount >= batchMaxVertices)
                {
                    EndBatch();
                    Flush();
                    BeginBatch();
                }

                //Matrix4x4.CreateBillboard

                Matrix4x4 matrix = Matrix4x4.Identity;
                if (rotation != null)
                    matrix = Matrix4x4.CreateFromQuaternion((Quaternion)rotation);
                matrix = Matrix4x4.CreateScale(new Vector3(size, 1.0f)) * matrix * Matrix4x4Extension.CreateBillboard(ActiveCamera.camera.Right, ActiveCamera.camera.Up, ActiveCamera.camera.Front, position);

                SetupQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, matrix, color, 0.0f);

                // statistics
                batchRendererData.rendererStats.itemCount++;
            }

            static public void DrawTexturedBillboard(Vector3 position, Vector2 size, Texture texture, Vector4? tintColor = null, Quaternion? rotation = null)
            {
                if (batchRendererData.vertexCount >= batchMaxVertices)
                {
                    EndBatch();
                    Flush();
                    BeginBatch();
                }

                float textureIndex = 0.0f;
                for (int i = 1; i < batchTextureSlotIndex; i++)
                {
                    if (batchTextureSlots[i] == texture.id)
                    {
                        textureIndex = i;
                        break;
                    }
                }

                if (textureIndex == 0.0f)
                {
                    textureIndex = batchTextureSlotIndex;
                    batchTextureSlots[batchTextureSlotIndex] = texture.id;
                    batchTextureSlotIndex++;
                }

                Matrix4x4 matrix = Matrix4x4.Identity;
                if (rotation != null)
                    matrix = Matrix4x4.CreateFromQuaternion((Quaternion)rotation);
                matrix = Matrix4x4.CreateScale(new Vector3(size, 1.0f)) * matrix * Matrix4x4Extension.CreateBillboard(ActiveCamera.camera.Right, ActiveCamera.camera.Up, ActiveCamera.camera.Front, position);

                if (tintColor != null)
                    SetupQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, matrix, (Vector4)tintColor, textureIndex);
                else
                    SetupQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, matrix, new Vector4(1), textureIndex);

                // statistics
                batchRendererData.rendererStats.itemCount++;
            }

            static public void DrawSprite(Vector3 position, Vector2 size, Sprite sprite, Vector4 tintColor, Quaternion? rotation = null)
            {
                if (batchRendererData.vertexCount >= batchMaxVertices)
                {
                    EndBatch();
                    Flush();
                    BeginBatch();
                }

                float textureIndex = 0.0f;
                for (int i = 1; i < batchTextureSlotIndex; i++)
                {
                    if (batchTextureSlots[i] == sprite.texture.id)
                    {
                        textureIndex = i;
                        break;
                    }
                }

                if (textureIndex == 0.0f)
                {
                    textureIndex = batchTextureSlotIndex;
                    batchTextureSlots[batchTextureSlotIndex] = sprite.texture.id;
                    batchTextureSlotIndex++;
                }

                Matrix4x4 transform = Matrix4x4.Identity;
                if (rotation != null)
                    transform = Matrix4x4.CreateFromQuaternion((Quaternion)rotation);
                transform = Matrix4x4.CreateScale(new Vector3(size, 1.0f)) * transform * Matrix4x4.CreateTranslation(position);

                SetupSpriteQuad((BatchVertex[])batchRendererData.localVertexBuffer, ref batchRendererData.vertexCount, sprite.texCoords, transform, tintColor, textureIndex);

                // statistics
                batchRendererData.rendererStats.itemCount++;
            }
            #endregion
            #endregion
        }

        public class Text
        {
            #region Text
            struct TextVertex
            {
                public Vector3 position;
                public Vector2 texCoords;
            }

            // constansts
            const uint textMaxQuads = 1000;
            const uint textMaxVertices = textMaxQuads * 4;
            const uint textMaxIndices = textMaxQuads * 6;

            static RendererData textRendererData;

            static Shader textShader;

            public static unsafe void Init()
            {
                // cpu side
                textShader = AssetManager.Shaders.GetShader("shaders/text/distance.shader");

                textRendererData.localVertexBuffer = new TextVertex[textMaxVertices];

                // gpu side
                textRendererData.va = new VertexArray();
                textRendererData.va.Bind(); // needs to be bound!!!

                textRendererData.vb = new VertexBuffer((void*)null, (int)textMaxVertices * sizeof(TextVertex), BufferUsage.DynamicDraw);
                VertexBufferLayout layout = new VertexBufferLayout();
                layout.Add(typeof(TextVertex));
                textRendererData.va.AddBuffer(textRendererData.vb, layout);

                uint[] indices = new uint[textMaxIndices];
                uint offset = 0;
                for (int i = 0; i < textMaxIndices; i += 6)
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
                    textRendererData.ib = new IndexBuffer(indicesp, (int)textMaxIndices);
            }

            public static void Shutdown()
            {
                textRendererData.va.Dispose();
                textRendererData.vb.Dispose();
                textRendererData.ib.Dispose();

                textRendererData.localVertexBuffer = null;
            }

            static unsafe void Flush(Texture texture, Vector4 foreColor, Vector4 backColor, float thickness, float edge, Vector3 position, Quaternion? rotation, Vector3 scale)
            {
                if (textRendererData.vertexCount > 0)
                {
                    fixed (TextVertex* quadBufferp = &((TextVertex[])textRendererData.localVertexBuffer)[0])
                    {
                        textRendererData.vb.SetData(quadBufferp, sizeof(TextVertex) * (int)textRendererData.vertexCount);
                    }

                    textShader.Use();

                    texture.BindTo(0);
                    textShader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                    textShader.SetMat4("view", ActiveCamera.camera.ViewMatrix);
                    textShader.SetMat4("model", Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion((rotation != null) ? (Quaternion)rotation : Quaternion.Identity) * Matrix4x4.CreateTranslation(position));
                    textShader.SetVec4("uForeColor", foreColor);
                    textShader.SetVec4("uBackColor", backColor);
                    textShader.SetFloat("uWidth", thickness);
                    textShader.SetFloat("uEdge", edge);

                    texture.Bind();

                    textRendererData.va.Bind();
                    textRendererData.ib.Bind();

                    glDisable(GL_DEPTH_TEST);
                    glDrawElements(GL_TRIANGLES, (int)textRendererData.vertexCount / 4 * 6, GL_UNSIGNED_INT, (void*)0);
                    glEnable(GL_DEPTH_TEST);

                    textRendererData.vertexCount = 0;
                }
            }

            #region Draw Methods
            static void SetupTextQuad(TextVertex[] quads, ref uint index, Vector2 offset, System.Drawing.RectangleF texCoords, Vector2 pixelSize)
            {
                // the Y is flipped
                Vector2 position;

                position = new Vector2(-0.5f, 0.5f);
                position *= Vector2Extension.FromSizeF(texCoords.Size);
                position += offset;
                position *= pixelSize;
                quads[index].position = new Vector3(position, 0.0f);
                quads[index].texCoords = new Vector2(texCoords.X, texCoords.Y);
                index++;

                position = new Vector2(0.5f, 0.5f);
                position *= Vector2Extension.FromSizeF(texCoords.Size);
                position += offset;
                position *= pixelSize;
                quads[index].position = new Vector3(position, 0.0f);
                quads[index].texCoords = new Vector2(texCoords.Right, texCoords.Y);
                index++;

                position = new Vector2(0.5f, -0.5f);
                position *= Vector2Extension.FromSizeF(texCoords.Size);
                position += offset;
                position *= pixelSize;
                quads[index].position = new Vector3(position, 0.0f);
                quads[index].texCoords = new Vector2(texCoords.Right, texCoords.Bottom);
                index++;

                position = new Vector2(-0.5f, -0.5f);
                position *= Vector2Extension.FromSizeF(texCoords.Size);
                position += offset;
                position *= pixelSize;
                quads[index].position = new Vector3(position, 0.0f);
                quads[index].texCoords = new Vector2(texCoords.X, texCoords.Bottom);
                index++;
            }

            static public void DrawText(string text, TextProperties textProps, Vector3 position, Vector2 size, Quaternion? rotation = null)
            {
                Vector2 offset = new Vector2();

                for (int i = 0; i < text.Length; i++)
                {
                    int charIndex = (int)text[i] - FontAtlasRenderer.firstChar;

                    if (text[i] == ' ')
                    {
                        offset.X += textProps.fontAtlas.characters[charIndex].Width + textProps.spacing.X;
                        continue;
                    }
                    if (text[i] == '\n')
                    {
                        offset.X = 0;
                        offset.Y -= (float)textProps.fontAtlas.font.Height / FontAtlasRenderer.textureAtlasSize.X + textProps.spacing.Y;
                        continue;
                    }

                    if (text[i] < FontAtlasRenderer.firstChar || text[i] > FontAtlasRenderer.lastChar)
                        continue;

                    SetupTextQuad((TextVertex[])textRendererData.localVertexBuffer, ref textRendererData.vertexCount, offset, textProps.fontAtlas.characters[charIndex], textProps.fontAtlas.pixelSize);

                    offset.X += textProps.fontAtlas.characters[charIndex].Width + textProps.spacing.X;

                    if (textRendererData.vertexCount >= textMaxVertices)
                    {
                        Flush(textProps.fontAtlas.texture, textProps.foreColor, textProps.backColor, textProps.thickness, textProps.edge, position, rotation, new Vector3(size, 1.0f));
                    }
                }
                Flush(textProps.fontAtlas.texture, textProps.foreColor, textProps.backColor, textProps.thickness, textProps.edge, position, rotation, new Vector3(size, 1.0f));
            }
            #endregion
            #endregion
        }

        public class InstanceRenderer
        {
            static Dictionary<CrossEngine.Rendering.Geometry.Mesh, List<CrossEngine.Rendering.Geometry.MeshInstance>> registered; // quite not sure

            const uint instanceMaxInstances = 1000;

            static RendererData instanceRendererData;

            public static unsafe void Init()
            {
                registered = new Dictionary<CrossEngine.Rendering.Geometry.Mesh, List<CrossEngine.Rendering.Geometry.MeshInstance>> { };

                instanceRendererData.localVertexBuffer = new Matrix4x4[instanceMaxInstances];

                instanceRendererData.vb = new VertexBuffer((void*)null, (int)instanceMaxInstances * sizeof(Matrix4x4), BufferUsage.StreamDraw); // may also be referred to as a buffer of matrices with model transforms

                //TransparentInstance
            }

            public static void Shutdown()
            {
                instanceRendererData.vb.Dispose();

                instanceRendererData.localVertexBuffer = null;
            }

            static unsafe void Flush(CrossEngine.Rendering.Geometry.Mesh mesh)
            {
                if (instanceRendererData.vertexCount <= 0)
                    return;

                //instanceVA.Bind();
                fixed (void* p = &((Matrix4x4[])instanceRendererData.localVertexBuffer)[0])
                    instanceRendererData.vb.SetData(p, sizeof(Matrix4x4) * (int)instanceMaxInstances);

                //material.shader.Use();
                //material.Bind();
                //
                //material.shader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                //material.shader.SetMat4("view", ActiveCamera.camera.ViewMatrix);

                Shader shader = AssetManager.Shaders.GetShader("shaders/instancing/lit_instancing.shader");
                shader.Use();
                shader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                shader.SetMat4("view", ActiveCamera.camera.ViewMatrix);

                mesh.va.Bind();
                mesh.ib.Bind();

                glDrawElementsInstanced(GL_TRIANGLES, mesh.ib.GetCount(), GL_UNSIGNED_INT, (void*)0, (int)instanceRendererData.vertexCount);

                instanceRendererData.vertexCount = 0;

                instanceRendererData.rendererStats.drawCount++;
            }

            static public unsafe void Render()
            {
                foreach (KeyValuePair<CrossEngine.Rendering.Geometry.Mesh, List<CrossEngine.Rendering.Geometry.MeshInstance>> pair in registered)
                {
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        if (instanceRendererData.vertexCount >= instanceMaxInstances)
                        {
                            Flush(pair.Key);
                        }
                        ((Matrix4x4[])instanceRendererData.localVertexBuffer)[instanceRendererData.vertexCount++] = pair.Value[i].transform.TransformMatrix;

                        instanceRendererData.rendererStats.itemCount++;
                    }
                    Flush(pair.Key);
                }
            }

            static public unsafe void RegisterInstance(MeshInstance meshInstance)
            {
                if (registered.ContainsKey(meshInstance.mesh))
                    registered[meshInstance.mesh].Add(meshInstance);
                else
                {
                    // set attribute pointers for matrix (4 times vec4)
                    VertexBufferLayout layout = new VertexBufferLayout();
                    layout.Add(typeof(Vector4));
                    layout.Add(typeof(Vector4));
                    layout.Add(typeof(Vector4));
                    layout.Add(typeof(Vector4));
                    layout.elements[0].divisor = 1;
                    layout.elements[1].divisor = 1;
                    layout.elements[2].divisor = 1;
                    layout.elements[3].divisor = 1;

                    meshInstance.mesh.va.AddBuffer(instanceRendererData.vb, layout, 3); // extension

                    //glEnableVertexAttribArray(3);
                    //glVertexAttribPointer(3, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)0);
                    //glEnableVertexAttribArray(4);
                    //glVertexAttribPointer(4, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)(sizeof(Vector4)));
                    //glEnableVertexAttribArray(5);
                    //glVertexAttribPointer(5, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)(2 * sizeof(Vector4)));
                    //glEnableVertexAttribArray(6);
                    //glVertexAttribPointer(6, 4, GL_FLOAT, false, sizeof(Matrix4x4), (void*)(3 * sizeof(Vector4)));
                    //
                    //glVertexAttribDivisor(3, 1);
                    //glVertexAttribDivisor(4, 1);
                    //glVertexAttribDivisor(5, 1);
                    //glVertexAttribDivisor(6, 1);

                    registered.Add(meshInstance.mesh, new List<CrossEngine.Rendering.Geometry.MeshInstance> { meshInstance });
                }
            }

            static public unsafe void UnregisterInstance(MeshInstance meshInstance)
            {
                if (!registered.ContainsKey(meshInstance.mesh))
                    throw new Exception("instance mesh not registered!");
                if (!registered[meshInstance.mesh].Contains(meshInstance))
                    throw new Exception("instance not registered!");

                registered[meshInstance.mesh].Remove(meshInstance);
            }
        }

        public class ParticleRenderer
        {
            static List<ParticleSystem> registeredParticleSystems = new List<ParticleSystem> { };

            struct ParticleData
            {
                public Matrix4x4 matrix;
                public Vector4 color;
                public Vector4 textureOffsets;
            }

            static RendererData particleRendererData;
            const uint maxParticles = 1024;

            static VertexBuffer quadVB;
            static IndexBuffer quadIB;
            static VertexArray quadVA;

            static Texture whiteTexture;
            static Shader particleShader;

            struct ParticleVertex
            {
                public Vector3 position;
                public Vector2 texCoords;

                public ParticleVertex(Vector3 pos, Vector2 texCoords)
                {
                    this.position = pos;
                    this.texCoords = texCoords;
                }
            }

            public static unsafe void Init()
            {
                particleRendererData.vb = new VertexBuffer(null, sizeof(ParticleData) * (int)maxParticles, BufferUsage.StreamDraw);

                ParticleVertex[] vertices = {
                    new ParticleVertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0.0f, 0.0f)),
                    new ParticleVertex(new Vector3(0.5f, -0.5f, 0.0f), new Vector2(1.0f, 0.0f)),
                    new ParticleVertex(new Vector3(0.5f, 0.5f, 0.0f), new Vector2(1.0f, 1.0f)),
                    new ParticleVertex(new Vector3(-0.5f, 0.5f, 0.0f), new Vector2(0.0f, 1.0f)),
                };
                fixed (void* p = &vertices[0])
                    quadVB = new VertexBuffer(p, sizeof(ParticleVertex) * vertices.Length, BufferUsage.StaticDraw);

                quadVA = new VertexArray();
                VertexBufferLayout layout = new VertexBufferLayout();
                layout.Add(typeof(Vector3));
                layout.Add(typeof(Vector2));
                quadVA.AddBuffer(quadVB, layout);

                layout = new VertexBufferLayout();

                layout.Add(typeof(Vector4)); // matrix part
                layout.Add(typeof(Vector4));
                layout.Add(typeof(Vector4));
                layout.Add(typeof(Vector4));

                layout.Add(typeof(Vector4)); // color
                layout.Add(typeof(Vector4)); // texture offset
                layout.SetAllDivisors(1);
                quadVA.AddBuffer(particleRendererData.vb, layout, 2);

                uint[] indices = {
                    0,
                    2,
                    1,

                    3,
                    2,
                    0,
                };
                fixed (uint* p = &indices[0])
                    quadIB = new IndexBuffer(p, indices.Length, BufferUsage.StaticDraw);

                particleRendererData.localVertexBuffer = new ParticleData[maxParticles];
                particleRendererData.vertexCount = 0;

                whiteTexture = new Texture(0xffffffff);

                particleShader = AssetManager.Shaders.GetShader("shaders/instancing/pptestshader.shader");
            }

            public static void Shutdown()
            {
                quadVB.Dispose();
                quadIB.Dispose();
                quadVA.Dispose();

                whiteTexture.Dispose();

                particleRendererData.Dispose();
            }

            #region Drawing
            public static void Render()
            {
                Pass();
            }

            static void Pass()
            {
                registeredParticleSystems.Sort(delegate (ParticleSystem x, ParticleSystem y)
                {
                    if (x.particleEmitter == null || y.particleEmitter == null) return 0;

                    float xlen = Vector3.DistanceSquared(ActiveCamera.camera.Transform.Position, x.particleEmitter.transform.Position);
                    float ylen = Vector3.DistanceSquared(ActiveCamera.camera.Transform.Position, y.particleEmitter.transform.Position);
                    if (xlen < ylen) return -1;
                    else if (xlen > ylen) return 1;
                    else return 0;
                });

                for (int systemIndex = 0; systemIndex < registeredParticleSystems.Count; systemIndex++)
                {
                    //uint[] particleIndices = registeredParticleSystems[systemIndex].SortMeDaPizza(ActiveCamera.camera.Transform.Position);
                    ParticleSystem.Particle[] particlePool = registeredParticleSystems[systemIndex].GetParticlePool();
                    for (int i = 0; i < particlePool.Length; i++)
                    {
                        if (!particlePool[i].active)
                            continue;

                        float particleLife = particlePool[i].lifeRemaining / particlePool[i].lifeTime;

                        Vector3 size = particlePool[i].sizeGradient.SampleVector3(particleLife);
                        size = size - size * particlePool[i].sizeVariation;
                        Vector4 color = particlePool[i].colorGradient.SampleVector4(particleLife);
                        if (particlePool[i].hsvVariation != Vector3.Zero)
                        {
                            color = new Vector4(Vector3Extension.RGBToHSV(color.XYZ()), color.W);
                            color.X += particlePool[i].hsvVariation.X;
                            color.Y += particlePool[i].hsvVariation.Y;
                            color.Z += particlePool[i].hsvVariation.Z;
                            color = new Vector4(Vector3Extension.HSVToRGB(color.XYZ()), color.W);
                        }
                        Matrix4x4 modelMat = Matrix4x4.CreateScale(size) * Matrix4x4.CreateRotationZ(particlePool[i].rotation) * Matrix4x4Extension.CreateBillboard(ActiveCamera.camera.Right, ActiveCamera.camera.Up, ActiveCamera.camera.Front, particlePool[i].position);

                        ((ParticleData[])particleRendererData.localVertexBuffer)[particleRendererData.vertexCount].matrix = modelMat;
                        ((ParticleData[])particleRendererData.localVertexBuffer)[particleRendererData.vertexCount].color = color;
                        //((ParticleData[])particleRendererData.localVertexBuffer)[particleRendererData.vertexCount].textureOffsets = registeredParticleSystems[systemIndex].textureAtlas != null ? registeredParticleSystems[systemIndex].textureAtlas.GetTextureOffsets(0) : new Vector4(0, 0, 1, 1);
                        ((ParticleData[])particleRendererData.localVertexBuffer)[particleRendererData.vertexCount].textureOffsets = new Vector4(0, 0, 1, 1);
                        
                        particleRendererData.vertexCount++;

                        particleRendererData.rendererStats.itemCount++;

                        if (particleRendererData.vertexCount >= maxParticles)
                        {
                            Flush(registeredParticleSystems[systemIndex]);
                        }
                    }
                    Flush(registeredParticleSystems[systemIndex]);
                }
            }

            static unsafe void Flush(ParticleSystem currentSystem)
            {
                if (particleRendererData.vertexCount <= 0)
                    return;

                //instanceVA.Bind();
                fixed (void* p = &((ParticleData[])particleRendererData.localVertexBuffer)[0])
                    particleRendererData.vb.SetData(p, sizeof(ParticleData) * (int)particleRendererData.vertexCount);

                //material.shader.Use();
                //material.Bind();
                //
                //material.shader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                //material.shader.SetMat4("view", ActiveCamera.camera.ViewMatrix);

                particleShader.Use();
                particleShader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                particleShader.SetMat4("view", ActiveCamera.camera.ViewMatrix);
                if (currentSystem.textureAtlas != null)
                    currentSystem.textureAtlas.Texture.BindTo(0);

                quadVA.Bind();
                quadIB.Bind();

                glDrawElementsInstanced(GL_TRIANGLES, quadIB.GetCount(), GL_UNSIGNED_INT, (void*)0, (int)particleRendererData.vertexCount);

                particleRendererData.vertexCount = 0;

                particleRendererData.rendererStats.drawCount++;
            }
            #endregion

            #region Registration
            public static void RegisterParticleSystem(ParticleSystem particleSystem)
            {
                if (registeredParticleSystems.Contains(particleSystem))
                    throw new Exception("paricle system is already registered!");

                registeredParticleSystems.Add(particleSystem);
            }

            public static void UnregisterParticleSystem(ParticleSystem particleSystem)
            {
                if (!registeredParticleSystems.Contains(particleSystem))
                    throw new Exception("paricle system is not registered!");

                registeredParticleSystems.Remove(particleSystem);
            }
            #endregion

            #region Material
            #endregion
        }

        #region Stats
        //public RendererStats GetLineRendererStats()
        //{
        //    return lineRendererData.rendererStats;
        //}
        #endregion
        */
    }

    public enum PolygonMode : int
    {
        Fill = GL_FILL,
        Line = GL_LINE,
        Point = GL_POINT
    }

    public enum DrawMode : int
    {
        Lines = GL_LINES,
        Traingles = GL_TRIANGLES,
    }

    public enum BlendFunc : int
    {
        OneMinusSrcAlpha = GL_ONE_MINUS_SRC_ALPHA
    }

    public enum DepthFunc : int
    {
        Default = Less,

        Never = GL_NEVER,
        Less = GL_LESS,
        Equal = GL_EQUAL,
        LessEqual = GL_LEQUAL,
        Greater = GL_GREATER,
        NotEqual = GL_NOTEQUAL,
        GreaterEqual = GL_GEQUAL,
        Always = GL_ALWAYS,
    }
}
