using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using CrossEngine.Debugging;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;

namespace CrossEngine.Utils;

public class CullChecker : ISceneOverlay
{
    const int MaxBufferSize = 1024;
    
    private static CullChecker active;
    
    public static void Append(IVolume volume)
    {
        if (volume == null || active == null) return;

        if (active._volumes.Count >= MaxBufferSize)
        {
            Log.Default.Warn("CullChecker: buffer limit reached");
        }
        active._volumes.Enqueue(volume);
    }

    public void Activate()
    {
        active = this;
    }

    public void Deactivate()
    {
        active = null;
    }
    
    public ICamera Camera { get; set; }

    private readonly Queue<IVolume> _volumes = new Queue<IVolume>();
    
    public void Draw()
    {
        LineRenderer.BeginScene(Camera.GetViewProjectionMatrix());
        
        var frustum = Camera.GetFrustum();
        while (_volumes.TryDequeue(out var volume))
        {
            var color = volume.IsInFrustum(frustum) switch
            {
                Halfspace.Inside => new Vector4(.2f, .2f, 1, 1),
                Halfspace.Intersect => new Vector4(1, .2f, .2f, 1),
                _ => new Vector4(1, 1, 1, 1),
            };

            switch (volume)
            {
                case AABox box:
                    LineRenderer.DrawBox(box.corner, box.corner + new Vector3(box.x, box.y, box.z), color);
                    break;
                case Sphere sphere:
                    LineRenderer.DrawSphere(sphere.center, color, sphere.radius);
                    break;
                default: Debug.Fail($"Unknown volume type: {volume.GetType()}"); break;
            }
        }
        
        LineRenderer.EndScene();
    }

    public void Resize(float width, float height)
    {
    }
}