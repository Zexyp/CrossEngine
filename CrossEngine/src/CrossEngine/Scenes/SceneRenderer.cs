using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Scenes
{
    static class SceneRenderer
    {
        private static readonly Dictionary<Type, IRenderable> _renderables = new Dictionary<Type, IRenderable>(new InterfaceTypeComparer<IObjectRenderData>())
        {
            {typeof(ISpriteRenderData), new SpriteRenderable()},
            {typeof(IMeshRenderData), new MeshRenderable()},
        };

        public static void Init()
        {
            foreach (var rend in _renderables.Values)
            {
                rend.Init();
            }
        }

        public static void Shutdown()
        {
            foreach (var rend in _renderables.Values)
            {
                rend.Destroy();
            }
        }

        //private static void OnRender(ISurface surface, Scene scene)
        //{
        //    surface.Context.Api.SetViewport(0, 0, (uint)surface.Size.X, (uint)surface.Size.Y);
        //
        //    var camera = OverrideCamera ?? PrimaryCamera;
        //    if (camera == null)
        //        return;
        //
        //    foreach (var rend in _renderables.Values)
        //    {
        //        rend.Begin(camera);
        //    }
        //    foreach (IObjectRenderData rd in scene.World.Storage.GetIndex(typeof(RendererComponent)))
        //    {
        //        _renderables[rd.GetType()].Submit(rd);
        //    }
        //    foreach (var rend in _renderables.Values)
        //    {
        //        rend.End();
        //    }
        //}
        //
        //private static void OnResize(ISurface surface, Scene scene, float width, float height)
        //{
        //    _lastSize = new(width, height);
        //    _primaryCamera?.Resize(width, height);
        //    if (_overrideCamera != null && _overrideCamera is IResizableCamera rc)
        //        rc.Resize(width, height);
        //}

        // pot of boiling shit
        private class InterfaceTypeComparer<T> : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                if (x == y) return true;
                if (x.IsAssignableFrom(y)) return true;
                return false;
            }

            public int GetHashCode(Type obj)
            {
                if (obj.IsInterface)
                    return obj.GetHashCode();

                Type baseInterface = null;
                var ints = obj.GetInterfaces();
                for (int i = ints.Length - 1; i >= 0; i--)
                {
                    if (!typeof(T).IsAssignableFrom(ints[i]))
                        continue;

                    baseInterface = ints[i];
                    break;
                }
                return baseInterface?.GetHashCode() ?? obj.GetHashCode();
            }
        }
    }
}
