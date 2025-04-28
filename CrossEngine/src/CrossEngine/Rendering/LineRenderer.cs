using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using CrossEngine.Logging;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering
{
    public class LineRenderer
    {
        #region Shader Sources
#if OPENGL
        const string VertexShaderSource =
#if !OPENGL_ES
"#version 330 core" +
#else
@"#version 300 es
precision highp float;" +
#endif
@"

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;

uniform mat4 uViewProjection;

out vec4 vColor;

void main()
{
   gl_Position = uViewProjection * vec4(aPosition, 1.0);
   vColor = aColor;
}";

        const string GeometryShaderSource =
@"you got jebaited";

        const string FragmentShaderSource =
#if !OPENGL_ES
"#version 330 core" +
#else
@"#version 300 es
precision highp float;" +
#endif
@"

layout(location = 0) out vec4 oColor;

in vec4 vColor;

void main()
{
   oColor = vColor;
}";
#elif GDI
        const string VertexShaderSource =
@"
Matrix4x4 uViewProjection = (Matrix4x4)Uniforms[""uViewProjection""];
Vector3 aPosition = (Vector3)AttributesIn[0];
Vector4 aColor = (Vector4)AttributesIn[1];

Out[""vColor""] = aColor;

gdi_Position = Vector4.Transform(new Vector4(aPosition, 1), uViewProjection);
";

        const string FragmentShaderSource =
@"
gdi_Color = (Vector4)In[""vColor""];
";
#endif
        #endregion

        struct LineVertex
        {
            public Vector3 position;
            public Vector4 color;

            public LineVertex(Vector3 position, Vector4 color)
            {
                this.position = position;
                this.color = color;
            }
        }

        struct LineRendererData
        {
            public const uint MaxLines = 5000;
            public const uint MaxVertices = MaxLines * 2;

            public WeakReference<VertexArray> lineVertexArray;
            public WeakReference<VertexBuffer> lineVertexBuffer;
            public WeakReference<ShaderProgram> lineShader;

            public uint lineCount;
            public LineVertex[] lineVertexBufferBase;
            public unsafe LineVertex* lineVertexBufferPtr;

            public RendererStats stats;
        }

        static LineRendererData data;
        static RendererApi _rapi;

        public static unsafe void Init(RendererApi rapi)
        {
            Log.Default.Debug($"initializing {nameof(LineRenderer)}");

            _rapi = rapi;

            data.lineVertexArray = VertexArray.Create();

            data.lineVertexBuffer = VertexBuffer.Create(null, (uint)(LineRendererData.MaxVertices * sizeof(LineVertex)), BufferUsageHint.DynamicDraw);
            data.lineVertexBuffer.GetValue().SetLayout(new BufferLayout(
                new BufferElement(ShaderDataType.Float3, "aPosition"),
                new BufferElement(ShaderDataType.Float4, "aColor")
            ));

            data.lineVertexArray.GetValue().AddVertexBuffer(data.lineVertexBuffer);

            data.lineVertexBufferBase = new LineVertex[LineRendererData.MaxVertices];

            var vertex = Shader.Create(VertexShaderSource, ShaderType.Vertex).GetValue();
            var fragment = Shader.Create(FragmentShaderSource, ShaderType.Fragment).GetValue();
            data.lineShader = ShaderProgram.Create(vertex, fragment);
            vertex.Dispose();
            fragment.Dispose();
        }

        public static void Shutdown()
        {
            Log.Default.Debug($"shutting down {nameof(LineRenderer)}");

            data.lineShader.Dispose();
            data.lineVertexArray.Dispose();
            data.lineVertexBuffer.Dispose();
            data.lineVertexBufferBase = null;
        }

        public static void SetLineWidth(float width) => _rapi.SetLineWidth(width);

        public static unsafe void BeginScene(Matrix4x4 viewProjectionMatrix)
        {
            var shader = data.lineShader.GetValue();
            shader.Use();
            shader.SetParameterMat4("uViewProjection", viewProjectionMatrix);

            StartBatch();
        }

        public static void EndScene()
        {
            Flush();
        }

        static unsafe void StartBatch()
        {
            data.lineCount = 0;
            fixed (LineVertex* p = &data.lineVertexBufferBase[0])
                data.lineVertexBufferPtr = p;
        }

        public static unsafe void Flush()
        {
            if (data.lineCount == 0)
                return;

            uint dataSize;
            fixed (LineVertex* p = &data.lineVertexBufferBase[0])
            {
                dataSize = (uint)((byte*)data.lineVertexBufferPtr - (byte*)p);
                data.lineVertexBuffer.GetValue().SetData(p, dataSize);
            }

            data.lineShader.GetValue().Use();
            _rapi.DrawArray(data.lineVertexArray, data.lineCount * 2, DrawMode.Lines);

            data.stats.DrawCalls++;
        }

        static void NextBatch()
        {
            Flush();
            StartBatch();
        }

        #region Draw Methods
        static public unsafe void DrawLine(Vector3 from, Vector3 to, Vector4 color) //, float lifetime = 0.0f
        {
            if (data.lineCount >= LineRendererData.MaxLines)
                NextBatch();

            data.lineVertexBufferPtr->position = from;
            data.lineVertexBufferPtr->color = color;
            data.lineVertexBufferPtr++;
            data.lineVertexBufferPtr->position = to;
            data.lineVertexBufferPtr->color = color;
            data.lineVertexBufferPtr++;

            data.lineCount++;

            data.stats.ItemCount++;
        }

        static readonly Vector3[] squareVertices = new Vector3[4]
        {
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f, -0.5f, 0),
        };

        static readonly Vector3[] boxVertices = new Vector3[8]
        {
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),

            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
        };

        /// <summary>
        /// Draws a 1×1 square.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="color"></param>
        static public void DrawSquare(Matrix4x4 matrix, Vector4 color)
        {
            Vector3[] points = new Vector3[squareVertices.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Vector3.Transform(squareVertices[i], matrix);
            }
            DrawLine(points[0], points[1], color);
            DrawLine(points[1], points[2], color);
            DrawLine(points[2], points[3], color);
            DrawLine(points[3], points[0], color);
        }

        /// <summary>
        /// Draws a 1×1×1 cube.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="color"></param>
        public static void DrawBox(Matrix4x4 matrix, Vector4 color)
        {
            Vector3[] points = new Vector3[boxVertices.Length];

            for (int i = 0; i < points.Length; i++)
            {
                // upsík dupsík, this was broken too much, now it should work
                var p = Vector4.Transform(new Vector4(boxVertices[i], 1), matrix);
                points[i] = new Vector3(p.X, p.Y, p.Z) / p.W;
            }

            DrawLine(points[0], points[1], color);
            DrawLine(points[1], points[2], color);
            DrawLine(points[2], points[3], color);
            DrawLine(points[3], points[0], color);

            DrawLine(points[4], points[5], color);
            DrawLine(points[5], points[6], color);
            DrawLine(points[6], points[7], color);
            DrawLine(points[7], points[4], color);

            DrawLine(points[0], points[4], color);
            DrawLine(points[1], points[5], color);
            DrawLine(points[2], points[6], color);
            DrawLine(points[3], points[7], color);
        }

        public static void DrawBox(Vector3 min, Vector3 max, Vector4 color)
        {
            DrawLine(min, new Vector3(max.X, min.Y, min.Z), color);
            DrawLine(min, new Vector3(min.X, max.Y, min.Z), color);
            DrawLine(min, new Vector3(min.X, min.Y, max.Z), color);

            DrawLine(max, new Vector3(min.X, max.Y, max.Z), color);
            DrawLine(max, new Vector3(max.X, min.Y, max.Z), color);
            DrawLine(max, new Vector3(max.X, max.Y, min.Z), color);

            DrawLine(new Vector3(max.X, min.Y, min.Z), new Vector3(max.X, max.Y, min.Z), color);
            DrawLine(new Vector3(min.X, max.Y, min.Z), new Vector3(min.X, max.Y, max.Z), color);
            DrawLine(new Vector3(min.X, min.Y, max.Z), new Vector3(max.X, min.Y, max.Z), color);
            
            DrawLine(new Vector3(min.X, max.Y, max.Z), new Vector3(min.X, min.Y, max.Z), color);
            DrawLine(new Vector3(max.X, min.Y, max.Z), new Vector3(max.X, min.Y, min.Z), color);
            DrawLine(new Vector3(max.X, max.Y, min.Z), new Vector3(min.X, max.Y, min.Z), color);
        }

        static public void DrawCircle(Matrix4x4 matrix, Vector4 color, float radius = 1.0f, int segments = 16)
        {
            Vector3[] points = new Vector3[segments];
            float increment = MathF.PI * 2 / points.Length;
            float currentAngle = 0;

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Vector3.Transform(new Vector3(0, radius, 0), Matrix4x4.CreateRotationZ(currentAngle) * matrix);

                if (i > 0)
                {
                    DrawLine(points[i - 1], points[i], color);
                }

                currentAngle += increment;
            }
            DrawLine(points[points.Length - 1], points[0], color);
        }

        /// <summary>
        /// Draws lines in X, Y, Z direction with matching colors.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="len"></param>
        public static void DrawAxies(Matrix4x4 matrix, float len = 1.0f)
        {
            DrawLine(Vector3.Transform(Vector3.Zero, matrix), Vector3.Transform(new Vector3(len, 0, 0), matrix), new Vector4(1, 0, 0, 1));
            DrawLine(Vector3.Transform(Vector3.Zero, matrix), Vector3.Transform(new Vector3(0, len, 0), matrix), new Vector4(0, 1, 0, 1));
            DrawLine(Vector3.Transform(Vector3.Zero, matrix), Vector3.Transform(new Vector3(0, 0, len), matrix), new Vector4(0, 0, 1, 1));
        }

        static public void DrawSphere(Matrix4x4 matrix, Vector4 color, float radius = 1.0f, int segments = 16)
        {
            DrawCircle(Matrix4x4.CreateRotationX(MathF.PI / 2) * matrix, color, radius, segments);
            DrawCircle(Matrix4x4.CreateRotationY(MathF.PI / 2) * matrix, color, radius, segments);
            DrawCircle(Matrix4x4.CreateRotationZ(MathF.PI / 2) * matrix, color, radius, segments);
        }
        #endregion
    }
}
