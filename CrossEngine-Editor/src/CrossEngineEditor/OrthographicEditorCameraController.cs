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

        //float pitch = 0.0f, yaw = 0.0f, roll = 0.0f;

        float zoomLevel = 1.0f;

        float cameraTranslationSpeed = 5.0f;

        float viewportWidth, viewportHeight;

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

        Vector2 initialMousePosition = Vector2.Zero;

        public OrthographicEditorCameraController(EditorCamera camera)
        {
            this.Camera = camera;
        }

        public void OnUpdate(float timestep)
        {
            Vector2 mouse = new Vector2(Input.GetMouseX(), Input.GetMouseY());
            Vector2 delta = (mouse - initialMousePosition) * 0.003f;
            initialMousePosition = mouse;

            if (Input.IsMouseButtonPressed(Mouse.Middle))
                MousePan(delta);

            float moveSpeedMultiplier = Input.GetKey(Key.LeftShift) ? 2.0f : (Input.GetKey(Key.LeftControl) ? 0.5f : 1.0f);
            moveSpeedMultiplier *= timestep * cameraTranslationSpeed;

            if (Input.GetKey(Key.ArrowRight))
            {
                Camera.Position += moveSpeedMultiplier * GetRightDirection();
            }
            if (Input.GetKey(Key.ArrowLeft))
            {
                Camera.Position -= moveSpeedMultiplier * GetRightDirection();
            }
            
            if (Input.GetKey(Key.ArrowUp))
            {
                Camera.Position += moveSpeedMultiplier * GetUpDirection();
            }
            if (Input.GetKey(Key.ArrowDown))
            {
                Camera.Position -= moveSpeedMultiplier * GetUpDirection();
            }

            cameraTranslationSpeed = zoomLevel;
        }

        public void OnEvent(Event e)
        {
            EventDispatcher dispatcher = new EventDispatcher(e);
            dispatcher.Dispatch<MouseScrolledEvent>(OnMouseScrolled);
            //dispatcher.Dispatch<WindowResizeEvent>((e) => { SetViewportSize(e.Width, e.Height); });
        }

        public void SetViewportSize(float width, float height)
        { 
            viewportWidth = width;
            viewportHeight = height;
            UpdateProjection();
        }

        void OnMouseScrolled(MouseScrolledEvent e)
        {
            zoomLevel -= e.Y * 0.25f;
            zoomLevel = Math.Max(zoomLevel, 0.25f);
            UpdateProjection();
        }

        void UpdateProjection()
        {
            aspectRatio = viewportWidth / viewportHeight;
            Camera.ProjectionMatrix = Matrix4x4Extension.Ortho(-aspectRatio * zoomLevel, aspectRatio * zoomLevel, -zoomLevel, zoomLevel, -1, 1);
        }

        void MousePan(Vector2 delta)
        {
            (float xSpeed, float ySpeed) = PanSpeed();
            Camera.Position += -GetRightDirection() * delta.X * xSpeed;
            Camera.Position += GetUpDirection() * delta.Y * ySpeed;
        }

        (float, float) PanSpeed()
        {
            return (zoomLevel / (Math.Max(viewportWidth, viewportHeight) / 1000), zoomLevel / (Math.Max(viewportWidth, viewportHeight) / 1000));

            //float x = Math.Min(viewportWidth / 1000.0f, 2.4f); // max = 2.4f
            //float xFactor = 0.0366f * (x * x) - 0.1778f * x + 0.3021f;
            //
            //float y = Math.Min(viewportHeight / 1000.0f, 2.4f); // max = 2.4f
            //float yFactor = 0.0366f * (y * y) - 0.1778f * y + 0.3021f;
            //
            //return (xFactor, yFactor);
        }

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
