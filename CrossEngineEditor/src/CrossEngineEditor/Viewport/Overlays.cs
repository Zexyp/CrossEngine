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

namespace CrossEngineEditor.Viewport
{
    class TransformsOverlay : IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera EditorCamera { get; set; }

        void IOverlay.Draw()
        {
            if (Context.Scene?.Initialized != true)
                return;
            LineRenderer.BeginScene(EditorCamera.GetViewProjectionMatrix());
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
        public ICamera EditorCamera { get; set; }

        protected override void Content()
        {
            var model = Context.ActiveEntity?.Transform?.WorldPosition;
            if (model == null)
                return;
            
            Vector4 clipSpace = Vector4.Transform(new Vector4(model.Value, 1), EditorCamera.GetViewProjectionMatrix());
            //if (clipSpace.W == 0)
            //    return; // avoid divide by zero 😬
            Vector3 ndc = new Vector3(clipSpace.X, clipSpace.Y, clipSpace.Z) / clipSpace.W;
            ndc.Y *= -1;
            Vector3 screenPos = (ndc + Vector3.One) / 2 * new Vector3(Size, 0);
            Renderer2D.DrawQuad(Matrix4x4.CreateScale(8) * Matrix4x4.CreateTranslation(screenPos), new Vector4(1f, .75f, 0,1));
        }
    }
    
    class NameOverlay : HudOverlay, IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera EditorCamera { get; set; }

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
                
                Vector4 clipSpace = Vector4.Transform(new Vector4(model.Value, 1), EditorCamera.GetViewProjectionMatrix());
                //if (clipSpace.W == 0)
                //    return; // avoid divide by zero 😬
                Vector3 ndc = new Vector3(clipSpace.X, clipSpace.Y, clipSpace.Z) / clipSpace.W;
                ndc.Y *= -1;
                Vector3 screenPos = (ndc + Vector3.One) / 2 * new Vector3(Size, 0);
                TextRendererUtil.DrawText(Matrix4x4.CreateScale(.5f) * Matrix4x4.CreateTranslation(screenPos), ent.ToString(), VecColor.White);
            }
        }
    }

    class CameraOverlay : IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera EditorCamera { get; set; }

        public void Draw()
        {
            var cam = Context.Scene?.World.GetSystem<RenderSystem>().PrimaryCamera;
            cam ??= Context.ActiveEntity?.GetComponent<CameraComponent>();
            if (cam == null)
                return;
            var camTrans = cam.Entity?.Transform?.GetWorldTransformMatrix() ?? Matrix4x4.Identity;
            LineRenderer.BeginScene(EditorCamera.GetViewProjectionMatrix());
            LineRenderer.DrawAxies(camTrans);
            LineRenderer.DrawBox(Matrix4x4.CreateScale(2) * Matrix4x4Extension.SafeInvert(cam.ProjectionMatrix) * camTrans, VecColor.Black);
            LineRenderer.EndScene();
        }

        public void Resize(float width, float height)
        {
        }
    }
}
