using CrossEngine.Core;
using CrossEngine.Inputs;
using CrossEngine.Rendering.Cameras;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Geometry;
using CrossEngine.Utils.Maths;

namespace CrossEngine.Utils.Rendering
{
    public class FreeCamera : IResizableCamera
    {
        public Matrix4x4 ProjectionMatrix { get => Matrix4x4.CreatePerspectiveFieldOfView(MathExt.ToRadConstF * Fov, Aspect, Near, Far); }

        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector2 LookRot = Vector2.Zero;
        public float Fov = 90;
        public float Aspect = 1;
        public float Near = .1f;
        public float Far = 100;
        public float Speed = 1;

        public Matrix4x4 GetViewMatrix() => Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation)) * Matrix4x4.CreateTranslation(-Position);
        
        public void Update()
        {
            var offset = Vector3.Zero;
            if (Input.GetKey(Key.A))
                offset.X -= 1;
            if (Input.GetKey(Key.D))
                offset.X += 1;
            if (Input.GetKey(Key.W))
                offset.Z -= 1;
            if (Input.GetKey(Key.S))
                offset.Z += 1;
            if (Input.GetKey(Key.Q))
                offset.Y -= 1;
            if (Input.GetKey(Key.E))
                offset.Y += 1;

            offset = Vector3.Transform(offset, Rotation);

            if (Input.GetKey(Key.LeftControl))
                offset *= .5f;
            if (Input.GetKey(Key.LeftShift))
                offset *= 2;
            Position += offset * Time.DeltaF * Speed;

            if (Input.GetMouse(Button.Left))
            {
                var rotateOffset = Input.GetMousePositionDelta() / 256;
                LookRot += rotateOffset;
                LookRot.Y = Math.Clamp(LookRot.Y, -MathF.PI / 2, MathF.PI / 2);
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, LookRot.X) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, LookRot.Y);
            }

            Speed += Input.GetMouseScroll().Y;
            Speed = MathF.Max(0, Speed);
        }

        public void Resize(float width, float height)
        {
            Aspect = width / height;
        }
    }
}
