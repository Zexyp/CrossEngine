using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace CrossEngine.Rendering
{
    public class SceneRenderData
    {
        public Vector4? ClearColor = new Vector4(0, 0, 0, 0);
        public WeakReference<Framebuffer>? Output;

        public List<SceneLayerRenderData> Layers = new List<SceneLayerRenderData>();

        public event Action<SceneRenderData, float, float> Resize;

        public void PerformResize(float width, float height)
        {
            Resize?.Invoke(this, width, height);
        }
    }

    public class SceneLayerRenderData
    {
        public ICamera Camera;

        public List<(IRenderable Renderable, IList Objects)> Data = new List<(IRenderable, IList)>();
    }
}
