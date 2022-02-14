using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CrossEngine.Rendering
{
    public abstract class Renderable
    {
        public virtual void Begin() { }
        public virtual void End() { }

        public abstract void Submit(IObjectRenderData data);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class RequiredRenderDataType : Attribute
    {
        Type RequiredType;
        public RequiredRenderDataType(Type type)
        {
            if (!type.IsInterface || !type.GetInterfaces().Contains(typeof(IObjectRenderData)))
                throw new ArgumentException();

            RequiredType = type;
        }
    }

    public interface IObjectRenderData
    {
        //Matrix4x4 Transform { get; }
    }

    interface IDrawable<T, D> where T : Renderable where D : IObjectRenderData
    {
        D RenderData { get; }
    }
}
