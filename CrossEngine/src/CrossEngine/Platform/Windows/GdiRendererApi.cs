using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using static CrossEngine.Platform.Windows.GdiContext;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using CrossEngine.Utils;

namespace CrossEngine.Platform.Windows
{
    class GdiRendererApi : RendererApi
    {
        public override void Clear()
        {
            // What the fuck is this piece of shit?, it doesn't rly clear
            buffered.Clear(state.clearColor);
        }

        public override void DrawArray(WeakReference<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles)
        {
            for (uint i = 0; i < verticesCount;)
            {
                // todo: geometry emission
                var (posA, colA) = state.program.Run((GdiVertexArray)vertexArray.GetValue(), i);
                PointF NDCToScreen(PointF point)
                {
                    point.X *= state.viewport.Width / 2;
                    point.Y *= state.viewport.Height / 2;
                    point.X += state.viewport.Width / 2 + state.viewport.X;
                    point.Y += state.viewport.Height / 2 + state.viewport.Y;
                    return point;
                }

                using (var brush = new SolidBrush(colA))
                {
                    switch (mode)
                    {
                        case DrawMode.Traingles:
                            {
                                var (posB, colB) = state.program.Run((GdiVertexArray)vertexArray.GetValue(), i + 1);
                                var (posC, colC) = state.program.Run((GdiVertexArray)vertexArray.GetValue(), i + 2);
                                buffered.FillPolygon(brush, new[] { NDCToScreen(posA), NDCToScreen(posB), NDCToScreen(posC) });
                                i += 3;
                            }
                            break;
                        case DrawMode.Lines:
                            {
                                var (posB, colB) = state.program.Run((GdiVertexArray)vertexArray.GetValue(), i + 1);
                                using (var pen = new Pen(brush))
                                {
                                    buffered.DrawPolygon(pen, new[] { NDCToScreen(posA), NDCToScreen(posB) });
                                }
                                i += 2;
                            }
                            break;
                        case DrawMode.Points:
                            {
                                var p = NDCToScreen(posA);
                                buffered.FillRectangle(brush, p.X, p.Y, 1, 1);
                                i += 1;
                            }
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            }
        }

        public override void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0)
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            state.mode = PolygonMode.Fill;

            Log.Debug("gdi pipepline initialized");
        }

        public override void SetBlendFunc(BlendFunc func)
        {
            throw new NotImplementedException();
        }

        public override void SetClearColor(float r, float g, float b, float a)
        {
            state.clearColor = Color.FromArgb(
                (int)(Math.Clamp(a, 0, 1) * 255),
                (int)(Math.Clamp(r, 0, 1) * 255),
                (int)(Math.Clamp(g, 0, 1) * 255),
                (int)(Math.Clamp(b, 0, 1) * 255));
        }

        public override void SetCullFace(CullFace face)
        {
            throw new NotImplementedException();
        }

        public override void SetDepthFunc(DepthFunc func)
        {
            throw new NotImplementedException();
        }

        public override void SetLineWidth(float width)
        {
            throw new NotImplementedException();
        }

        public override void SetPolygonMode(PolygonMode mode)
        {
            state.mode = mode;
        }

        public override void SetViewport(uint x, uint y, uint width, uint height)
        {
            var rect = new Rectangle((int)x, (int)y, (int)width, (int)height);
            state.viewport = rect;
            ((GdiContext)GraphicsContext.Current).Viewport(rect);
        }
    }
}
