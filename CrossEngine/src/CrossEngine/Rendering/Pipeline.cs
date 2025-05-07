using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Rendering;

public class Pipeline
{
    Vector4? _clearColor = VecColor.Gray;
    List<Pass> _passes = new List<Pass>();
    public WeakReference<Framebuffer> Buffer { get; protected set; }
    public ICamera Camera;
    
    private bool _initialized;

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

    public void Process(ISurface surface)
    {
        Debug.Assert(_initialized);
        if (Camera is null)
            return;

        var rapi = surface.Context.Api;
        var buffer = Buffer.GetValue();

        if (surface.Size != buffer.Size)
        {
            buffer.Resize((uint)surface.Size.X, (uint)surface.Size.Y);
        }
        
        buffer.Bind();
        rapi.SetViewport(0, 0, (uint)surface.Size.X, (uint)surface.Size.Y);
        
        // clear
        if (_clearColor != null)
        {
            var color = _clearColor.Value;
            rapi.SetClearColor(color);
            rapi.Clear();
        }

        OnBeforePasses();
        
        Pass last = null;
        for (int i = 0; i < _passes.Count; i++)
        {
            Pass pass = _passes[i];
            
            Profiler.BeginScope($"processing pass {pass.GetType().Name}");

            buffer.EnableColorAttachments(pass.ModifyAttachments);

            rapi.SetDepthFunc(pass.Depth);
            rapi.SetBlendFunc(pass.Blend);
            //rapi.SetPolygonMode(pass.PolyMode); // mby in futere, now i need to debug
            rapi.SetCullFace(pass.Cull);
            rapi.SetDepthMask(pass.DepthMask);

            pass.Draw();

            Profiler.EndScope();
        }

        OnAfterPasses();
        
        buffer.BlitTo(surface.Buffer, new[] {(0, 0), (1, 1)});
        
        buffer.Unbind();
    }

    public void Init()
    {
        OnInit();
        
        for (int i = 0; i < _passes.Count; i++)
        {
            _passes[i].Init();
        }
        
        _initialized = true;
    }
    
    public void Destroy()
    {
        _initialized = false;
        
        for (int i = 0; i < _passes.Count; i++)
        {
            _passes[i].Destroy();
        }
        
        OnDestroy();
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
    
    protected virtual void OnInit() { }
    protected virtual void OnDestroy() { }
    protected virtual void OnBeforePasses() { }
    protected virtual void OnAfterPasses() { }
}

public abstract class Pass
{
    public Pipeline Pipeline { get; internal set; }

    // indexes
    public IList<int> ModifyAttachments = [0];
    
    // cull
    public DepthFunc Depth;
    public BlendFunc Blend;
    public PolygonMode PolyMode = PolygonMode.Fill;
    public CullFace Cull;
    public bool DepthMask = true;

    public virtual void Init() { }
    public virtual void Destroy() { }
    
    public abstract void Draw();
}