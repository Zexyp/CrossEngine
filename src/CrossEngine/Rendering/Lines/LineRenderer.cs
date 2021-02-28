using System;
using static OpenGL.GL;

using System.Numerics;
using System.Collections.Generic;

using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Rendering.Buffers;
using CrossEngine.MainLoop;
using CrossEngine.Utils;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering.Lines
{
    public class LineRenderer
    {
        //class Line
        //{
        //    public Vector3 from;
        //    public Vector3 to;
        //    public Vector4 color;
        //    public float lifetime;
        //
        //    public Line(Vector3 from, Vector3 to, Vector4 color, float lifetime)
        //    {
        //        this.from = from;
        //        this.to = to;
        //        this.color = color;
        //        this.lifetime = lifetime;
        //    }
        //
        //    public void DecreaseLifetime(float time)
        //    {
        //        lifetime -= time;
        //    }
        //}

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

        // constansts
        const uint maxLineCount = 1000;
        const uint maxVertexCount = maxLineCount * 2;

        //static List<Line> lastingLines = new List<Line> { };

        #region Renderer Data
        // buffers
        static VertexArray lineVA = null;
        static VertexBuffer lineVB = null;

        static uint vertexCount = 0;

        static LineVertex[] lineBuffer = null;
        static int workingPos = 0;

        // stats
        static RendererStats rendererStats; // debug thing
        #endregion

        static Shader lineShader;

        static public unsafe void Init()
        {
            //glLineWidth(10.0f);
            //glEnable(GL_LINE_SMOOTH);

            lineShader = AssetManager.Shaders.GetShader("shaders/line/line.shader");

            lineBuffer = new LineVertex[maxVertexCount];

            // gpu side stuff
            lineVA = new VertexArray();
            lineVA.Bind(); // needs to be bound!!!

            lineVB = new VertexBuffer((void*)null, (int)maxVertexCount * sizeof(LineVertex), true); // limited

            VertexBufferLayout layout = new VertexBufferLayout();
            layout.Add(typeof(LineVertex));
            lineVA.AddBuffer(lineVB, layout);
        }

        static public void Shutdown()
        {
            lineVA.Dispose();
            lineVB.Dispose();

            lineBuffer = null; // idk if it helps
        }

        static public unsafe void BeginBatch()
        {
            //for (int i = 0; i < lastingLines.Count; i++) // thought this is mess
            //{
            //    lastingLines[i].lifetime -= Time.DeltaTime;
            //    if (lastingLines[i].lifetime <= 0.0f)
            //    {
            //        lastingLines.RemoveAt(i);
            //        i--;
            //    }
            //}
            //
            //for (int run = 0; run < lastingLines.Count; run += (int)maxLineCount)
            //{
            //    for (int i = 0; i < maxLineCount; i += 2)
            //    {
            //        if (lastingLines.Count > i + run)
            //        {
            //            lineBuffer[workingPos].position = lastingLines[run + i / 2].from;
            //            lineBuffer[workingPos].color = lastingLines[run + i / 2].color;
            //            workingPos++;
            //
            //            lineBuffer[workingPos].position = lastingLines[run + i / 2].to;
            //            lineBuffer[workingPos].color = lastingLines[run + i / 2].color;
            //            workingPos++;
            //
            //            vertexCount += 2;
            //        }
            //        else break;
            //    }
            //
            //    EndBatch();
            //    Flush();
            //    workingPos = 0;
            //}

            workingPos = 0;
        }

        static public unsafe void EndBatch()
        {
            fixed (LineVertex* quadBufferp = &lineBuffer[0])
            {
                int size = workingPos * sizeof(LineVertex);

                lineVB.SetData(quadBufferp, size);
            }
        }

        static public unsafe void Flush()
        {
            if (workingPos > 0)
            {
                lineShader.Use();

                lineShader.SetMat4("projection", ActiveCamera.camera.ProjectionMatrix);
                lineShader.SetMat4("view", ActiveCamera.camera.ViewMatrix);
                lineShader.SetMat4("model", Matrix4x4.Identity);

                lineVA.Bind();

                glDrawArrays(GL_LINES, 0, (int)vertexCount);

                vertexCount = 0;

                //stats
                rendererStats.drawCount++;
            }
        }

        static public void DrawLine(Vector3 from, Vector3 to, Vector4 color/*, float lifetime = 0.0f*/)
        {
            if (vertexCount >= maxVertexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            //if (lifetime > 0.0f)
            //{
            //    Line line = new Line(from, to, color, lifetime);
            //    lastingLines.Add(line);
            //}

            lineBuffer[workingPos].position = from;
            lineBuffer[workingPos].color = color;
            workingPos++;

            lineBuffer[workingPos].position = to;
            lineBuffer[workingPos].color = color;
            workingPos++;

            vertexCount += 2;

            // statistics
            rendererStats.itemCount++;
        }

        static public void DrawSquare(Vector3 center, Vector2 size, Vector4 color, float rotation = 0.0f)
        {
            if (vertexCount >= maxVertexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            Vector3 min = -new Vector3(size, 0.0f) * 0.5f;
            Vector3 max = new Vector3(size, 0.0f) * 0.5f;

            Vector3[] vertices = {
                min,
                new Vector3(max.X, min.Y, center.Z),
                max,
                new Vector3(min.X, max.Y, center.Z)
            };

            if (rotation != 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = center + Vector3.Transform(vertices[i], Matrix4x4.CreateRotationZ(rotation));
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += center;
                }
            }

            DrawLine(vertices[0], vertices[1], color);
            DrawLine(vertices[1], vertices[2], color);
            DrawLine(vertices[2], vertices[3], color);
            DrawLine(vertices[3], vertices[0], color);
        }

        static public void DrawBox(Vector3 center, Vector3 size, Vector4 color, Quaternion? rotation = null)
        {
            if (vertexCount >= maxVertexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            Vector3 min = -size * 0.5f;
            Vector3 max = size * 0.5f;

            Vector3[] vertices = {
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                max,
                new Vector3(min.X, max.Y, max.Z),

                min,
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(max.X, max.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z),
            };

            if (rotation != null)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = center + Vector3.Transform(vertices[i], (Quaternion)rotation);
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += center;
                }
            }

            DrawLine(vertices[0], vertices[1], color);
            DrawLine(vertices[1], vertices[2], color);
            DrawLine(vertices[2], vertices[3], color);
            DrawLine(vertices[3], vertices[0], color);

            DrawLine(vertices[4], vertices[5], color);
            DrawLine(vertices[5], vertices[6], color);
            DrawLine(vertices[6], vertices[7], color);
            DrawLine(vertices[7], vertices[4], color);

            DrawLine(vertices[0], vertices[4], color);
            DrawLine(vertices[1], vertices[5], color);
            DrawLine(vertices[2], vertices[6], color);
            DrawLine(vertices[3], vertices[7], color);
        }

        static public void DrawCircle(Vector3 center, float radius, Vector4 color, int segments = 16)
        {
            if (vertexCount >= maxVertexCount)
            {
                EndBatch();
                Flush();
                BeginBatch();
            }

            Vector3[] points = new Vector3[segments];
            float increment = MathF.PI * 2 / points.Length;
            float currentAngle = 0;

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = center + Vector3.Transform(new Vector3(0, radius, 0), Matrix4x4.CreateRotationZ(currentAngle));
                if (i > 0)
                {
                    DrawLine(points[i - 1], points[i], color);
                }

                currentAngle += increment;
            }
            DrawLine(points[points.Length - 1], points[0], color);
        }

        public static RendererStats GetStats()
        {
            return rendererStats;
        }

        //public static int GetLastingCount()
        //{
        //    return lastingLines.Count;
        //}

        public static void ResetStats()
        {
            RendererStats cleanStats;
            cleanStats.itemCount = 0;
            cleanStats.drawCount = 0;
            rendererStats = cleanStats;
        }
    }
}
