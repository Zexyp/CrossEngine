using System.Collections.Generic;
using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Geometry;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using Microsoft.Win32;
using Silk.NET.Core.Native;

namespace CrossEngine.Components;

public class MeshRendererSystem : Ecs.System
{
    Dictionary<IMesh, MeshRenderer> _renderers = new();
    
    protected internal override void OnAttach()
    {
        World.Storage.AddNotifyRegister(typeof(MeshRendererComponent), RegisterMesh);
        World.Storage.AddNotifyUnregister(typeof(MeshRendererComponent), UnregisterMesh);
    }

    protected internal override void OnDetach()
    {
        World.Storage.RemoveNotifyRegister(typeof(MeshRendererComponent), RegisterMesh);
        World.Storage.RemoveNotifyUnregister(typeof(MeshRendererComponent), UnregisterMesh);
    }

    protected internal override void OnShutdown()
    {
        foreach (var renderer in _renderers.Values)
        {
            renderer.Dispose();
        }
    }

    private void RegisterMesh(Component component)
    {
        var mrc = (MeshRendererComponent)component;
        mrc.MeshChanged += OnMeshChanged;
        OnMeshChanged(mrc);
    }

    private void UnregisterMesh(Component component)
    {
        var mrc = (MeshRendererComponent)component;
        mrc.MeshChanged -= OnMeshChanged;
        OnMeshChanged(mrc);
    }

    private void OnMeshChanged(MeshRendererComponent component)
    {
        var mesh = component.Mesh?.Mesh;
        
        if (mesh == null) // no mesh
        {
            component.renderer = null;
            return;
        }
        
        if (_renderers.TryGetValue(mesh, out var renderer)) // already loaded
        {
            component.renderer = renderer;
            return;
        }
        else
        {
            var newRenderer = new MeshRenderer();
            _renderers.Add(mesh, newRenderer);
            component.renderer = newRenderer;
            
            World.GetSystem<RenderSystem>().Graphics.Commands.Submit(() => // deal with it
            {
                newRenderer.Setup(mesh);
            });
        }
    }
}