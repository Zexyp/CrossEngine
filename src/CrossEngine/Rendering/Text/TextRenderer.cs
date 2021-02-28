using System;
using static OpenGL.GL;

using System.Numerics;
using System.Drawing;
using System.Collections.Generic;

using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Cameras;
using CrossEngine.MainLoop;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Text
{
    public class TextRenderer
    {
        // have in mind that the indices might not work so don't be mad!!!

        struct TextVertex
        {
            public Vector3 position;
            public Vector2 texCoords;
        }

        #region Renderer Data
        // buffers
        const uint maxQuadCount = 1000;
        const uint maxVertexCount = maxQuadCount * 4;
        const uint maxIndexCount = maxQuadCount * 6;

        static VertexArray quadVA = null;
        static VertexBuffer quadVB = null;
        static IndexBuffer quadIB = null;

        static uint indexCount = 0;

        static TextVertex[] quadBuffer = null;
        static int workingPos = 0;

        // stats
        #endregion

        static Shader textShader;

        //static Dictionary<TextData, TextVertex[]> dataCache = new Dictionary<TextData, TextVertex[]> { };

        static public unsafe void Init()
        {
            textShader = AssetManager.Shaders.GetShader("shaders/text/distance.shader");

            textShader.Use();

            quadBuffer = new TextVertex[maxVertexCount];

            // gpu side stuff
            quadVA = new VertexArray();
            quadVA.Bind(); // needs to be bound!!!

            quadVB = new VertexBuffer((void*)null, (int)maxVertexCount * sizeof(TextVertex), true); // limited

            VertexBufferLayout layout = new VertexBufferLayout();
            layout.Add(typeof(TextVertex));
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
        }

        static public unsafe void Flush(Texture texture, Vector4 foreColor, Vector4 backColor, float thickness, float edge, Vector3 position, Quaternion rotation, Vector3 size)
        {
            if (workingPos > 0)
            {
                int sizeLen = workingPos * sizeof(TextVertex);
                fixed (TextVertex* quadBufferp = &quadBuffer[0])
                {
                    quadVB.SetData(quadBufferp, sizeLen);
                }

                textShader.Use();

                texture.BindTo(0);
                textShader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                textShader.SetMat4("view", ActiveCamera.camera.ViewMatrix);
                textShader.SetMat4("model", Matrix4x4.CreateScale(size) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position));
                textShader.SetVec4("uForeColor", foreColor);
                textShader.SetVec4("uBackColor", backColor);
                textShader.SetFloat("uWidth", thickness);
                textShader.SetFloat("uEdge", edge);

                texture.Bind();

                quadVA.Bind();
                quadIB.Bind();

                glDisable(GL_DEPTH_TEST);
                glDrawElements(GL_TRIANGLES, (int)indexCount, GL_UNSIGNED_INT, (void*)0);
                glEnable(GL_DEPTH_TEST);

                indexCount = 0;
            }

            workingPos = 0;

        }

        static public void DrawText(string text, TextProperties textProps, Vector3 position, Vector2 size, Quaternion rotation)
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
                if(text[i] == '\n')
                {
                    offset.X = 0;
                    offset.Y -= (float)textProps.fontAtlas.font.Height / FontAtlasRenderer.textureAtlasSize.Height + textProps.spacing.Y;
                    continue;
                }

                if (text[i] < FontAtlasRenderer.firstChar || text[i] > FontAtlasRenderer.lastChar)
                    continue;
                
                SetupTextQuad(ref quadBuffer, ref workingPos, offset, textProps.fontAtlas.characters[charIndex], textProps.fontAtlas.pixelSize);
                indexCount += 6;
            
                offset.X += textProps.fontAtlas.characters[charIndex].Width + textProps.spacing.X;
            
                if (indexCount >= maxIndexCount)
                {
                    Flush(textProps.fontAtlas.texture, textProps.foreColor, textProps.backColor, textProps.thickness, textProps.edge, position, rotation, new Vector3(size, 0.0f));
                }
            }
            
            Flush(textProps.fontAtlas.texture, textProps.foreColor, textProps.backColor, textProps.thickness, textProps.edge, position, rotation, new Vector3(size, 0.0f));
        }

        static void SetupTextQuad(ref TextVertex[] quads, ref int index, Vector2 offset, RectangleF texCoords, Vector2 pixelSize)
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
    }
}
