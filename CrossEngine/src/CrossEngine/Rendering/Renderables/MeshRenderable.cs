using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Rendering.Renderables
{
    interface IMeshRenderData : IObjectRenderData
    {
        IMesh Mesh { get; }
        Material Material { get; }
    }

    class MeshRenderable : Renderable<IMeshRenderData>
    {
        ICamera _camera;

        public override void Begin(ICamera camera)
        {
            _camera = camera;
        }

        public override void Submit(IMeshRenderData data)
        {
            ShaderProgram shader = (data.Material?.Shader ?? CrossEngine.Rendering.Shaders.ShaderPreprocessor.DefaultShaderProgram).GetValue();
            shader.Use();
            shader.SetParameterMat4("uViewProjection", _camera.GetViewProjectionMatrix());

            if (data.Mesh.Indexed)
                RApi.DrawIndexed(data.Mesh.VA);
            else
                RApi.DrawArray(data.Mesh.VA, (uint)data.Mesh.Vertices.Count);
        }
    }
}
