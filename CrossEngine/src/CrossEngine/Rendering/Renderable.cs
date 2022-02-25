using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CrossEngine.Rendering
{
    public interface IRenderable
    {
        virtual void Begin(Matrix4x4 viewProjectionMatrix) { }
        virtual void End() { }

        void Submit(IObjectRenderData data);
    }

    public abstract class Renderable<T> : IRenderable where T : IObjectRenderData
    {
        public virtual void Begin(Matrix4x4 viewProjectionMatrix) { }
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
        virtual Matrix4x4 Transform { get => throw new NotImplementedException(); }
    }

    interface IDrawable<T, D> where T : Renderable<D> where D : IObjectRenderData
    {
        D RenderData { get; }
    }
}
