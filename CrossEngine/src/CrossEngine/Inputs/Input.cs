using System;

using System.Numerics;
using System.Linq;
using System.Collections.Generic;

using CrossEngine.Events;
using CrossEngine.Display;
using Silk.NET.Input;

namespace CrossEngine.Inputs
{
    public static class Input
    {
        static readonly HashSet<Key> keys = new();
        static readonly HashSet<Key> keysDown = new();
        static readonly HashSet<Key> keysUp = new();

        static readonly HashSet<Mouse> buttons = new();
        static readonly HashSet<Mouse> buttonsDown = new();
        static readonly HashSet<Mouse> buttonsUp = new();

        static Vector2 mousePosition;
        static Vector2 mouseDelta;
        static Vector2 mouseLastPosition;

        static internal Window window;

        public static bool Enabled = true;

        static internal void Update()
        {
            mouseDelta = Vector2.Zero;
            mouseLastPosition = mousePosition;

            keysDown.Clear();
            keysUp.Clear();

            buttonsDown.Clear();
            buttonsUp.Clear();
        }

        static public void ForceReset()
        {
            mouseDelta = Vector2.Zero;

            keys.Clear();
            keysDown.Clear();
            keysUp.Clear();

            buttons.Clear();
            buttonsDown.Clear();
            buttonsUp.Clear();
        }

        #region Events
        internal static void OnEvent(Event e)
        {
            if (!Enabled) return;

            new EventDispatcher(e)
                .Dispatch<KeyPressedEvent>(OnKeyPressed)
                .Dispatch<KeyReleasedEvent>(OnKeyReleased)
                .Dispatch<MousePressedEvent>(OnMousePressed)
                .Dispatch<MouseReleasedEvent>(OnMouseReleased)
                .Dispatch<MouseScrolledEvent>(OnMouseScrolled)
                .Dispatch<MouseMovedEvent>(OnMouseMoved);
        }

        static void OnKeyPressed(KeyPressedEvent e)
        {
            keys.Add(e.KeyCode);
            if (!e.Repeated) keysDown.Add(e.KeyCode);
        }
        static void OnKeyReleased(KeyReleasedEvent e)
        {
            keys.Remove(e.KeyCode);
            keysUp.Add(e.KeyCode);
        }
        static void OnMousePressed(MousePressedEvent e)
        {
            buttons.Add(e.ButtonCode);
            buttonsDown.Add(e.ButtonCode);
        }
        static void OnMouseReleased(MouseReleasedEvent e)
        {
            buttons.Remove(e.ButtonCode);
            buttonsUp.Add(e.ButtonCode);
        }
        static void OnMouseScrolled(MouseScrolledEvent e)
        {
            
        }
        static void OnMouseMoved(MouseMovedEvent e)
        {
            mousePosition = new Vector2(e.X, e.Y);
            mouseDelta = mousePosition - mouseLastPosition;
        }
        #endregion

        #region Get
        #region Key
        public static bool GetKey(Key key)
        {
            return keys.Contains(key);
        }
        public static bool GetKeyDown(Key key)
        {
            return keysDown.Contains(key);
        }
        public static bool GetKeyUp(Key key)
        {
            return keysUp.Contains(key);
        }
        #endregion

        #region Mouse
        public static bool GetMouse(Mouse button)
        {
            return buttons.Contains(button);
        }
        public static bool GetMouseDown(Mouse button)
        {
            return buttonsDown.Contains(button);
        }
        public static bool GetMouseUp(Mouse button)
        {
            return buttonsUp.Contains(button);
        }
        #endregion
        #endregion

        #region Is
        public static bool IsKeyPressed(Key key)
        {

            return window.IsKeyPressed(key);
            //InputState state = Glfw.GetKey(Application.Instance.Window.Handle, (Keys)key);
            //return state == InputState.Press || state == InputState.Repeat;
        }
        public static bool IsMousePressed(Mouse button)
        {
            return window.IsMousePressed(button);
            //InputState state = Glfw.GetMouseButton(Application.Instance.Window.Handle, (MouseButton)button);
            //return state == InputState.Press;
        }
        #endregion

        public static Vector2 GetMousePosition()
        {
            return mousePosition;
        }
        public static Vector2 GetMouseScroll() => throw new NotImplementedException();
        public static Vector2 GetMouseDelta() => throw new NotImplementedException();

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
