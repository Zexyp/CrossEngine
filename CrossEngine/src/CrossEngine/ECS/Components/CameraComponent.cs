using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class CameraComponent : Component
    {
        public Camera Camera;

        private bool _primary;
        [EditorValue]
        public bool Primary
        {
            get => this == _boundTo.Primary;
            set => _boundTo.Primary = value ? this : null;
        }

        private RendererSystem _boundTo;

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
            Camera = new Camera();
        }

        protected internal override void Attach(World world)
        {
            world.GetSystem<RendererSystem>().RegisterCamera(this);
            _boundTo = world.GetSystem<RendererSystem>();
        }

        protected internal override void Detach(World world)
        {
            world.GetSystem<RendererSystem>().UnregisterCamera(this);
            _boundTo = null;
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
