using System;

namespace CrossEngine.Core;

[Obsolete("not implemented yet")]
abstract class AttachableObject
{
    internal protected AttachableContainer Container { get; internal set; }
    
    public virtual void OnInit() { }
    public virtual void OnDestroy() { }
    
    public virtual void OnAttach() { }
    public virtual void OnDetach() { }
}