using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

using CrossEngine;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Layers;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Inputs;
using CrossEngine.Rendering.Textures;
using CrossEngine.Scenes;
using CrossEngine.Entities;
using CrossEngine.Entities.Components;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;
using CrossEngine.Rendering.Lines;
using CrossEngine.Assets;
using CrossEngine.Rendering.Passes;

namespace CrossEngineRuntime
{
    class SceneLayer : Layer
    {
        Scene scene;

        public override void OnAttach()
        {
            scene = SceneLoader.Load("test scene/scene.json");

            scene.Pipeline = new RenderPipeline();
            scene.Pipeline.RegisterPass(new Renderer2DPass());
            scene.Pipeline.RegisterPass(new LineRenderPass());

            scene.Load();
            scene.Start();
        }

        public override void OnDetach()
        {
            scene.End();
            scene.Unload();
            scene.Destroy();
        }

        public override void OnUpdate(float timestep)
        {
            scene.OnUpdateRuntime(Math.Min(Time.DeltaTimeF, 0.032f));
        }

        public override void OnRender()
        {
            Renderer.Clear();

            ImGuiLayer.Instance.Begin();

            scene.Pipeline.Framebuffer.Bind();
            Renderer.Clear();
            scene.OnRenderRuntime();
            scene.Pipeline.Framebuffer.CopyToScreen();
            Framebuffer.Unbind();
            //ImGui.ShowDemoWindow();
            ImGuiLayer.Instance.End();
        }

        public override void OnEvent(Event e)
        {
            scene.OnEvent(e);
            if (e is WindowResizeEvent)
            {
                var wre = (WindowResizeEvent)e;
                scene.Pipeline.Framebuffer.Resize(wre.Width, wre.Height);
            }
        }
    }
}
