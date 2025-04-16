using CrossEngine.Debugging;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    public class GdiShaderProgram : ShaderProgram
    {
        Dictionary<string, ValueType> uniforms = new();

        public class ShaderVariables
        {
            public Dictionary<int, ValueType> Attributes;
            public Dictionary<string, ValueType> Out;
            public Dictionary<string, ValueType> In;
            public Vector4 Position;
            public Vector4 Color;
            public Dictionary<string, ValueType> Uniforms;

            public ShaderVariables(Dictionary<string, ValueType> uniforms)
            {
                Uniforms = uniforms;
                Attributes = new();
                Out = In = new();
            }
        }

        Script vertex, fragment;

        internal GdiShaderProgram(GdiShader vertex, GdiShader fragment)
        {
            this.vertex = vertex.script;
            this.fragment = fragment.script;
        }

        internal unsafe (PointF, Color) Run(GdiVertexArray va, uint rangeStart)
        {
            var vars = new ShaderVariables(uniforms);
            var vas = va.GetVertexBuffers();

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

            Debug.Assert(vas.Length == 1);
            var vb = (GdiVertexBuffer)vas[0].GetValue();
            var layout = vb.GetLayout();
            vb.stream.Position = rangeStart * layout.Stride;
            for (int ei = 0; ei < layout.Elements.Count; ei++)
            {
                switch (layout.Elements[ei].Type)
                {
                    case ShaderDataType.Float:
                        vars.Attributes[ei] = *(float*)(vb.stream.PositionPointer + layout.Elements[ei].Offset);
                        break;
                    case ShaderDataType.Float2:
                        vars.Attributes[ei] = *(Vector2*)(vb.stream.PositionPointer + layout.Elements[ei].Offset);
                        break;
                    case ShaderDataType.Float3:
                        vars.Attributes[ei] = *(Vector3*)(vb.stream.PositionPointer + layout.Elements[ei].Offset);
                        break;
                    case ShaderDataType.Float4:
                        vars.Attributes[ei] = *(Vector4*)(vb.stream.PositionPointer + layout.Elements[ei].Offset);
                        break;
                    //case ShaderDataType.Mat3:
                    //    break;
                    //case ShaderDataType.Mat4:
                    //    break;
                    //case ShaderDataType.Int:
                    //    break;
                    //case ShaderDataType.Int2:
                    //    break;
                    //case ShaderDataType.Int3:
                    //    break;
                    //case ShaderDataType.Int4:
                    //    break;
                    //case ShaderDataType.Bool:
                    //    break;
                    default: Debug.Assert(false);
                        break;
                }
            }

            try
            {
                vertex.RunAsync(vars).Wait();
                fragment.RunAsync(vars).Wait();
            }
            catch (Exception e)
            {
                GdiRendererApi.Log.Error($"shader error:\n{e}");
            }

            var color = Color.FromArgb(
                (int)(Math.Clamp(vars.Color.W, 0, 1) * 255),
                (int)(Math.Clamp(vars.Color.X, 0, 1) * 255),
                (int)(Math.Clamp(vars.Color.Y, 0, 1) * 255),
                (int)(Math.Clamp(vars.Color.Z, 0, 1) * 255));
            var point = TransformVector4ToPointF(vars.Position);
            return (point, color);
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
            throw new NotImplementedException();
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
