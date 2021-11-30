using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Events;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Entities.Components
{
    public class CameraComponent : Component
    {
        public Camera Camera;
        [EditorBooleanValue]
        public bool Primary;

        //public bool FixedAspectRatio = false;

        public CameraComponent()
        {
            
        }

        public CameraComponent(Camera camera)
        {
            Camera = camera;
        }

        //public override void OnEvent(Event e)
        //{
        //    EventDispatcher dispatcher = new EventDispatcher(e);
        //    dispatcher.Dispatch<WindowResizeEvent>(() =>
        //    {
        //        if (!FixedAspectRatio)
        //        {
        //            if (Camera is OrthographicCamera)
        //                ((OrthographicCamera)Camera).SetProjection();
        //            Resize(e.Width, e.Height);
        //        }
        //    });
        //}

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
