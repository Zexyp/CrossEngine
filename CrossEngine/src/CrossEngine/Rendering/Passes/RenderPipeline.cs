using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Profiling;

namespace CrossEngine.Rendering.Passes
{
    public class RenderPipeline
    {
        public struct FramebufferStructureIndex
        {
            public int Color;
            public int ID;

            public static FramebufferStructureIndex Default
            {
                get
                {
                    return new FramebufferStructureIndex()
                    {
                        Color = 0,
                        ID = 1,
                    };
                }
            }
        }

        public FramebufferStructureIndex FBStructureIndex = FramebufferStructureIndex.Default;

        private List<RenderPass> _passes = new List<RenderPass>();

        public void RegisterPass(RenderPass pass)
        {
            _passes.Add(pass);
        }

        public void RemovePass(RenderPass pass)
        {
            _passes.Remove(pass);
        }

        public void RemovePass(int index)
        {
            _passes.RemoveAt(index);
        }

        public void RemoveAllPasses() => _passes.Clear();

        public void Render(SceneData sceneData, Framebuffer framebuffer = null)
        {
            Profiler.BeginScope("Drawing pipeline");

            framebuffer.Bind();
            framebuffer.EnableAllColorDrawBuffers(true);
            for (int i = 0; i < _passes.Count; i++)
            {
                _passes[i].Draw(sceneData.Scene, sceneData.ViewProjectionMatrix, framebuffer);
            }

            Profiler.EndScope();
        }
    }
}
