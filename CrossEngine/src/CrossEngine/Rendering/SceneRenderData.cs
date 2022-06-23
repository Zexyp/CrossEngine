using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering
{
    public class SceneRenderData
    {
        public Vector4? ClearColor = new Vector4(0, 0, 0, 0);
        public Ref<Framebuffer> Output;

        public List<SceneLayerRenderData> Layers = new List<SceneLayerRenderData>();
    }

    public class SceneLayerRenderData
    {
        //public Matrix4x4 ProjectionViewMatrix = Matrix4x4.Identity;
        public Camera Camera;

        public List<(IRenderable Renderable, IList Objects)> Data = new List<(IRenderable, IList)>();
    }
}
