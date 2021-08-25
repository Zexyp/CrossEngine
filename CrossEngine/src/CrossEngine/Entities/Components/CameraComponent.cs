using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Events;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Serialization.Json;

namespace CrossEngine.Entities.Components
{
    public class CameraComponent : Component, ISerializable
    {
        public Camera Camera;

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

        #region ISerializable
        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue("Camera", Camera);
        }

        public CameraComponent(DeserializationInfo info)
        {
            Camera = (Camera)info.GetValue("Camera", typeof(Camera));
        }
        #endregion
    }
}
