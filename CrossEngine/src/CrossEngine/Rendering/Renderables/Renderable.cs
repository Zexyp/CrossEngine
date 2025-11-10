using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using CrossEngine.Geometry;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Textures;

namespace CrossEngine.Rendering.Renderables
{
    public interface IRenderable
    {
        virtual void Init() { }
        virtual void Destroy() { }
        
        virtual void Begin(ICamera camera) { }
        virtual void End() { }

        void Submit(IObjectRenderData data);
    }

    public abstract class Renderable<T> : IRenderable where T : IObjectRenderData
    {
        public virtual void Init() { }
        public virtual void Destroy() { }
        
        public virtual void Begin(ICamera camera) { }
        public virtual void End() { }

        public abstract void Submit(T data);

        public void Submit(IObjectRenderData data) => Submit((T)data);
    }

    [Obsolete("not implemented")]
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    class RequiredRenderable<T> : Attribute where T : IRenderable
    {
    }

    public interface IObjectRenderData
    {
        Matrix4x4 Transform { get; }
        virtual int Id => 0;
        bool IsVisible { get; set; }
        IVolume GetVolume();
    }

    public interface ISkyboxRenderData : IObjectRenderData
    {
        Texture Texture { get; }
    }

    //interface IDrawable<T, D> where T : Renderable<D> where D : IObjectRenderData
    //{
    //    D RenderData { get; }
    //}
}