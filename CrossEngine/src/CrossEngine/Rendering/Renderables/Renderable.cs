using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Rendering.Cameras;
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
        virtual bool Visible { get => true; }
        virtual int Id { get => throw new NotImplementedException(); }
    }
    
    interface IMeshRenderData : IObjectRenderData
    {
        IMesh Mesh { get; }
        IMaterial Material { get; }
    }
    
    interface ISpriteRenderData : IObjectRenderData
    {
        Vector4 Color { get; }
        virtual Vector4 TextureOffsets => new Vector4(0, 0, 1, 1);
        virtual WeakReference<Texture> Texture => null;
        virtual BlendMode Blend => BlendMode.Opaque;
        virtual Vector4 DrawOffsets => new Vector4(0, 0, 1, 1);
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