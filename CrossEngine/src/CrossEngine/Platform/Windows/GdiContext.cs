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
            public PolygonMode polygonMode;
            public Color clearColor;
            public Rectangle viewport;
            public GdiVertexArray va;
            public GdiVertexBuffer vb;
            public GdiIndexBuffer ib;
            public Dictionary<uint, GdiTexture> samplers;
            public float lineWidth;
        }

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

            InitBuffered(new Rectangle(0, 0, 1, 1));
        }

        public override void MakeCurrent()
        {
            buffered = _buffered;
        }

        public override void Shutdown()
        {
            _graphics.Dispose();
            _buffered.Dispose();
            _bufferedGraphics.Dispose();

            _graphics = null;
            _buffered = null;
            _bufferedGraphics = null;
        }

        public override void SwapBuffers()
        {
            _graphics.Flush();
            _buffered.Flush();
            _bufferedGraphics.Render(_graphics);
        }

        internal void Viewport(Rectangle rect)
        {
            _bufferedGraphics?.Dispose();
            _buffered?.Dispose();

            //var lastCompositingMode = _buffered.CompositingMode;

            InitBuffered(rect);

            //_buffered.CompositingMode = lastCompositingMode;

            if (GraphicsContext.Current == this)
                MakeCurrent();
        }

        private void InitBuffered(in Rectangle rect)
        {
            _bufferedGraphics = BufferedGraphicsManager.Current.Allocate(_graphics, rect);
            _buffered = _bufferedGraphics.Graphics;
        }
    }
}
