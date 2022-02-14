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
using CrossEngine.Utils;
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
            string scenePath = new IniFile("filesystem", true).Read("files", "InitialScene");
            if (!File.Exists("filesystem") || String.IsNullOrEmpty(scenePath))
            {
                Log.App.Fatal("cannot resolve initial scene");
                Application.Instance.Window.ShouldClose = true;
                return;
            }

            scene = SceneLoader.Load(scenePath);

            scene.Pipeline = new RenderPipeline();
            scene.Pipeline.RegisterPass(new Renderer2DPass());
            scene.Pipeline.RegisterPass(new LineRenderPass());

            scene.Load();
            scene.Start();
        }

        public override void OnDetach()
        {
            if (scene != null)
            {
                scene.End();
                scene.Unload();
                scene.Destroy();
                scene = null;
            }
        }

        public override void OnUpdate(float timestep)
        {
            if (scene != null)
            {
                scene.OnUpdateRuntime(Math.Min(Time.DeltaTimeF, 0.032f));
            }
        }

        public override void OnRender()
        {
            Renderer.Clear();

            ImGuiLayer.Instance.Begin();

            if (scene != null)
            {
                scene.Pipeline.Framebuffer.Bind();
                Renderer.Clear();
                scene.OnRenderRuntime();
                Framebuffer.Unbind();
                scene.Pipeline.Framebuffer.CopyToScreen();
            }
            ImGui.ShowDemoWindow();

            ImGuiLayer.Instance.End();
        }

        public override void OnEvent(Event e)
        {
            if (scene != null)
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
}
