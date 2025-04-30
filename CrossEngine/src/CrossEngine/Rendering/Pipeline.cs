using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;

namespace CrossEngine.Rendering;

public class Pipeline
{
    Vector4? _clearColor = VecColor.Gray;
    List<Pass> _passes = new List<Pass>();
    WeakReference<Framebuffer> Buffer;

    public void PushBack(Pass pass)
    {
        _passes.Add(pass);
        AttachPass(pass);
    }

    public void PushFront(Pass pass)
    {
        _passes.Insert(0, pass);
        AttachPass(pass);
    }

    public void Remove(Pass pass)
    {
        _passes.Remove(pass);
        DetachPass(pass);
    }

    public void Process(GraphicsContext context)
    {
        if (_clearColor != null)
        {
            var color = _clearColor.Value;
            context.Api.SetClearColor(color);
            context.Api.Clear();
        }
        
        for (int i = 0; i < _passes.Count; i++)
        {
            Pass pass = _passes[i];
            
            Profiler.BeginScope($"processing pass {pass.GetType().Name}");
                
            context.Api.SetDepthFunc(pass.Depth);
            context.Api.SetBlendFunc(pass.Blend);
            context.Api.SetPolygonMode(pass.PolyMode);
            context.Api.SetCullFace(pass.Cull);

            pass.Draw();

            Profiler.EndScope();
        }
    }

    public void Init()
    {
        for (int i = 0; i < _passes.Count; i++)
        {
            _passes[i].Init();
        }
    }
    
    public void Destroy()
    {
        for (int i = 0; i < _passes.Count; i++)
        {
            _passes[i].Destroy();
        }
    }

    public T GetPass<T>() where T : Pass => (T)GetPass(typeof(T));
    public Pass GetPass(Type type) => _passes.Find(p => p.GetType() == type);

    private void AttachPass(Pass pass)
    {
        Debug.Assert(pass.Pipeline == null);
        pass.Pipeline = this;
    }
    
    private void DetachPass(Pass pass)
    {
        Debug.Assert(pass.Pipeline == this);
        pass.Pipeline = null;
    }
}

public abstract class Pass
{
    public Pipeline Pipeline { get; internal set; }
    
    // cull
    public DepthFunc Depth;
    public BlendFunc Blend;
    public PolygonMode PolyMode = PolygonMode.Fill;
    public CullFace Cull;

    public virtual void Init() { }
    public virtual void Destroy() { }
    
    public abstract void Draw();
}