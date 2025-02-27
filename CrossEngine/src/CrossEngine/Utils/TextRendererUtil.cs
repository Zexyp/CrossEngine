//#define AVOID_SPACE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.Debugging;
using CrossEngine.Loaders;

namespace CrossEngine.Utils
{
    public static class TextRendererUtil
    {
        public enum DrawMode
        {
            None = default, YUp, YDown
        }
        
        public struct TextRendererUtilData
        {
            public const char SymbolStart = ' ';
            public const int SymbolsCount = 95;
            public const float SymbolWidth = 12;
            public const float SymbolHeight = 16;
            public static readonly Vector4 SymbolMargin = new Vector4(0, 0, 4, 0);

            public WeakReference<Texture> AtlasTexture;
            public Vector4[] AtlasOffsets;
            public DrawMode Mode;
        }

        // atlas options
        public static TextRendererUtilData data = new TextRendererUtilData()
        {
            Mode = DrawMode.YUp
        };
        
        public static void Init()
        {
            data.AtlasTexture = TextureLoader.LoadTextureFromBytes(Properties.Resources.DebugFontAtlas);
            var tex = data.AtlasTexture.GetValue();
            tex.SetFilterParameter(FilterParameter.Nearest);
            data.AtlasOffsets = TextureAtlas.CreateOffsets(tex.Size, new Vector2(TextRendererUtilData.SymbolWidth, TextRendererUtilData.SymbolHeight), TextRendererUtilData.SymbolsCount, margin: TextRendererUtilData.SymbolMargin);
        }

        public static void Shutdown()
        {
            data.AtlasTexture.Dispose();
            data.AtlasTexture = null;
        }

        public static void DrawText(Matrix4x4 transform, string text, Vector4 color, int entID = 0)
        {
            if (text == null)
                return;

            int line = 0;
            // i hate matrices
            float scaleY = TextRendererUtilData.SymbolHeight, offsetY = TextRendererUtilData.SymbolHeight;
            switch (data.Mode)
            {
                case DrawMode.YUp:
                    scaleY = TextRendererUtilData.SymbolHeight;
                    offsetY = TextRendererUtilData.SymbolHeight;
                    break;
                case DrawMode.YDown:
                    scaleY = -TextRendererUtilData.SymbolHeight;
                    offsetY = -TextRendererUtilData.SymbolHeight;
                    break;
                default: Debug.Assert(false, "Invalid DrawMode"); break;
            };

            transform = Matrix4x4.CreateScale(new Vector3(TextRendererUtilData.SymbolWidth, scaleY, 1)) * Matrix4x4.CreateTranslation(new Vector3(TextRendererUtilData.SymbolWidth / 2, -offsetY / 2, 0)) * transform;
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
                if (chord < 0 || chord >= TextRendererUtilData.SymbolsCount)
                    chord = '?' - TextRendererUtilData.SymbolStart;

                Renderer2D.DrawTexturedQuad(matrix,
                                            data.AtlasTexture,
                                            color,
                                            data.AtlasOffsets[chord],
                                            entID);
            }
        }

        public static DrawMode SetMode(DrawMode mode)
        {
            var last = data.Mode;
            data.Mode = mode;
            return last;
        }
    }
}