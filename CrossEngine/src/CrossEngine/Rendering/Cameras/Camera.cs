﻿using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Rendering.Cameras
{
    public class Camera : ISerializable
    {
        //_viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
        public Matrix4x4 ProjectionMatrix = Matrix4x4.Identity;

        public Camera()
        {

        }

        #region ISerializable
        public void OnSerialize(SerializationInfo info)
        {
            info.AddValue("ProjectionMatrix", ProjectionMatrix);
        }

        public void OnDeserialize(SerializationInfo info)
        {
            ProjectionMatrix = (Matrix4x4)info.GetValue("ProjectionMatrix", typeof(Matrix4x4));
        }
        #endregion
    }
}
