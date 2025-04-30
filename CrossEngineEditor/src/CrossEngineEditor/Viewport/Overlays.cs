using CrossEngine.Components;
using CrossEngine.Debugging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

file class Util
{
    /// <summary>
    /// Convert to screen pos
    /// </summary>
    /// <returns>Is visible</returns>
    public static bool ToScreen(Vector3 scenePosition, Vector2 screenSize, ICamera camera, out Vector2 screenPos)
    {
        screenPos = Vector2.Zero;
        
        Vector4 clipSpace = Vector4.Transform(new Vector4(scenePosition, 1), camera.GetViewProjectionMatrix());
        //if (clipSpace.W == 0)
        //    return; // avoid divide by zero 😬
        if (clipSpace.Z < -1) // 0 or -1
            return false; // behind camera
        Vector3 ndc = new Vector3(clipSpace.X, clipSpace.Y, clipSpace.Z) / clipSpace.W;
        ndc.Y *= -1;
        Vector3 pos = (ndc + Vector3.One) / 2 * new Vector3(screenSize, 0);
        screenPos = new Vector2(pos.X, pos.Y);
        return true;
    }
}

namespace CrossEngineEditor.Viewport
{
    class TransformsOverlay : IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera Camera { get; set; }

        void IOverlay.Draw()
        {
            LineRenderer.BeginScene(Camera.GetViewProjectionMatrix());
            Context.Scene.World.GetSystem<TransformSystem>().RenderDebugLines();
            LineRenderer.EndScene();
        }

        public void Resize(float width, float height)
        {
        }
    }

    class SelectedOverlay : HudOverlay, IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera Camera { get; set; }

        [Obsolete("opengl specific")]
        protected override void Content()
        {
            var model = Context.ActiveEntity?.Transform?.WorldPosition;
            if (model == null)
                return;

            if (!Util.ToScreen(model.Value, Size, Camera, out var screenPos)) return;
            
            Renderer2D.DrawQuad(Matrix4x4.CreateScale(8) * Matrix4x4.CreateTranslation(new Vector3(screenPos, 0)), new Vector4(1f, .75f, 0,1));
        }
    }
    
    class NameOverlay : HudOverlay, IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera Camera { get; set; }

        [Obsolete("opengl specific")]
        protected override void Content()
        {
            var ents = Context.Scene?.Entities;
            if (ents == null)
                return;

            foreach (var ent in ents)
            {
                var model = ent.Transform?.WorldPosition;
                if (model == null)
                    continue;
                
                if (!Util.ToScreen(model.Value, Size, Camera, out var screenPos)) return;
                TextRendererUtil.DrawText(Matrix4x4.CreateScale(.5f) * Matrix4x4.CreateTranslation(new Vector3(screenPos, 0)), ent.ToString(), VecColor.White);
            }
        }
    }

    class CameraOverlay : IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera Camera { get; set; }

        public void Draw()
        {
            var cam = Context.Scene?.World.GetSystem<RenderSystem>().PrimaryCamera;
            cam ??= Context.ActiveEntity?.GetComponent<CameraComponent>();
            if (cam == null)
                return;
            var camTrans = cam.Entity?.Transform?.GetWorldTransformMatrix() ?? Matrix4x4.Identity;
            LineRenderer.BeginScene(Camera.GetViewProjectionMatrix());
            LineRenderer.DrawBox(Matrix4x4.CreateScale(2) * Matrix4x4Extension.SafeInvert(cam.ProjectionMatrix) * camTrans, VecColor.Black);
            LineRenderer.EndScene();
        }

        public void Resize(float width, float height)
        {
        }
    }

    class IconOverlay : IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera Camera { get; set; }

        public void Resize(float width, float height)
        {
        }

        public void Draw()
        {
            
        }
    }

    class ViewportCullCkecker : CullChecker, IViewportOverlay
    {
        public IEditorContext Context { get; set; }

        public void Prepare() => Activate();
        public void Finish() => Deactivate();
    }
}
