using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.Assets;

namespace CrossEngine.Utils
{
    public static class TextRendererUtil
    {
        const int SymbolsCount = 95;
        static Ref<Texture> textTexture;
        static TextureAtlas textAtlas;

        static TextRendererUtil()
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                textTexture = TextureLoader.LoadTexture(Properties.Resources.DebugFontAtlas);
                textTexture.Value.SetFilterParameter(FilterParameter.Nearest);
                textAtlas = new TextureAtlas(textTexture.Value.Size, 16, 16, SymbolsCount);
            });
        }

        public static void DrawText(Matrix4x4 transform, string text, Vector4 color)
        {
            int line = 0;
            for (int i = 0; i < text.Length; i++)
            {
                Matrix4x4 matrix = Matrix4x4.CreateTranslation(new Vector3(i, line, 0)) * transform;

                // special characters
                if (text[i] == '\n')
                {
                    line++;
                    continue;
                }

                int chord = text[i] - ' ';
                // clamp
                if (chord < 0 || chord >= SymbolsCount) chord = '?' - ' ';

                Renderer2D.DrawTexturedQuad(matrix,
                                            textTexture,
                                            color,
                                            textAtlas?.GetTextureOffsets(chord) ?? new Vector4(0, 0, 1, 1));
            }
        }
    }
}
