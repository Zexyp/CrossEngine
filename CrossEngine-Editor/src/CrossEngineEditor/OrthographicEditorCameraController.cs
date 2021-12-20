using System;

using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Utils;
using CrossEngine.Rendering.Cameras;

namespace CrossEngineEditor
{
    public class OrthographicEditorCameraController
    {
        public EditorCamera Camera;

        float aspectRatio = 1.0f;
        Vector2 viewportSize;

        //float pitch = 0.0f, yaw = 0.0f, roll = 0.0f;

        float zoomLevel = 1.0f;

        float cameraTranslationSpeed = 5.0f;

        public Vector3 GetUpDirection()
        {
            return Vector3.Transform(Vector3.UnitY, Camera.Rotation);
        }
        public Vector3 GetRightDirection()
        {
            return Vector3.Transform(Vector3.UnitX, Camera.Rotation);
        }
        public Vector3 GetForwardDirection()
        {
            return Vector3.Transform(Vector3.UnitZ, Camera.Rotation);
        }

        //public Quaternion GetOrientation()
        //{
        //    // mby negate roll
        //    return QuaternionExtension.CreateFromXYZRotation(new Vector3(-pitch, -yaw, roll));
        //}

        public OrthographicEditorCameraController(EditorCamera camera)
        {
            this.Camera = camera;
        }

        public void Move(Vector2 delta)
        {
            Vector2 move = new Vector2(delta.X / viewportSize.Y, delta.Y / viewportSize.Y) * zoomLevel * 2;
            Camera.Position += new Vector3(move, 0.0f);
        }

        public void Zoom(float delta)
        {
            zoomLevel -= delta * 0.25f;
            zoomLevel = Math.Max(zoomLevel, 0.25f);

            UpdateProjection();
        }

        public void Resize(Vector2 size)
        {
            viewportSize = size;

            UpdateProjection();
        }

        void UpdateProjection()
        {
            aspectRatio = viewportSize.X / viewportSize.Y;
            Camera.ProjectionMatrix = Matrix4x4Extension.Ortho(-aspectRatio * zoomLevel, aspectRatio * zoomLevel, -zoomLevel, zoomLevel, -1, 1);
        }

        //public void OnUpdateX(float timestep)
        //{
        //    Vector2 mouse = new Vector2(Input.GetMouseX(), Input.GetMouseY());
        //    Vector2 delta = (mouse - initialMousePosition) * 0.003f;
        //    initialMousePosition = mouse;
        //
        //    if (Input.IsMouseButtonPressed(Mouse.Middle))
        //        MousePan(delta);
        //
        //    float moveSpeedMultiplier = Input.GetKey(Key.LeftShift) ? 2.0f : (Input.GetKey(Key.LeftControl) ? 0.5f : 1.0f);
        //    moveSpeedMultiplier *= timestep * cameraTranslationSpeed;
        //
        //    if (Input.GetKey(Key.ArrowRight))
        //    {
        //        Camera.Position += moveSpeedMultiplier * GetRightDirection();
        //    }
        //    if (Input.GetKey(Key.ArrowLeft))
        //    {
        //        Camera.Position -= moveSpeedMultiplier * GetRightDirection();
        //    }
        //    
        //    if (Input.GetKey(Key.ArrowUp))
        //    {
        //        Camera.Position += moveSpeedMultiplier * GetUpDirection();
        //    }
        //    if (Input.GetKey(Key.ArrowDown))
        //    {
        //        Camera.Position -= moveSpeedMultiplier * GetUpDirection();
        //    }
        //
        //    cameraTranslationSpeed = zoomLevel;
        //}
        //
        //public void Resize(float width, float height)
        //{
        //    aspectRatio = width / height;
        //    camera.SetProjection(-aspectRatio * zoomLevel, aspectRatio * zoomLevel, -zoomLevel, zoomLevel);
        //}
        //
        //
        //void OnWindowResize(WindowResizeEvent e)
        //{
        //    if (AutoResize)
        //        Resize(e.Width, e.Height);
        //}
    }
}
