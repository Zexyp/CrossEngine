using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;

namespace CrossEngine.Entities.Components
{
    public class CameraComponent : Component
    {
        public Camera Camera;
        [EditorBooleanValue]
        public bool Primary = true;
        [EditorBooleanValue]
        public bool FixedAspectRatio = false;

        public Matrix4x4 ProjectionMatrix { get => Camera.ProjectionMatrix; }

        public Matrix4x4 ViewProjectionMatrix
        {
            get
            {
                Matrix4x4 transform = Matrix4x4.Identity;
                if (Entity.Transform != null) transform = Matrix4x4.CreateTranslation(-Entity.Transform.WorldPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Entity.Transform.WorldRotation));
                return transform * Camera.ProjectionMatrix;
            }
        }

        public CameraComponent()
        {
            Camera = new OrthographicCamera();
        }

        public CameraComponent(Camera camera)
        {
            Camera = camera;
        }

        public override void OnEvent(Event e)
        {
            if (e is WindowResizeEvent)
            {
                WindowResizeEvent wre = e as WindowResizeEvent;
                if (!FixedAspectRatio)
                {
                    if (Camera is OrthographicCamera)
                        ((OrthographicCamera)Camera).SetProjection(-wre.Width / 20, wre.Width / 20, -wre.Height / 20, wre.Height / 20);
                }
            }
        }

        public override void OnRender(RenderEvent re)
        {
            if (re is LineRenderEvent)
            {
                LineRenderer.DrawBox(Matrix4x4.CreateScale(new Vector3(2)) * Entity.Transform.WorldTransformMatrix * Camera.ProjectionMatrix, new Vector4(1, 0.8f, 0.2f, 1));
            }
        }

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Camera", Camera);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Camera = (Camera)info.GetValue("Camera", typeof(Camera));
        }
    }
}
