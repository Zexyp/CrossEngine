using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering
{
    public class SceneRenderData
    {
        public List<SceneLayerRenderData> Layers = new List<SceneLayerRenderData>();
    }

    public class SceneLayerRenderData
    {
        public Matrix4x4 ProjectionViewMatrix = Matrix4x4.Identity;
        public List<(IRenderable Renderable, IList Objects)> Data = new List<(IRenderable, IList)>();
    }
}
