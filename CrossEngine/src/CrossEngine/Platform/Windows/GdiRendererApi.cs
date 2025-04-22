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
using System.Reflection;
using static CrossEngine.Display.WindowService;
using CrossEngine.Platform.OpenGL;
using System.Runtime.CompilerServices;

// todo: geometry emission
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
            if (state.viewport.Width == 0 || state.viewport.Height == 0)
                return;

            var va = (GdiVertexArray)vertexArray.GetValue();
            try
            {
                for (uint i = 0; i < verticesCount;)
                {
                    DrawPrimitive(va, mode, () => i++);
                }
            }
            catch (Exception e)
            {
                Log.Error($"draw array failed: {e.GetType().Name}: {e.Message}");
            }
        }

        public override unsafe void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0)
        {
            if (state.viewport.Width == 0 || state.viewport.Height == 0)
                return;

            var va = (GdiVertexArray)vertexArray.GetValue();
            var ib = (GdiIndexBuffer)va.GetIndexBuffer().GetValue();
            var ibStart = GdiHelper.StreamStart(ib.stream);
            uint count = (indexCount != 0) ? indexCount : ib.Count;

            try
            {
                for (uint i = 0; i < count;)
                {
                    uint Dequeue()
                    {
                        // fixme: no feedback whe data type invalid
                        uint index = ib.DataType switch
                        {
                            IndexDataType.UInt => *(uint*)(ibStart + i * sizeof(uint)),
                            IndexDataType.UShort => *(ushort*)(ibStart + i * sizeof(ushort)),
                            IndexDataType.UByte => *(ibStart + i * sizeof(byte)),
                            _ => 0
                        };

                        i++;

                        return index;
                    }

                    DrawPrimitive(va, DrawMode.Traingles, Dequeue);
                }
            }
            catch (Exception e)
            {
                Log.Error($"draw indexed failed: {e.GetType().Name}: {e.Message}");
            }
        }

        public override void Init()
        {
            state.polygonMode = PolygonMode.Fill;
            state.samplers = new();
            state.lineWidth = 1;

            Log.Debug("gdi pipepline initialized");
        }

        public override void SetBlendFunc(BlendFunc func)
        {
            switch (func)
            {
                case BlendFunc.OneMinusSrcAlpha:
                    buffered.CompositingMode = CompositingMode.SourceOver;
                    break;
                case BlendFunc.One:
                    buffered.CompositingMode = CompositingMode.SourceCopy;
                    break;
                default:
                    break;
            }
        }

        public override void SetClearColor(float r, float g, float b, float a)
        {
            state.clearColor = ConvertCol(r, g, b, a);
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
            state.lineWidth = width;
        }

        public override void SetPolygonMode(PolygonMode mode)
        {
            state.polygonMode = mode;
        }

        public override void SetViewport(uint x, uint y, uint width, uint height)
        {
            var rect = new Rectangle((int)x, (int)y, (int)width, (int)height);
            state.viewport = rect;
            ((GdiContext)GraphicsContext.Current).Viewport(rect);
        }

        private void DrawPrimitive(GdiVertexArray va, DrawMode mode, Func<uint> dequeue)
        {
            var (points, brush) = PrimitveAssembly(va, mode, dequeue);
            switch (mode)
            {
                case DrawMode.Traingles:
                    switch (state.polygonMode)
                    {
                        case PolygonMode.Fill:
                            buffered.FillPolygon(brush, points);
                            break;
                        case PolygonMode.Line:
                            using (var pen = new Pen(brush, state.lineWidth))
                                buffered.DrawPolygon(pen, points);
                            break;
                        case PolygonMode.Point:
                            buffered.FillRectangle(brush, points[0].X, points[0].Y, 1, 1);
                            buffered.FillRectangle(brush, points[1].X, points[1].Y, 1, 1);
                            buffered.FillRectangle(brush, points[2].X, points[2].Y, 1, 1);
                            break;
                    }
                    break;
                case DrawMode.Lines:
                    using (var pen = new Pen(brush, state.lineWidth))
                        buffered.DrawLine(pen, points[0], points[1]);
                    break;
                case DrawMode.Points:
                    buffered.FillRectangle(brush, points[0].X, points[0].Y, 1, 1);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            brush.Dispose();
        }

        private static (PointF[], Brush) PrimitveAssembly(GdiVertexArray va, DrawMode mode, Func<uint> dequeue)
        {
            PointF[] points = null;
            Brush brush = null;

            switch (mode)
            {
                case DrawMode.Traingles:
                    {
                        var (posA, colA) = state.program.Run(va, dequeue.Invoke());
                        var (posB, colB) = state.program.Run(va, dequeue.Invoke());
                        var (posC, colC) = state.program.Run(va, dequeue.Invoke());
                        points = new[] { NDCToScreen(posA), NDCToScreen(posB), NDCToScreen(posC) };

                        static Color CenterCol(Color c1, Color c2, Color c3)
                        {
                            int r = (c1.R + c2.R + c3.R) / 3;
                            int g = (c1.G + c2.G + c3.G) / 3;
                            int b = (c1.B + c2.B + c3.B) / 3;
                            int a = (c1.A + c2.A + c3.A) / 3;

                            return Color.FromArgb(a, r, g, b);
                        }

                        brush = new PathGradientBrush(points) { CenterColor = CenterCol(colA, colB, colC), SurroundColors = new[] { colA, colB, colC } };
                    }
                    break;
                case DrawMode.Lines:
                    {
                        var (posA, colA) = state.program.Run(va, dequeue.Invoke());
                        var (posB, colB) = state.program.Run(va, dequeue.Invoke());
                        points = new[] { NDCToScreen(posA), NDCToScreen(posB) };

                        brush = new LinearGradientBrush(points[0], points[1], colA, colB);
                    }
                    break;
                case DrawMode.Points:
                    {
                        var (posA, colA) = state.program.Run(va, dequeue.Invoke());
                        points = new[] { NDCToScreen(posA) };

                        brush = new SolidBrush(colA);
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            return (points, brush);
        }

        private static PointF NDCToScreen(PointF point)
        {
            point.X *= state.viewport.Width / 2;
            point.Y *= -state.viewport.Height / 2; // lil funny minus sign
            point.X += state.viewport.Width / 2 + state.viewport.X;
            point.Y += state.viewport.Height / 2 + state.viewport.Y;
            return point;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color ConvertCol(Vector4 v) => ConvertCol(v.X, v.Y, v.Z, v.W);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color ConvertCol(float r, float g, float b, float a)
        {
            return Color.FromArgb(
                (int)(Math.Clamp(a, 0, 1) * 255),
                (int)(Math.Clamp(r, 0, 1) * 255),
                (int)(Math.Clamp(g, 0, 1) * 255),
                (int)(Math.Clamp(b, 0, 1) * 255));
        }
    }
}
