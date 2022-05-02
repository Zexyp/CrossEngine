using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class CameraComponent : Component
    {
        public Camera Camera;

        private bool _primary;
        public bool Primary
        {
            get => this == RendererSystem.Instance.Primary;
            set => RendererSystem.Instance.Primary = value ? this : null;
        }

        public Matrix4x4? ViewMatrix
        {
            get
            {
                if (Entity.TryGetComponent(out TransformComponent transformComp))
                    return Matrix4x4.CreateTranslation(-transformComp.WorldPosition) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(transformComp.WorldRotation));
                return null;
            }
        }
        
        public CameraComponent()
        {

        }

        public CameraComponent(Camera camera)
        {
            Camera = camera;
        }

        protected internal override void Attach()
        {
            RendererSystem.Instance.RegisterCamera(this);
        }

        protected internal override void Detach()
        {
            RendererSystem.Instance.UnregisterCamera(this);
        }

        protected override Component CreateClone()
        {
            Logging.Log.Core.Debug("CameraComponent says: 'panic!'");
            throw new NotImplementedException();
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            var sussy = new System.Diagnostics.StackFrame();
            Logging.Log.Core.Debug($"Impl this: in {sussy.GetFileName()} at line {sussy.GetFileLineNumber()}");
            Logging.Log.Core.Debug("CameraComponent says: 'panic!'");
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            var sussy = new System.Diagnostics.StackFrame();
            Logging.Log.Core.Debug($"Impl this: in {sussy.GetFileName()} at line {sussy.GetFileLineNumber()}");
            Logging.Log.Core.Debug("CameraComponent says: 'panic!'");
        }
    }
}
