using CrossEngine.Rendering;
using CrossEngine.Utils.Rendering;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Debugging
{
    public class MetricsOverlay : HudOverlay
    {
        private double[] _deltas = new double[256];
        private int _deltasIndex = 0;
        private RenderService _rs;

        public MetricsOverlay(RenderService rs)
        {
            _rs = rs;
        }

        public override void Draw()
        {
            Renderer2D.SetBlending(BlendMode.Blend);
            GraphicsContext.Current.Api.SetDepthFunc(DepthFunc.None);
            base.Draw();
        }

        protected override void Content()
        {
            var frameDuration = _rs.GetLastFrameDuration();

            // text
            var t = $"{1d / frameDuration:000.00} fps\n{frameDuration * 1000:00.000} ms\n[{(Debugger.IsAttached ? "debugger" : "standalone")}]";
            var offset = new Vector3(0, Size.Y - TextRendererUtil.TextRendererUtilData.SymbolHeight * 3, 0);
            TextRendererUtil.DrawText(Matrix4x4.CreateTranslation(offset), t, ColorHelper.U32ToVec4(0x7f1fb311));

            // graph
            var graphY = Size.Y - TextRendererUtil.TextRendererUtilData.SymbolHeight * 3;
            var graphX = 0;
            _deltas[_deltasIndex] = frameDuration;

            for (int ioff = 0; ioff < _deltas.Length; ioff++)
            {
                var i = (_deltasIndex + ioff) % _deltas.Length;
                var barHeight = (float)(_deltas[i] * 1000);
                Renderer2D.DrawQuad(Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) *
                                    Matrix4x4.CreateScale(new Vector3(1, barHeight, 1)) *
                                    Matrix4x4.CreateTranslation(new Vector3(ioff + graphX, -barHeight + graphY, 0)),
                    new Vector4(0, .5f, .5f, .75f));
            }

            _deltasIndex++;
            _deltasIndex %= _deltas.Length;
        }
    }
}
