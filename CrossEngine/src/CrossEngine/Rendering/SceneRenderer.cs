using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Scenes;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Buffers;

namespace CrossEngine.Rendering
{
    public static class SceneRenderer
    {
        public static void DrawScene(Scene scene, ICamera overrideEditorCamera = null)
        {
            Profiler.BeginScope();

            var scenRendereData = scene.UpdateRenderData();
            if (scenRendereData == null) return;

            scenRendereData.Output?.Value.Bind();
            if (scenRendereData.ClearColor != null)
            {
                Application.Instance.RendererAPI.SetClearColor(scenRendereData.ClearColor.Value);
                Application.Instance.RendererAPI.Clear();
            }

            for (int layerIndex = 0; layerIndex < scenRendereData.Layers.Count; layerIndex++)
            {
                SceneLayerRenderData layerData = scenRendereData.Layers[layerIndex];
                ICamera currentCamera = overrideEditorCamera ?? layerData.Camera;

                if (currentCamera == null)
                {
                    Application.Log.Warn("skipping render layer: no camere to render with");
                    continue;
                }
                currentCamera.Frustum.Prepare(currentCamera.ProjectionMatrix, currentCamera.ViewMatrix);

                Renderer2D.BeginScene(currentCamera.ViewProjectionMatrix);
                LineRenderer.BeginScene(currentCamera.ViewProjectionMatrix);

                foreach ((IRenderable Renderable, IList Objects) item in layerData.Data)
                {
                    var rndbl = item.Renderable;
                    var objs = item.Objects;

                    Profiler.BeginScope(rndbl.GetType().Name);

                    rndbl.Begin(currentCamera);
                    for (int objectIndex = 0; objectIndex < objs.Count; objectIndex++)
                    {
                        Debug.Assert(objs[objectIndex] != null);
                        rndbl.Submit(objs[objectIndex]);
                    }
                    rndbl.End();

                    Renderer2D.Flush();
                    LineRenderer.Flush();

                    Profiler.EndScope();
                }

                Renderer2D.EndScene();
                LineRenderer.EndScene();
            }

            scenRendereData.Output?.Value.Unbind();

            Profiler.EndScope();
        }
    }
}
