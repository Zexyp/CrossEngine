using System;

namespace CrossEngineEditor.Viewport;

[Obsolete("not implemented")]
public class ViewportOverlayManager
{
    IEditorContext _context;
    
    public void Init(IEditorContext context)
    {
        _context = context;
    }

    public void Destroy()
    {
        
    }
    
    public void AddOverlay(IViewportOverlay overlay)
    {
        
    }

    public void RemoveOverlay(IViewportOverlay overlay)
    {
        
    }
}