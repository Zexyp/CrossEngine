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

    //[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    //class RequiredRenderDataType : Attribute
    //{
    //    Type RequiredType;
    //    public RequiredRenderDataType(Type type)
    //    {
    //        if (!type.IsInterface || !type.GetInterfaces().Contains(typeof(IObjectRenderData)))
    //            throw new ArgumentException();
    //
    //        RequiredType = type;
    //    }
    //}

    public interface IObjectRenderData
    {
        Matrix4x4 Transform { get; }
        virtual int Id { get => throw new NotImplementedException(); }
        bool IsVisible { get; set; }
        IVolume GetVolume();
    }

    interface ISkyboxRenderData : IObjectRenderData
    {
        WeakReference<Texture> Texture { get; }
    }

    //interface IDrawable<T, D> where T : Renderable<D> where D : IObjectRenderData
    //{
    //    D RenderData { get; }
    //}
}