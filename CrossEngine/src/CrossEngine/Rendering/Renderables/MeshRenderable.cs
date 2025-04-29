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
    class MeshRenderable : Renderable<IMeshRenderData>
    {
        ICamera _camera;
        private IMaterial defaultMaterial;

        public override void Init()
        {
            defaultMaterial = new DefaultMaterial();
        }

        public override void Begin(ICamera camera)
        {
            _camera = camera;
        }

        public override void Submit(IMeshRenderData data)
        {
            CullChecker.Append(data.GetVolume());
            
            if (data.Mesh == null)
                return;

            IMaterial mater = (data.Material ?? defaultMaterial);
            ShaderProgram shader = mater.Shader.GetValue();
            shader.Use();
            mater.Update(shader);
            shader.SetParameterMat4("uViewProjection", _camera.GetViewProjectionMatrix());
            shader.SetParameterMat4("uModel", data.Transform);
            shader.SetParameterInt("uEntityID", data.Id);

            if (data.Mesh is IIndexedMesh)
                GraphicsContext.Current.Api.DrawIndexed(data.Mesh.VA);
            else
                GraphicsContext.Current.Api.DrawArray(data.Mesh.VA, (uint)data.Mesh.Vertices.Length);
        }
    }
}
