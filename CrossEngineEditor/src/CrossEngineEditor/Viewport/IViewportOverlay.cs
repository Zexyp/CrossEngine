using CrossEngine.Debugging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Viewport
{
    public interface IViewportOverlay : ISceneOverlay, IPassConfig
    {
        protected internal IEditorContext Context { get; internal set; }
        virtual IList<int> ModifyAttachments => [0];

        virtual void Prepare() { }
        virtual void Finish() { }
        virtual void Init() { }
        virtual void Destroy() { }
    }
    
    class ViewportWrapOverlay : IViewportOverlay
    {
        public ICamera Camera { get => _overlay.Camera; set => _overlay.Camera = value; }
        public IEditorContext Context { get; set; }

        public ISceneOverlay _overlay;
        
        public ViewportWrapOverlay(ISceneOverlay overlay)
        {
            _overlay = overlay;
        }

        public void Resize(float width, float height) => _overlay.Resize(width, height);
        public void Draw() => _overlay.Draw();
    }
}
