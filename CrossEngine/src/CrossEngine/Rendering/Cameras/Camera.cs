using System;
using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Serialization;
using CrossEngine.Rendering.Culling;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Rendering.Cameras
{
    public interface ICamera
    {
        Matrix4x4 ViewMatrix { get; }
        Matrix4x4 ProjectionMatrix { get; }
        virtual Matrix4x4 ViewProjectionMatrix { get => ViewMatrix * ProjectionMatrix; }

        Frustum Frustum { get; }
    }

    public class Camera : ICamera, ISerializable
    {
        public enum ProjectionType
        {
            Perspective,
            Orthographic,
        }

        //_viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
        public virtual Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
        public virtual Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 ViewProjectionMatrix { get => ViewMatrix * ProjectionMatrix; }
        public Frustum Frustum { get; set; }
        
        [EditorEnum]
        public ProjectionType Projection { get => _projection; set { _projection = value; RecreateProjection(); } }
        [EditorDrag(Min = float.Epsilon)]
        public float Width { get => _width; set { _width = value; RecreateProjection(); } }
        [EditorDrag(Min = float.Epsilon)]
        public float Height { get => _height; set { _height = value; RecreateProjection(); } }
        [EditorDisplay]
        public float AspectRatio { get => _width / _height; }
        [EditorDrag]
        public float Near { get => _near; set { _near = value; RecreateProjection(); } }
        [EditorDrag]
        public float Far { get => _far; set { _far = value; RecreateProjection(); } }
        [EditorSection("Perspective")]
        [EditorDrag(Min = float.Epsilon, Max = 179.999985f)]
        public float FOV { get => _p_fov; set { _p_fov = value; RecreateProjection(); } }
        [EditorSection("Orthographic")]
        [EditorDrag(Min = float.Epsilon)]
        public float OrthoSize { get => _o_size; set { _o_size = value; RecreateProjection(); } }

        // TODO: finish this
        private ProjectionType _projection = ProjectionType.Orthographic;
        private bool _resize = true;
        private float _width = 1, _height = 1;
        private float _near = .1f, _far = 100;

        private float _p_fov = 90;
        private float _o_size = 1;

        public Camera()
        {
            
        }

        public void SetOrtho()
        {
            Projection = ProjectionType.Orthographic;
        }

        public void SetPerspective()
        {
            Projection = ProjectionType.Perspective;
        }

        public virtual void Resize(float width, float height)
        {
            _width = width;
            _height = height;
            RecreateProjection();
        }

        private void RecreateProjection()
        {
            if (Projection == ProjectionType.Perspective)
                ProjectionMatrix = Matrix4x4Extension.Perspective(MathExtension.ToRadians(_p_fov), AspectRatio, _near, _far);
            if (Projection == ProjectionType.Orthographic)
                ProjectionMatrix = Matrix4x4Extension.Ortho(-_o_size * AspectRatio, _o_size * AspectRatio, -_o_size, _o_size, Near, Far);
        }

        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(Projection), Projection);
            info.AddValue(nameof(Width), Width);
            info.AddValue(nameof(Height), Height);
            info.AddValue(nameof(Near), Near);
            info.AddValue(nameof(Far), Far);
            info.AddValue(nameof(FOV), FOV);
            info.AddValue(nameof(OrthoSize), OrthoSize);
        }
        
        public void SetObjectData(SerializationInfo info)
        {
            Projection = info.GetValue(nameof(Projection), Projection);
            Width = info.GetValue(nameof(Width), Width);
            Height = info.GetValue(nameof(Height), Height);
            Near = info.GetValue(nameof(Near), Near);
            Far = info.GetValue(nameof(Far), Far);
            FOV = info.GetValue(nameof(FOV), FOV);
            OrthoSize = info.GetValue(nameof(OrthoSize), OrthoSize);
        }
    }
}
