using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CrossEngine.Core;

[Obsolete("not implemented yet")]
class AttachableContainer
{
    private bool _initialized;
    readonly List<AttachableObject> _objects = new();

    public void Add(AttachableObject obj)
    {
        Debug.Assert(obj.Container == null);
        Debug.Assert(!_objects.Contains(obj));
        
        obj.Container = this;
        
        if (_initialized)
            obj.OnAttach();
        
        _objects.Add(obj);
    }

    public void Remove(AttachableObject obj)
    {
        Debug.Assert(obj.Container == this);
        Debug.Assert(_objects.Contains(obj));

        _objects.Remove(obj);
        
        if (_initialized)
            obj.OnDetach();
    }
    
    public T? Get<T>() where T : AttachableObject => (T)Get(typeof(T));
    public AttachableObject? Get(Type type) => _objects.Find(obj => obj.GetType() == type);
    
    public void Init()
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            _objects[i].OnAttach();
        }
        
        _initialized = true;
    }

    public void Destroy()
    {
        _initialized = false;
        
        for (int i = 0; i < _objects.Count; i++)
        {
            _objects[i].OnDetach();
        }
    }
}