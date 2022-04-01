using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CrossEngine.Rendering;
using CrossEngine.Scenes;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;

namespace CrossEngine.Layers
{
    public class SceneLayer : Layer
    {
        private class SceneData
        {
            public bool updateEnabled = true;
            public bool renderEnabled = true;
        }

        Dictionary<Scene, SceneData> _scenes = new Dictionary<Scene, SceneData>();

        EventWaitHandle _waitHandle;

        public SceneLayer()
        {
            _waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
        }

        public override void OnRender()
        {
            _waitHandle.WaitOne();
            _waitHandle.Reset();
            Profiler.BeginScope();

            // draw
            foreach (var scenePair in _scenes)
            {
                Scene scene = scenePair.Key;
                SceneData sceneData = scenePair.Value;
                if (!sceneData.renderEnabled) continue;
                var scenRendereData = scene.GetRenderData();
                if (scenRendereData == null) continue;

                ((Framebuffer?)scenRendereData.Output)?.Bind();
                if (scenRendereData.ClearColor != null)
                {
                    Application.Instance.RendererAPI.SetClearColor(scenRendereData.ClearColor.Value);
                    Application.Instance.RendererAPI.Clear();
                }

                for (int layerIndex = 0; layerIndex < scenRendereData.Layers.Count; layerIndex++)
                {
                    SceneLayerRenderData layerData = scenRendereData.Layers[layerIndex];
                    foreach ((IRenderable Renderable, IList Objects) item in layerData.Data)
                    {
                        var rndbl = item.Renderable;
                        var objs = item.Objects;
                        rndbl.Begin(layerData.Camera.ViewProjectionMatrix);
                        for (int objectIndex = 0; objectIndex < objs.Count; objectIndex++)
                        {
                            rndbl.Submit((IObjectRenderData)objs[objectIndex]);
                        }
                        rndbl.End();
                    }
                }

                ((Framebuffer?)scenRendereData.Output)?.Unbind();
            }

            Profiler.EndScope();
            _waitHandle.Set();
        }

        public override void OnUpdate()
        {
            foreach (var scenePair in _scenes)
            {
                if (scenePair.Value.updateEnabled)
                    scenePair.Key.Update();
            }
        }

        public void AddScene(Scene scene)
        {
            Profiler.BeginScope();

            _waitHandle.WaitOne();
            _waitHandle.Reset();

            _scenes.Add(scene, new SceneData());

            _waitHandle.Set();

            Profiler.EndScope();
        }

        public void RemoveScene(Scene scene)
        {
            Profiler.BeginScope();

            _waitHandle.WaitOne();
            _waitHandle.Reset();

            _scenes.Remove(scene);

            _waitHandle.Set();

            Profiler.EndScope();
        }
    }
}
