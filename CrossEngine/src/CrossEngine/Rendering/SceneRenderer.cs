using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Scenes;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Rendering
{
    public static class SceneRenderer
    {
        public static void DrawScene(SceneRenderData renderData, RendererApi rapi, ICamera overrideEditorCamera = null)
        {
            Profiler.BeginScope();

            renderData.Output?.GetValue().Bind();
            if (renderData.ClearColor != null)
            {
                rapi.SetClearColor(renderData.ClearColor.Value);
                rapi.Clear();
            }

            for (int layerIndex = 0; layerIndex < renderData.Layers.Count; layerIndex++)
            {
                SceneLayerRenderData layerData = renderData.Layers[layerIndex];
                ICamera activeCamera = overrideEditorCamera ?? layerData.Camera;

                if (activeCamera == null)
                    continue;

                foreach ((IRenderable Renderable, IList Objects) item in layerData.Data)
                {
                    var rndbl = item.Renderable;
                    var objs = item.Objects;

                    Profiler.BeginScope(rndbl.GetType().Name);

                    rndbl.Begin(activeCamera);
                    for (int objectIndex = 0; objectIndex < objs.Count; objectIndex++)
                    {
                        rndbl.Submit((IObjectRenderData)objs[objectIndex]);
                    }
                    rndbl.End();

                    Profiler.EndScope();
                }
            }

            renderData.Output?.GetValue().Unbind();

            Profiler.EndScope();
        }
    }
}