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
            "#version 330 core\n" +
            "\n" +
            "layout(location = 0) in vec3 aPosition;\n" +
            "layout(location = 1) in vec4 aColor;\n" +
            "\n" +
            "uniform mat4 uViewProjection;\n" +
            "\n" +
            "out vec4 vColor;\n" +
            "\n" +
            "void main()\n" +
            "{\n" +
            "   vColor = aColor;\n" +
            "   gl_Position = uViewProjection * vec4(aPosition, 1.0);\n" +
            "}\n";
        const string FragmentShaderSource =
            "#version 330 core\n" +
            "\n" +
            "layout(location = 0) out vec4 oColor;\n" +
            "\n" +
            "in vec4 vColor;\n" +
            "\n" +
            "void main()\n" +
            "{\n" +
            "   oColor = vColor;\n" +
            "}\n";
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
            ((VertexBuffer?)data.lineVertexBuffer).SetLayout(new BufferLayout(
                new BufferElement(ShaderDataType.Float3, "aPosition"),
                new BufferElement(ShaderDataType.Float4, "aColor")
            ));

            ((VertexArray?)data.lineVertexArray).AddVertexBuffer(data.lineVertexBuffer);

            data.lineVertexBufferBase = new LineVertex[LineRendererData.MaxVertices];

            var vertex = new Shader(VertexShaderSource, ShaderType.Vertex);
            var fragment = new Shader(FragmentShaderSource, ShaderType.Fragment);
            data.lineShader = new ShaderProgram(vertex, fragment);
            vertex.Dispose();
            fragment.Dispose();
        }

        public static void Shutdown()
        {
            throw new NotImplementedException();
        }

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

        static unsafe void Flush()
        {
            if (data.lineCount == 0)
                return;

            uint dataSize;
            fixed (LineVertex* p = &data.lineVertexBufferBase[0])
            {
                dataSize = (uint)((byte*)data.lineVertexBufferPtr - (byte*)p);
                ((VertexBuffer?)data.lineVertexBuffer).SetData(p, dataSize);
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

        static public void DrawSquare(Vector3 center, Vector2 size, Vector4 color, Quaternion? rotation = null)
        {
            if (rotation != null)
                DrawSquare(Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateFromQuaternion((Quaternion)rotation) * Matrix4x4.CreateTranslation(center), color);
            else
                DrawSquare(Matrix4x4.CreateScale(new Vector3(size, 0.0f)) * Matrix4x4.CreateTranslation(center), color);
        }

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

        static public void DrawBox(Vector3 center, Vector3 size, Vector4 color, Quaternion? rotation = null)
        {
            if (rotation != null)
                DrawBox(Matrix4x4.CreateScale(size) * Matrix4x4.CreateFromQuaternion((Quaternion)rotation) * Matrix4x4.CreateTranslation(center), color);
            else
                DrawBox(Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(center), color);
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

        static public void DrawCircle(Vector3 center, float radius, Vector4 color, Quaternion? rotation = null, int segments = 16)
        {
            Vector3[] points = new Vector3[segments];
            float increment = MathF.PI * 2 / points.Length;
            float currentAngle = 0;

            for (int i = 0; i < points.Length; i++)
            {
                if (rotation != null)
                    points[i] = center + Vector3.Transform(new Vector3(0, radius, 0), Matrix4x4.CreateRotationZ(currentAngle) * Matrix4x4.CreateFromQuaternion((Quaternion)rotation));
                else
                    points[i] = center + Vector3.Transform(new Vector3(0, radius, 0), Matrix4x4.CreateRotationZ(currentAngle));

                if (i > 0)
                {
                    DrawLine(points[i - 1], points[i], color);
                }

                currentAngle += increment;
            }
            DrawLine(points[points.Length - 1], points[0], color);
        }

        public static void DrawAxes(Matrix4x4 matrix, float len = 1.0f)
        {
            DrawLine(Vector3.Transform(Vector3.Zero, matrix), Vector3.Transform(new Vector3(len, 0, 0), matrix), new Vector4(1, 0, 0, 1));
            DrawLine(Vector3.Transform(Vector3.Zero, matrix), Vector3.Transform(new Vector3(0, len, 0), matrix), new Vector4(0, 1, 0, 1));
            DrawLine(Vector3.Transform(Vector3.Zero, matrix), Vector3.Transform(new Vector3(0, 0, len), matrix), new Vector4(0, 0, 1, 1));
        }
        #endregion
    }
}
