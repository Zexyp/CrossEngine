using CrossEngine.Components;
using CrossEngine.Debugging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
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
            Vector3 positon = Vector3.Transform(model.Value, EditorCamera.GetViewProjectionMatrix());
            positon.Y *= -1;
            positon = (positon + Vector3.One) / 2;
            positon *= new Vector3(Size, 0);
            Renderer2D.DrawQuad(Matrix4x4.CreateScale(8) * Matrix4x4.CreateTranslation(positon), new Vector4(0.96468f, 0.658388f, 0.001517f, 1));
        }
    }

    class CameraOverlay : IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera EditorCamera { get; set; }

        public void Draw()
        {
            var cam = Context.Scene.World.GetSystem<RenderSystem>().PrimaryCamera;
            if (cam == null)
                return;
            var camTrans = cam.Entity?.Transform?.GetWorldTransformMatrix() ?? Matrix4x4.Identity;
            LineRenderer.BeginScene(EditorCamera.GetViewProjectionMatrix());
            LineRenderer.DrawAxies(camTrans);
            LineRenderer.DrawBox(Matrix4x4Extension.Invert(cam.ProjectionMatrix) * camTrans, VecColor.Green);
            LineRenderer.EndScene();
        }

        public void Resize(float width, float height)
        {
        }
    }
}
