using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;
using CrossEngine.Utils;

namespace CrossEngine.Entities.Components
{
    public class CameraComponent : Component
    {
        public enum CameraType
        {
            Undefined,
            Orthographic,
            Perspective,
        }

        [EditorBooleanValue]
        public bool Primary = true;
        [EditorBooleanValue]
        public bool FixedAspectRatio = false;
        [EditorEnumValue]
        public CameraType Type
        {
            get => _type;
            set
            {
                _type = value;
                switch (Type)
                {
                    case CameraType.Orthographic:
                        _camera = new OrthographicCamera();
                        break;
                    case CameraType.Perspective:
                        _camera = new PerspectiveCamera();
                        break;
                }
            }
        }

        [EditorInnerValue]
        public Camera Camera
        {
            get => _camera;
            set
            {
                if (_camera == value) return;
                _camera = value;
                if (_camera is OrthographicCamera) _type = CameraType.Orthographic;
                else if (_camera is PerspectiveCamera) _type = CameraType.Perspective;
                else _type = CameraType.Undefined;
            }
        }

        public Matrix4x4 ProjectionMatrix { get => _camera.ProjectionMatrix; }

        public Matrix4x4 ViewProjectionMatrix
        {
            get
            {
                Matrix4x4 transform = Matrix4x4.Identity;
                if (Entity.Transform != null) transform = Matrix4x4.CreateTranslation(-Entity.Transform.WorldPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Entity.Transform.WorldRotation));
                return transform * _camera.ProjectionMatrix;
            }
        }

        public CameraType _type = CameraType.Orthographic;
        private Camera _camera;

        #region Constructors
        public CameraComponent()
        {
            switch (Type)
            {
                case CameraType.Undefined:
                    break;

                case CameraType.Orthographic:
                    _camera = new OrthographicCamera();
                    break;
                case CameraType.Perspective:
                    _camera = new PerspectiveCamera();
                    break;
            }
        }

        public CameraComponent(CameraType type)
        {
            Type = type;

            switch (Type)
            {
                case CameraType.Undefined:
                    break;

                case CameraType.Orthographic:
                    _camera = new OrthographicCamera();
                    break;
                case CameraType.Perspective:
                    _camera = new PerspectiveCamera();
                    break;
            }
        }

        public CameraComponent(Camera camera)
        {
            Camera = camera;
        }
        #endregion

        public override void OnEvent(Event e)
        {
            if (e is WindowResizeEvent)
            {
                WindowResizeEvent wre = e as WindowResizeEvent;
                if (!FixedAspectRatio)
                {
                    _camera.Resize(wre.Width, wre.Height);
                }
            }
        }

        public override void OnRender(RenderEvent re)
        {
            if (re is EditorDrawRenderEvent && Entity.Transform != null)
            {
                LineRenderer.DrawBox(
                    Matrix4x4.CreateScale(new Vector3(2)) *
                    Matrix4x4Extension.Invert(_camera.ProjectionMatrix) *
                    Matrix4x4.CreateFromQuaternion(Entity.Transform.WorldRotation) *
                    Matrix4x4.CreateTranslation(Entity.Transform.WorldPosition), new Vector4(1, 0.8f, 0.2f, 1));
            }
        }

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Type", Type);
            info.AddValue("Camera", Camera);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Type = (CameraType)info.GetValue("Type", typeof(CameraType));
            Camera = (Camera)info.GetValue("Camera", typeof(Camera));
        }

        public void Resize(float x, float y)
        {
            Camera.Resize(x, y);
        }
    }
}
