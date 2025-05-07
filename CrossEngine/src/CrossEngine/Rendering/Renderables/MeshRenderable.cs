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
using CrossEngine.Geometry;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.Rendering;
using static CrossEngine.Display.WindowService;

namespace CrossEngine.Rendering.Renderables
{
    interface IMeshRenderData : IObjectRenderData
    {
        MeshRenderer Renderer { get; internal set; }
        IMaterial Material { get; }
    }
    
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
            var volume = data.GetVolume();
            CullChecker.Append(volume);
            
            if (data.Renderer == null)
                return;

            IMaterial mater = (data.Material ?? defaultMaterial);
            ShaderProgram shader = mater.Shader?.GetValue() ?? defaultMaterial.Shader.GetValue();
            shader.Use();
            mater.Update(shader);
            shader.SetParameterMat4("uViewProjection", _camera.GetViewProjectionMatrix());
            shader.SetParameterMat4("uModel", data.Transform);
            shader.SetParameterInt("uEntityID", data.Id);

            data.Renderer.Draw(GraphicsContext.Current.Api);
        }
    }
}
