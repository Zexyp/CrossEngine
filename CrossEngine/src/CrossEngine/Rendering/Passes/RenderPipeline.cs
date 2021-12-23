using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Profiling;

namespace CrossEngine.Rendering.Passes
{
    public class RenderPipeline : IDisposable
    {
        public struct FramebufferStructureIndex
        {
            public int Color;
            public int Id;

            public static FramebufferStructureIndex Default
            {
                get
                {
                    return new FramebufferStructureIndex()
                    {
                        Color = 0,
                        Id = 1,
                    };
                }
            }
        }

        public FramebufferStructureIndex FbStructureIndex = FramebufferStructureIndex.Default;

        public Framebuffer Framebuffer;

        private List<RenderPass> _passes = new List<RenderPass>();

        public RenderPipeline()
        {
            var spec = new FramebufferSpecification();
            spec.Attachments = new FramebufferAttachmentSpecification(
                // using floating point colors
                new FramebufferTextureSpecification(TextureFormat.ColorRGBA32F) { Index = FbStructureIndex.Color },
                new FramebufferTextureSpecification(TextureFormat.ColorR32I) { Index = FbStructureIndex.Id },
                new FramebufferTextureSpecification(TextureFormat.Depth24Stencil8)
                );
            spec.Width = 1;
            spec.Height = 1;
            Framebuffer = new Framebuffer(spec);
        }

        public void Dispose()
        {
            Framebuffer?.Dispose();
            Framebuffer = null;
        }

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

        public void Render(SceneData sceneData)
        {
            Profiler.BeginScope("Drawing pipeline");

            Framebuffer.Bind();
            Framebuffer.EnableAllColorDrawBuffers(true);
            Profiler.BeginScope("Prepare");
            for (int i = 0; i < _passes.Count; i++)
            {
                _passes[i].GatherData(sceneData.Scene);
            }
            Profiler.EndScope();
            Profiler.BeginScope("Draw");
            for (int i = 0; i < _passes.Count; i++)
            {
                _passes[i].Draw(sceneData.Scene, sceneData.ViewProjectionMatrix, Framebuffer);
            }
            Profiler.EndScope();
            Profiler.BeginScope("Clear");
            for (int i = 0; i < _passes.Count; i++)
            {
                _passes[i].Clear();
            }
            Profiler.EndScope();

            Profiler.EndScope();
        }
    }
}
