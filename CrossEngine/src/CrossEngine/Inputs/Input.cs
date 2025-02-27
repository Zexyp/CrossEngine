using System;

using System.Numerics;
using System.Linq;
using System.Collections.Generic;

using CrossEngine.Events;
using CrossEngine.Display;
using CrossEngine.Logging;

namespace CrossEngine.Inputs
{
    public static class Input
    {
        private static Logger _log = new Logger("input");

        [ThreadStatic]
        static internal Mouse mouse;
        [ThreadStatic]
        static internal Keyboard keyboard;

        static public void ForceReset()
        {
            _log.Warn("reset is not fully implemented");
            throw new NotImplementedException();

            //mouseDelta = Vector2.Zero;
            //
            //keys.Clear();
            //keysDown.Clear();
            //keysUp.Clear();
            //
            //buttons.Clear();
            //buttonsDown.Clear();
            //buttonsUp.Clear();
        }

        #region Get
        public static bool GetKey(Key key) => keyboard.Get(key);
        public static bool GetKeyDown(Key key) => keyboard.GetDown(key);
        public static bool GetKeyUp(Key key) => keyboard.GetUp(key);

        public static bool GetMouse(Button button) => mouse.Get(button);
        public static bool GetMouseDown(Button button) => mouse.GetDown(button);
        public static bool GetMouseUp(Button button) => mouse.GetUp(button);
        #endregion

        #region Is
        //public static bool IsKeyPressed(Key key)
        //{
        //    return window.IsKeyPressed(key);
        //}
        //public static bool IsMousePressed(Button button)
        //{
        //    return window.IsMousePressed(button);
        //}
        #endregion

        public static Vector2 GetMousePosition() => mouse.CursorPosition;
        public static Vector2 GetMouseScroll() => mouse.ScrollDelta;
        public static Vector2 GetMousePositionDelta() => mouse.CursorDelta;

        //public static float GetProjectedMouseX(Rendering.Cameras.Camera camera)
        //{
        //    float currentX = (mousePosition.X / (float)Application.Instance.Window.Width) * 2.0f - 1.0f;
        //    return Vector3.Transform(new Vector3(currentX, 0, 0), Matrix4x4Extension.Invert(camera.ViewProjectionMatrix)).X;
        //}
        //public static float GetProjectedMouseY(Rendering.Cameras.Camera camera)
        //{
        //    float currentY = (mousePosition.Y / (float)Application.Instance.Window.Height) * 2.0f - 1.0f;
        //    return Vector3.Transform(new Vector3(0, -currentY, 0), Matrix4x4Extension.Invert(camera.ViewProjectionMatrix)).Y;
        //}
        //public static Vector2 GetProjectedMouse(Rendering.Cameras.Camera camera, Vector2 mouse, Vector2 window, Vector3 position, Quaternion rotation)
        //{
        //    float currentX = (mouse.X / window.X) * 2.0f - 1.0f;
        //    float currentY = (mouse.Y / window.Y) * 2.0f - 1.0f;
        //    Vector3 projected = Vector3.Transform(new Vector3(currentX, -currentY, 0), Matrix4x4Extension.Invert(Matrix4x4.CreateTranslation(-position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(rotation)) * camera.ProjectionMatrix));
        //    return new Vector2(projected.X, projected.Y);
        //}
    }
}
