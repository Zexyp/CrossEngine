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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.FX.Particles;
using CrossEngine.Loaders;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Rendering.Textures;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.Rendering;

file class Util
{
    /// <summary>
    /// Convert to screen pos
    /// </summary>
    /// <returns>Is visible</returns>
    [Obsolete("opengl specific")]
    public static bool ToScreen(Vector3 scenePosition, Vector2 screenSize, ICamera camera, out Vector2 screenPos)
    {
        screenPos = Vector2.Zero;

        Vector4 clipSpace = Vector4.Transform(new Vector4(scenePosition, 1), camera.GetViewProjectionMatrix());

        if (clipSpace.W <= 0)
            return false; // behind or invalid

        Vector3 ndc = new Vector3(clipSpace.X, clipSpace.Y, clipSpace.Z) / clipSpace.W;

        if (ndc.Z < 0 || ndc.Z > 1)
            return false; // outside view frustum

        // Flip Y if needed based on screen origin
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

        [EditorDrag]
        public float Length = 1;

        void IOverlay.Draw()
        {
            LineRenderer.BeginScene(Camera.GetViewProjectionMatrix());
            var arr = Context.Scene?.World.Storage.GetArray(typeof(TransformComponent));
            if (arr == null)
                return;
            
            for (int i = 0; i < arr.Count; i++)
            {
                LineRenderer.DrawAxies(Matrix4x4.CreateScale(Length) * ((TransformComponent)arr[i]).GetWorldTransformMatrix());
            }
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

        public const float PointSize = 8;
        public const float PointOutlineSize = 4;
        
        protected override void Content()
        {
            var model = Context.ActiveEntity?.Transform?.WorldPosition;
            if (model == null)
                return;

            if (!Util.ToScreen(model.Value, Size, Camera, out var screenPos)) return;
            
            var translation = Matrix4x4.CreateTranslation(new Vector3(screenPos, 0));
            Renderer2D.DrawQuad(Matrix4x4.CreateRotationZ(MathF.PI / 4) * Matrix4x4.CreateScale(PointSize + PointOutlineSize) * translation, VecColor.Black);
            Renderer2D.DrawQuad(Matrix4x4.CreateRotationZ(MathF.PI / 4) * Matrix4x4.CreateScale(PointSize) * translation, new Vector4(1f, .75f, 0, 1));
        }
    }
    
    class NameOverlay : HudOverlay, IViewportOverlay
    {
        public IEditorContext Context { get; set; }
        public ICamera Camera { get; set; }
        public IList<int> ModifyAttachments => [0, 1];

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

                screenPos += new Vector2(SelectedOverlay.PointSize + SelectedOverlay.PointOutlineSize) / 2;
                TextRendererUtil.DrawText(Matrix4x4.CreateScale(.5f) * Matrix4x4.CreateTranslation(new Vector3(screenPos, 0)), ent.ToString(),
                    VecColor.White,
                    ent.Id);
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
        public IList<int> ModifyAttachments => [DeferredPipeline.AttachmentIndexColor, DeferredPipeline.AttachmentIndexId];
        [EditorDrag]
        public float Size = 1;
        
        private WeakReference<Texture> _iconTexture;
        private Vector4[] _offsets = TextureAtlas.CreateOffsets(new Vector2(80, 16), new Vector2(16, 16), 5);

        private const int IconCamera = 0;
        private const int IconBulb = 2;
        private const int IconSun = 3;
        private const int IconParticles = 4;
        
        List<(Type CompType, int Icon)> _iconLookup = new()
        {
            (typeof(CameraComponent), IconCamera),
            (typeof(ParticleSystemComponent), IconParticles),
            (typeof(LightComponent), IconBulb),
        };

        public void Resize(float width, float height)
        {
        }

        public void Draw()
        {
            if (Context.Scene == null)
                return;

            var viewMatrix = Camera.GetViewMatrix();
            viewMatrix.Translation = Vector3.Zero;
            viewMatrix = Matrix4x4Extension.SafeInvert(viewMatrix);
            
            var cameraRight = Vector3.Transform(Vector3.UnitX, viewMatrix);
            var cameraUp = Vector3.Transform(Vector3.UnitY, viewMatrix);
            Renderer2D.BeginScene(Camera.GetViewProjectionMatrix());
            Renderer2D.SetBlending(BlendMode.Blend);
            foreach (var pair in _iconLookup)
            {
                foreach (var comp in Context.Scene.World.Storage.EnumerateSubclasses(pair.CompType))
                {
                    var pos = comp.Entity.Transform?.WorldPosition ?? Vector3.Zero;
                    var matrix = Matrix4x4.CreateScale(Size) * Matrix4x4Extension.CreateBillboard(cameraUp, cameraRight, Vector3.Zero, pos);
                    Renderer2D.DrawTexturedQuad(matrix, _iconTexture, VecColor.White, _offsets[pair.Icon], comp.Entity.Id);
                }
            }
            Renderer2D.EndScene();
        }

        public void Init()
        {
            _iconTexture = TextureLoader.LoadTextureFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CrossEngineEditor.res.icons.png"));
            _iconTexture.GetValue().SetFilterParameter(FilterParameter.Nearest);
        }

        public void Destroy()
        {
            _iconTexture.Dispose();
            _iconTexture = null;
        }
    }

    class ViewportCullCkecker : CullChecker, IViewportOverlay
    {
        public IEditorContext Context { get; set; }

        public void Prepare() => Activate();
        public void Finish() => Deactivate();
    }

    class EmitterOverlay : IViewportOverlay
    {
        public ICamera Camera { get; set; }
        public IEditorContext Context { get; set; }
        
        public void Resize(float width, float height)
        {
        }

        public void Draw()
        {
            var pscs = Context.Scene?.World.Storage.GetArray(typeof(ParticleSystemComponent));
            if (pscs == null)
                return;
            
            LineRenderer.BeginScene(Camera.GetViewProjectionMatrix());
            for (int i = 0; i < pscs.Count; i++)
            {
                var comp = pscs[i];
                ((ParticleSystemComponent)comp).Emitter?.DebugDraw(comp.Entity.Transform?.GetWorldTransformMatrix() ?? Matrix4x4.Identity);
            }
            LineRenderer.EndScene();
        }
    }

    class LightOverlay : IViewportOverlay
    {
        public void Resize(float width, float height)
        {
        }

        public void Draw()
        {
            throw new NotImplementedException();
            
            var light = Context.ActiveEntity?.GetComponent<LightComponent>();
            if (light == null)
                return;
            
            LineRenderer.BeginScene(Camera.GetViewProjectionMatrix());

            var lightData = (ILightRenderData)light;
            
            LineRenderer.EndScene();
        }

        public ICamera Camera { get; set; }
        public IEditorContext Context { get; set; }
    }
}
