﻿//#define AVOID_SPACE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.Debugging;
using CrossEngine.Assets.Loaders;

namespace CrossEngine.Utils
{
    public static class TextRendererUtil
    {
        const int SymbolsCount = 95;
        public const float SymbolWidth = 12;
        public const float SymbolHeight = 16;

        static WeakReference<Texture> textTexture;
        static TextureAtlas textAtlas;

        public static void Init()
        {
            textTexture = TextureLoader.LoadTexture(Properties.Resources.DebugFontAtlas);
            var tex = textTexture.GetValue();
            tex.SetFilterParameter(FilterParameter.Nearest);
            textAtlas = new TextureAtlas(tex.Size, new Vector2(12, 16), SymbolsCount, margin: new Vector4(0, 0, 4, 0));
        }

        public static void Shutdown()
        {
            textTexture.Dispose();
            textTexture = null;
        }

        public static void DrawText(Matrix4x4 transform, string text, Vector4 color, int entID = 0)
        {
            if (text == null)
                return;

            int line = 0;
            // i hate matrices
            transform = Matrix4x4.CreateScale(new Vector3(SymbolWidth, SymbolHeight, 1)) * Matrix4x4.CreateTranslation(new Vector3(SymbolWidth / 2, -SymbolHeight / 2, 0)) * transform;
            int column = 0;
            for (int i = 0; i < text.Length; i++)
            {
                // what was i drinking while writing this code is uncertain
                // special characters
#if AVOID_SPACE
                if (text[i] == ' ')
                {
                    column++;
                    continue;
                }
#endif
                if (text[i] == '\n')
                {
                    line++;
                    column = 0;
                    continue;
                }

                Matrix4x4 matrix = Matrix4x4.CreateTranslation(new Vector3(column, -line, 0)) * transform;

                column++;

                int chord = text[i] - ' ';
                // clamp
                if (chord < 0 || chord >= SymbolsCount) chord = '?' - ' ';

                Renderer2D.DrawTexturedQuad(matrix,
                                            textTexture,
                                            color,
                                            textAtlas.GetTextureOffsets(chord),
                                            entID);
            }
        }
    }
}