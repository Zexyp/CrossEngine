using CrossEngine.Debugging;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    [Obsolete("internal")]
    public class ShaderVariables
    {
        public Dictionary<int, ValueType> AttributesIn;
        public Dictionary<int, ValueType> AttributesOut;
        public Dictionary<string, ValueType> Out;
        public Dictionary<string, ValueType> In;
        public Dictionary<string, object> Uniforms;
        public Vector4 gdi_Position;
        public Vector4 gdi_Color { get => (Vector4)(AttributesOut.TryGetValue(0, out var value) ? value : new Vector4(0, 0, 0, 1)); set => AttributesOut[0] = value; }

        public ShaderVariables(Dictionary<string, object> uniforms)
        {
            Uniforms = uniforms;
            AttributesIn = new();
            AttributesOut = new();
            Out = In = new();
        }

        public Vector4 Sample(int sampler, Vector2 uv)
        {
            var bitmap = state.samplers[(uint)sampler].bitmap;

            int x = (int)(uv.X * (bitmap.Width - 1));
            int y = (int)(uv.Y * (bitmap.Height - 1));

            var color = bitmap.GetPixel(Math.Clamp(x, 0, bitmap.Width - 1), Math.Clamp(y, 0, bitmap.Height - 1));

            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }

    class GdiShaderProgram : ShaderProgram
    {
        Dictionary<string, object> uniforms = new();

        Script vertex, fragment;

        internal GdiShaderProgram(GdiShader vertex, GdiShader fragment)
        {
            GC.KeepAlive(this);
            GPUGC.Register(this);

            this.vertex = vertex.script;
            this.fragment = fragment.script;
        }

        internal (PointF, Color) Run(GdiVertexArray va, uint index)
        {
            // thanks chat
            static PointF TransformVector4ToPointF(Vector4 v)
            {
                // Perspective divide (if needed)
                float x = v.X;
                float y = v.Y;
                float w = v.W;

                if (w != 0f && w != 1f)
                {
                    x /= w;
                    y /= w;
                }

                return new PointF(x, y);
            }


            // load attribs
            var vas = va.GetVertexBuffers();
            var vars = LoadVariables(vas, index);

            try
            {
                vertex.RunAsync(vars).Wait();
            }
            catch (Exception e)
            {
                GdiRendererApi.Log.Error($"vertex shader error:\n{e}");
            }
            try
            {
                fragment.RunAsync(vars).Wait();
            }
            catch (Exception e)
            {
                GdiRendererApi.Log.Error($"fragment shader error:\n{e}");
            }

            var color = GdiRendererApi.ConvertCol(vars.gdi_Color);
            var point = TransformVector4ToPointF(vars.gdi_Position);
            return (point, color);
        }

        private unsafe ShaderVariables LoadVariables(WeakReference<VertexBuffer>[] vas, uint index)
        {
            var vars = new ShaderVariables(uniforms);

            Debug.Assert(vas.Length == 1);
            var vb = (GdiVertexBuffer)vas[0].GetValue();
            var layout = vb.GetLayout();
            byte* streamStart = GdiHelper.StreamStart(vb.stream);
            byte* indexedElement = streamStart + index * layout.Stride;
            for (int ei = 0; ei < layout.Elements.Count; ei++)
            {
                switch (layout.Elements[ei].Type)
                {
                    case ShaderDataType.Float:
                        vars.AttributesIn[ei] = *(float*)(indexedElement + layout.Elements[ei].Offset);
                        break;
                    case ShaderDataType.Float2:
                        vars.AttributesIn[ei] = *(Vector2*)(indexedElement + layout.Elements[ei].Offset);
                        break;
                    case ShaderDataType.Float3:
                        vars.AttributesIn[ei] = *(Vector3*)(indexedElement + layout.Elements[ei].Offset);
                        break;
                    case ShaderDataType.Float4:
                        vars.AttributesIn[ei] = *(Vector4*)(indexedElement + layout.Elements[ei].Offset);
                        break;
                    //case ShaderDataType.Mat3:
                    //    break;
                    //case ShaderDataType.Mat4:
                    //    break;
                    case ShaderDataType.Int:
                        vars.AttributesIn[ei] = *(int*)(indexedElement + layout.Elements[ei].Offset);
                        break;
                    //case ShaderDataType.Int2:
                    //    break;
                    //case ShaderDataType.Int3:
                    //    break;
                    //case ShaderDataType.Int4:
                    //    break;
                    //case ShaderDataType.Bool:
                    //    break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return vars;
        }

        public override void Use()
        {
            state.program = this;
        }

        public override void Disuse()
        {
            state.program = null;
        }

        public override void SetParameterFloat(string name, float value)
        {
            uniforms[name] = value;
        }

        public override void SetParameterInt(string name, int value)
        {
            uniforms[name] = value;
        }

        public override void SetParameterIntVec(string name, int[] intVec)
        {
            uniforms[name] = intVec;
        }

        public override void SetParameterMat4(string name, Matrix4x4 mat)
        {
            uniforms[name] = mat;
        }

        public override void SetParameterVec2(string name, float x, float y)
        {
            uniforms[name] = new Vector2(x, y);
        }

        public override void SetParameterVec3(string name, float x, float y, float z)
        {
            uniforms[name] = new Vector3(x, y, z);
        }

        public override void SetParameterVec4(string name, float x, float y, float z, float w)
        {
            uniforms[name] = new Vector4(x, y, z, w);
        }
    }
}
