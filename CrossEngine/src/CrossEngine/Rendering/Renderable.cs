using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering
{
    // bare bones 💀
    public interface IRenderable
    {
        virtual void Begin(ICamera camera) { }
        virtual void End() { }

        void Submit(object data);
    }

    public abstract class Renderable<T> : IRenderable where T : IObjectRenderData
    {
        public virtual void Begin(ICamera camera) { }
        public virtual void End() { }

        public abstract void Submit(T data);

        public void Submit(IObjectRenderData data) => Submit((T)data);

        void IRenderable.Submit(object data) => Submit((IObjectRenderData)data);
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
        virtual Matrix4x4 Transform { get => throw new NotImplementedException(); }
        virtual int EntityId => 0;
    }

    //interface IDrawable<T, D> where T : Renderable<D> where D : IObjectRenderData
    //{
    //    D RenderData { get; }
    //}
}
