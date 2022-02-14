using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CrossEngine.Rendering
{
    public class SceneRenderData
    {
        public List<SceneLayerRenderData> Layers = new List<SceneLayerRenderData>();
    }

    public class SceneLayerRenderData
    {
        public Dictionary<Renderable, List<IObjectRenderData>> Objects = new Dictionary<Renderable, List<IObjectRenderData>>();
    }
}
