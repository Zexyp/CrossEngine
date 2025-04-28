using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public void PushBack(Pass pass) => _passes.Add(pass);
    public void PushFront(Pass pass) => _passes.Insert(0, pass);
    public void Remove(Pass pass) => _passes.Remove(pass);

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
}

public abstract class Pass
{
    // cull
    public DepthFunc Depth;
    public BlendFunc Blend;
    public PolygonMode PolyMode = PolygonMode.Fill;
    public CullFace Cull;

    public virtual void Init() { }
    public virtual void Destroy() { }
    
    public abstract void Draw();
}