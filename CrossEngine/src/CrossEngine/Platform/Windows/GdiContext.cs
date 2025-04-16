using CrossEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Platform.Windows
{
    class GdiContext : GraphicsContext
    {
        internal struct State
        {
            public GdiShaderProgram program;
            public PolygonMode mode;
            public Color clearColor;
            public Rectangle viewport;
            public GdiVertexArray va;
            public GdiVertexBuffer vb;
        }

        internal static Graphics graphics;
        internal static Graphics buffered;
        internal static State state;

        private Graphics _graphics;
        private Graphics _buffered;
        private BufferedGraphics _bufferedGraphics;
        private nint _windowHandle;

        public GdiContext(nint windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public override void Init()
        {
            _graphics = Graphics.FromHwnd(_windowHandle);
            
        }

        public override void MakeCurrent()
        {
            graphics = _graphics;
            buffered = _buffered;
        }

        public override void Shutdown()
        {
            _graphics.Dispose();
        }

        public override void SwapBuffers()
        {
            _graphics.Flush();
            _bufferedGraphics.Render(_graphics);
        }

        internal void Viewport(Rectangle rect)
        {
            _bufferedGraphics?.Dispose();
            _bufferedGraphics = BufferedGraphicsManager.Current.Allocate(graphics, rect);
            _buffered?.Dispose();
            _buffered = _bufferedGraphics.Graphics;
            
            if (GraphicsContext.Current == this)
                MakeCurrent();
        }
    }
}
