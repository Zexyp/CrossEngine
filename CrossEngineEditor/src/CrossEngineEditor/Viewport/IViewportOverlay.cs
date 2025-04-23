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
    public interface IViewportOverlay : IOverlay
    {
        protected internal IEditorContext Context { get; internal set; }
        protected internal ICamera EditorCamera { get; internal set; }
        //protected internal virtual void Init() { }
        //protected internal virtual void Destroy() { }
    }
}
