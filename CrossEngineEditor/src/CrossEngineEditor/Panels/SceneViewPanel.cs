﻿using ImGuiNET;
using System;
using System.Numerics;

using CrossEngine;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using CrossEngine.Scenes;
using CrossEngine.Services;
using CrossEngine.Ecs;
using CrossEngine.Platform.OpenGL;

namespace CrossEngineEditor.Panels
{
    public class SceneViewPanel : EditorPanel
    {
        protected virtual ICamera DrawCamera { get => null; }
        protected virtual Scene Scene { get => Context.Scene; }

        protected Vector2 ViewportSize { get; private set; }
        protected WeakReference<Framebuffer> Framebuffer { get; private set; }
        protected bool ViewportResized;
        private RenderService rs;
        protected FramebufferSurface Surface;

        public SceneViewPanel(RenderService rs) : base("Scene View")
        {
            this.rs = rs;
            Surface = new FramebufferSurface();
        }

        protected override void PrepareWindow()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        protected override void EndPrepareWindow()
        {
            ImGui.PopStyleVar();
        }

        protected override void DrawWindowContent()
        {
            // resize check
            {
                ViewportResized = false;
                Vector2 viewportPanelSize = ImGui.GetContentRegionAvail();
                if (viewportPanelSize != ViewportSize && viewportPanelSize.X > 0 && viewportPanelSize.Y > 0)
                {
                    ViewportSize = viewportPanelSize;

                    //var fb = Framebuffer.GetValue();
                    //fb.Resize((uint)ViewportSize.X, (uint)ViewportSize.Y);

                    OnCameraResize();

                    ViewportResized = true;
                }
            }

            if (Scene?.IsInitialized != true || Framebuffer == null)
            {
                ImGui.TextDisabled("No scene");
                return;
            }

            // needs to be set back so SceneManager can render only from given scene data
            // as of latest rewrite this is not valid
            // wtf is this comment
            var renderSys = Scene.World.GetSystem<RenderSystem>();
            renderSys.OverrideCamera = DrawCamera;

            Framebuffer.GetValue().Bind();
            ((GLFramebuffer)Framebuffer.GetValue()).EnableAllColorAttachments(true);
            
            Surface.Context.Api.Clear();
            lock (Scene)
            {
                Surface.DoUpdate();
            }
            
            ((GLFramebuffer)Framebuffer.GetValue()).EnableColorAttachment(1, false);
            Framebuffer.GetValue().Unbind();

            renderSys.OverrideCamera = null;

            // draw the framebuffer as image
            ImGui.Image(new IntPtr(Framebuffer.GetValue()?.GetColorAttachmentRendererID(0) ?? 0),
                ViewportSize,
                new Vector2(0, 1),
                new Vector2(1, 0));

        }

        public override void OnOpen()
        {
            var spec = new FramebufferSpecification();
            spec.Attachments = new FramebufferAttachmentSpecification(
                // using floating point colors
                new FramebufferTextureSpecification(TextureFormat.ColorRGBA16F),
                new FramebufferTextureSpecification(TextureFormat.ColorR32I),
                new FramebufferTextureSpecification(TextureFormat.Depth24Stencil8)
                );
            spec.Width = 1;
            spec.Height = 1;

            ViewportSize = -Vector2.One;

            rs.Execute(() =>
            {
                Framebuffer = CrossEngine.Rendering.Buffers.Framebuffer.Create(in spec);
                Surface.Context = rs.MainSurface.Context;
                Surface.Buffer = Framebuffer;
                Surface.Update += OnSurfaceUpdate;
                OnCameraResize();
            });
        }

        public override void OnClose()
        {
            rs.Execute(() =>
            {
                Framebuffer.Dispose();
                Framebuffer = null;
            });
        }

        protected virtual void OnCameraResize()
        {
            Surface.DoResize(ViewportSize.X, ViewportSize.Y);
        }

        private void OnSurfaceUpdate(ISurface surface)
        {
            SceneRenderer.Render(Scene, surface);
        }
    }
}