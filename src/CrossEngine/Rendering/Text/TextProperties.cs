using System;
using static OpenGL.GL;

using System.Numerics;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Generic;

using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Texturing;

// TODO:
// TextRenderer (just a batch renderer)

namespace CrossEngine.Rendering.Text
{
    public class TextProperties
    {
        public Vector4 foreColor = Vector4.One;
        public Vector4 backColor = Vector4.UnitW;

        Font font;

        public FontAtlas fontAtlas;

        public Vector2 spacing;

        public float thickness = 0.475f;
        public float edge = 0.05f;

        public TextProperties(Font font = null)
        {
            if (font == null)
                this.font = new Font("Consolas", 24); //debug
            else
                this.font = font;
            fontAtlas = AssetManager.Fonts.GetFontAtlas(this.font);
        }
    }

    /*
    class UIText
    {
        public Vector4 foreColor = Vector4.One;
        public Vector4 backColor = Vector4.UnitW;

        string text = "";

        public Transform transform = new Transform();

        Mesh mesh;
        Shader shader;

        //bool dynamicDraw;
        //int maxDynamicLenght = 256;

        public SizeF sizeOfText = new SizeF(0, 0);

        Font font = new Font("Consolas", 24.0f);

        FontAtlas fontAtlas;

        public UIText(string text = "")
        {
            fontAtlas = FontManager.GetFontAtlas(font); // also initializes the characters
            shader = new Shader(FileEnvironment.ResourceFolder + "shaders/vertex/model.shader", FileEnvironment.ResourceFolder + "shaders/text/text.shader");
            
            if (text != "") // can have initial text geometry
                SetText(text);
        }

        public void SetText(string text)
        {
            this.text = text;
            sizeOfText = new SizeF(0, 0);

            List<Vertex> vertices = new List<Vertex> { };
            List<uint> indices = new List<uint> { };

            float offsetX = 0.0f;
            int offsetY = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (!(text[i] != '\n'))
                {
                    offsetY++;
                    offsetX = 0.0f;
                }
                else
                {
                    RectangleF currentRect;
                    if ((int)text[i] < (int)FontAtlasRenderer.lastChar)
                        currentRect = fontAtlas.characters[(int)text[i] - FontAtlasRenderer.firstChar];
                    else
                    {
                        currentRect = fontAtlas.characters[0];
                        Log.Warn("invalid text character");
                    }

                    vertices.AddRange(new List<Vertex> {
                    new Vertex(new Vector3(offsetX,                     offsetY * currentRect.Height, 0.0f),       new Vector3(0.0f), new Vector2(currentRect.X, currentRect.Y)),
                    new Vertex(new Vector3(offsetX + currentRect.Width, offsetY * currentRect.Height, 0.0f),       new Vector3(0.0f), new Vector2(currentRect.Right, currentRect.Y)),
                    new Vertex(new Vector3(offsetX,                     (offsetY + 1) * currentRect.Height, 0.0f), new Vector3(0.0f), new Vector2(currentRect.X, currentRect.Bottom)),
                    new Vertex(new Vector3(offsetX + currentRect.Width, (offsetY + 1) * currentRect.Height, 0.0f), new Vector3(0.0f), new Vector2(currentRect.Right, currentRect.Bottom))
                    });

                    uint indexOffset = (uint)(i - offsetY) * 4;
                    indices.AddRange(new List<uint> {
                    0 + indexOffset, 3 + indexOffset, 1 + indexOffset,
                    0 + indexOffset, 2 + indexOffset, 3 + indexOffset
                    });

                    offsetX += currentRect.Width;

                    sizeOfText.Width = Math.Max(sizeOfText.Width, offsetX + currentRect.Width);
                    sizeOfText.Height = Math.Max(sizeOfText.Height, (offsetY + 1) * currentRect.Height);
                }
            }

            if(mesh != null)
            {
                mesh.SetGeomtryData(vertices, indices);
            }
            else
            {
                mesh = new Mesh(vertices, indices, new List<Texture> { fontAtlas.texture });
            }
        }

        public void Draw()
        {
            shader.Use();

            glBindTexture(GL_TEXTURE_2D, fontAtlas.texture.id);
            transform.scale = new Vector3(fontAtlas.pixelSize, 0); // debug
            shader.SetMat4("projection", Overlay.camera.GetProjectionMatrix());
            shader.SetMat4("view", Matrix4x4.Identity);
            shader.SetMat4("model", transform.GetTransformMatrix());
            shader.SetVec4("uForeColor", foreColor);
            shader.SetVec4("uBackColor", backColor);
            if(mesh != null)
                mesh.Draw(shader);
        }

        //public void SetDynamic(bool enable)
        //{
        //    dynamicDraw = enable;
        //}

        // !!!
        // batch rendering with multiple colors => every vertex has it's color
        // batch rendering with multiple textures => every vertex has it's texture index (and in shader is then sampler2D array) (glBindTextureUnit(index, texture), glUniform1iv(loc, count, samplers > array of texture units))
    }
    */
}
