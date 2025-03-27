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
using static CrossEngine.Display.WindowService;

namespace CrossEngine.Rendering.Renderables
{
    interface IMeshRenderData : IObjectRenderData
    {
        IMesh Mesh { get; }
        Material Material { get; }
    }

    class MeshRenderable : Renderable<IMeshRenderData>
    {
        private const string DefaultShaderSource =
@"#type vertex
#version 330 core
layout(location = 0) in vec3 aPosition;
uniform mat4 uViewProjection = mat4(1);
uniform mat4 uModel = mat4(1);
void main() {
    gl_Position = uViewProjection * uModel * vec4(aPosition, 1.0);
}

#type fragment
#version 330 core
layout(location = 0) out vec4 oColor;
layout(location = 1) out int oEntityIDColor;
uniform int uEntityID;
void main() {
    oColor = vec4(1, 0, 1, 1);
    oEntityIDColor = uEntityID;
}";

        ICamera _camera;

        public override void Begin(ICamera camera)
        {
            _camera = camera;
        }

        public override void Submit(IMeshRenderData data)
        {
            if (data.Mesh == null)
                return;

            ShaderProgram shader = (data.Material?.Shader ?? CrossEngine.Rendering.Shaders.ShaderPreprocessor.DefaultShaderProgram).GetValue();
            shader.Use();
            shader.SetParameterMat4("uViewProjection", _camera.GetViewProjectionMatrix());
            shader.SetParameterMat4("uModel", data.Transform);

            if (data.Mesh.Indexed)
                GraphicsContext.Current.Api.DrawIndexed(data.Mesh.VA);
            else
                GraphicsContext.Current.Api.DrawArray(data.Mesh.VA, (uint)data.Mesh.Vertices.Count);
        }
    }
}
