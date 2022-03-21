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

namespace CrossEngine.Layers
{
    public class SceneLayer : Layer
    {
        List<Scene> _scenes = new List<Scene>();

        EventWaitHandle _waitHandle;

        public SceneLayer()
        {
            _waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
        }

        public override void OnRender()
        {
            _waitHandle.WaitOne();
            _waitHandle.Reset();

            // draw
            for (int sceneIndex = 0; sceneIndex < _scenes.Count; sceneIndex++)
            {
                var sceneData = _scenes[sceneIndex].GetRenderData();
                
                for (int layerIndex = 0; layerIndex < sceneData.Layers.Count; layerIndex++)
                {
                    SceneLayerRenderData layerData = sceneData.Layers[layerIndex];
                    foreach ((IRenderable Renderable, IList Objects) item in layerData.Data)
                    {
                        var rndbl = item.Renderable;
                        var objs = item.Objects;
                        rndbl.Begin(layerData.ProjectionViewMatrix);
                        for (int objectIndex = 0; objectIndex < objs.Count; objectIndex++)
                        {
                            rndbl.Submit((IObjectRenderData)objs[objectIndex]);
                        }
                        rndbl.End();
                    }
                }
            }

            _waitHandle.Set();
        }

        public override void OnUpdate()
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                _scenes[i].Update();
            }
        }

        public void AddScene(Scene scene)
        {
            Profiler.BeginScope();

            _waitHandle.WaitOne();
            _waitHandle.Reset();

            _scenes.Add(scene);

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
