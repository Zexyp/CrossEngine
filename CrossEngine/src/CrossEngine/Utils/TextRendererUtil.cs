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
using CrossEngine.Assets.Loaders;

namespace CrossEngine.Utils
{
    public static class TextRendererUtil
    {
        public enum DrawMode
        {
            None = default, YUp, YDown
        }
        
        public struct SymbolAtlas
        {
            public char SymbolStart;
            public int SymbolsCount;
            public float SymbolWidth;
            public float SymbolHeight;
            public Vector4 SymbolMargin;
            public byte[] SymbolAtlasData;
            // mby add texture here
        }
        
        // atlas options
        public static SymbolAtlas Data = new SymbolAtlas()
        {
            SymbolStart = ' ',
            SymbolsCount = 95,
            SymbolWidth = 12,
            SymbolHeight = 16,
            SymbolMargin = new Vector4(0, 0, 4, 0),
            SymbolAtlasData = Properties.Resources.DebugFontAtlas
        };
        
        public static DrawMode Mode = DrawMode.YUp; 

        static WeakReference<Texture> textTexture;
        static Vector4[] textAtlas;

        public static void Init()
        {
            textTexture = TextureLoader.LoadTexture(Data.SymbolAtlasData);
            var tex = textTexture.GetValue();
            tex.SetFilterParameter(FilterParameter.Nearest);
            textAtlas = TextureAtlas.CreateOffsets(tex.Size, new Vector2(Data.SymbolWidth, Data.SymbolHeight), Data.SymbolsCount, margin: Data.SymbolMargin);
        }

        public static void Shutdown()
        {
            textTexture.Dispose();
            textTexture = null;
        }

        public static void DrawText(Matrix4x4 transform, string text, Vector4 color, int entID = 0)
        {
            Debug.Assert(Mode == DrawMode.YUp || Mode == DrawMode.YDown);
            
            if (text == null)
                return;

            int line = 0;
            // i hate matrices
            float scaleY = Data.SymbolHeight, offsetY = Data.SymbolHeight;
            switch (Mode)
            {
                case DrawMode.YUp:
                    scaleY = Data.SymbolHeight;
                    offsetY = Data.SymbolHeight;
                    break;
                case DrawMode.YDown:
                    scaleY = -Data.SymbolHeight;
                    offsetY = -Data.SymbolHeight;
                    break;
            };

            transform = Matrix4x4.CreateScale(new Vector3(Data.SymbolWidth, scaleY, 1)) * Matrix4x4.CreateTranslation(new Vector3(Data.SymbolWidth / 2, -offsetY / 2, 0)) * transform;
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
                if (chord < 0 || chord >= Data.SymbolsCount) chord = '?' - Data.SymbolStart;

                Renderer2D.DrawTexturedQuad(matrix,
                                            textTexture,
                                            color,
                                            textAtlas[chord],
                                            entID);
            }
        }
    }
}