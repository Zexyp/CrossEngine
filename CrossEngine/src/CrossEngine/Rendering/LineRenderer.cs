using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering
{
    public class LineRenderer
    {
        #region Shader Sources
        const string VertexShaderSource =
@"#version 330 core

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
@"#version 330 core

layout(location = 0) out vec4 oColor;

in vec4 vColor;

void main()
{
   oColor = vColor;
}";
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

            public Ref<VertexArray> lineVertexArray;
            public Ref<VertexBuffer> lineVertexBuffer;
            public ShaderProgram lineShader;

            public uint lineCount;
            public LineVertex[] lineVertexBufferBase;
            public unsafe LineVertex* lineVertexBufferPtr;

            public RendererStats stats;
        }

        static LineRendererData data;

        public static unsafe void Init()
        {
            data.lineVertexArray = VertexArray.Create();

            data.lineVertexBuffer = VertexBuffer.Create(null, (uint)(LineRendererData.MaxVertices * sizeof(LineVertex)), BufferUsageHint.DynamicDraw);
            data.lineVertexBuffer.Value.SetLayout(new BufferLayout(
                new BufferElement(ShaderDataType.Float3, "aPosition"),
                new BufferElement(ShaderDataType.Float4, "aColor")
            ));

            data.lineVertexArray.Value.AddVertexBuffer(data.lineVertexBuffer);

            data.lineVertexBufferBase = new LineVertex[LineRendererData.MaxVertices];

            var vertex = new Shader(VertexShaderSource, ShaderType.Vertex);
            var fragment = new Shader(FragmentShaderSource, ShaderType.Fragment);
            data.lineShader = new ShaderProgram(vertex, fragment);
            vertex.Dispose();
            fragment.Dispose();
        }

        public static void Shutdown()
        {
            data.lineShader.Dispose();
            data.lineVertexArray.Value.Dispose();
            data.lineVertexBuffer.Value.Dispose();
            data.lineVertexBufferBase = null;
        }

        public static void SetLineWidth(float width) => Application.Instance.RendererAPI.SetLineWidth(width);

        public static unsafe void BeginScene(Matrix4x4 viewProjectionMatrix)
        {
            data.lineShader.Use();
            data.lineShader.SetMat4("uViewProjection", viewProjectionMatrix);

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
                data.lineVertexBuffer.Value.SetData(p, dataSize);
            }

            data.lineShader.Use();
            Application.Instance.RendererAPI.DrawArray(data.lineVertexArray, data.lineCount * 2, DrawMode.Lines);

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
                points[i] = Vector3.Transform(boxVertices[i], matrix);
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

        static public void DrawCircle(Matrix4x4 matrix, Vector4 color, int segments = 16, float radius = 1.0f)
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

        static public void DrawSphere(Matrix4x4 matrix, Vector4 color, int segments = 16, float radius = 1.0f)
        {
            DrawCircle(Matrix4x4.CreateRotationX(MathF.PI / 2) * matrix, color, segments, radius);
            DrawCircle(Matrix4x4.CreateRotationY(MathF.PI / 2) * matrix, color, segments, radius);
            DrawCircle(Matrix4x4.CreateRotationZ(MathF.PI / 2) * matrix, color, segments, radius);
        }
        #endregion
    }
}
