using GLFW;
using System;

using System.Numerics;
using System.Linq;

using CrossEngine.Events;
using CrossEngine.Utils;

namespace CrossEngine.Inputs
{
    public static class Input
    {
        static bool[] keys = new bool[(int)Enum.GetValues(typeof(Key)).Cast<Key>().Max()];
        static bool[] keysDown = new bool[(int)Enum.GetValues(typeof(Key)).Cast<Key>().Max()];
        static bool[] keysUp = new bool[(int)Enum.GetValues(typeof(Key)).Cast<Key>().Max()];

        static bool[] buttons = new bool[(int)Enum.GetValues(typeof(Mouse)).Cast<Mouse>().Max()];
        static bool[] buttonsDown = new bool[(int)Enum.GetValues(typeof(Mouse)).Cast<Mouse>().Max()];
        static bool[] buttonsUp = new bool[(int)Enum.GetValues(typeof(Mouse)).Cast<Mouse>().Max()];

        static Vector2 mousePosition;
        static Vector2 mouseDelta;
        static Vector2 mouseLastPosition;

        public static bool Enabled = true;

        // meh
        //static Input()
        //{
        //    GlobalEventDispatcher.Register<KeyPressedEvent>(OnKeyPressed);
        //    GlobalEventDispatcher.Register<KeyReleasedEvent>(OnKeyReleased);
        //    GlobalEventDispatcher.Register<MouseButtonPressedEvent>(OnMouseButtonPressed);
        //    GlobalEventDispatcher.Register<MouseButtonReleasedEvent>(OnMouseButtonReleased);
        //    GlobalEventDispatcher.Register<MouseScrolledEvent>(OnMouseScrolled);
        //    GlobalEventDispatcher.Register<MouseMovedEvent>(OnMouseMoved);
        //}

        static public void Update()
        {
            mouseDelta.X = mouseDelta.Y = 0.0f;
            mouseLastPosition = mousePosition;

            for (int i = 0; i < keys.Length; i++)
            {
                keysDown[i] = false;
                keysUp[i] = false;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                buttonsDown[i] = false;
                buttonsUp[i] = false;
            }
        }

        static public void ForceReset()
        {
            mouseDelta.X = mouseDelta.Y = 0.0f;

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = false;
                keysDown[i] = false;
                keysUp[i] = false;
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = false;
                buttonsDown[i] = false;
                buttonsUp[i] = false;
            }
        }

        #region Events
        internal static void OnEvent(Event e)
        {
            if (!Enabled) return;

            EventDispatcher dispatcher = new EventDispatcher(e);
            dispatcher.Dispatch<KeyPressedEvent>(OnKeyPressed);
            dispatcher.Dispatch<KeyReleasedEvent>(OnKeyReleased);
            dispatcher.Dispatch<MouseButtonPressedEvent>(OnMouseButtonPressed);
            dispatcher.Dispatch<MouseButtonReleasedEvent>(OnMouseButtonReleased);
            dispatcher.Dispatch<MouseScrolledEvent>(OnMouseScrolled);
            dispatcher.Dispatch<MouseMovedEvent>(OnMouseMoved);
        }

        static void OnKeyPressed(KeyPressedEvent e)
        {
            if ((int)e.KeyCode > 0)
            {
                keys[(int)e.KeyCode] = true;
                keysDown[(int)e.KeyCode] = e.RepeatCount == 0;
            }
        }
        static void OnKeyReleased(KeyReleasedEvent e)
        {
            if ((int)e.KeyCode > 0)
            {
                keys[(int)e.KeyCode] = false;
                keysUp[(int)e.KeyCode] = true;
            }
        }
        static void OnMouseButtonPressed(MouseButtonPressedEvent e)
        {
            buttons[(int)e.ButtonCode] = true;
            buttonsDown[(int)e.ButtonCode] = true;
        }
        static void OnMouseButtonReleased(MouseButtonReleasedEvent e)
        {
            buttons[(int)e.ButtonCode] = false;
            buttonsUp[(int)e.ButtonCode] = true;
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
        public static bool GetKey(Key key)
        {
            return keys[(int)key];
        }
        public static bool GetKeyDown(Key key)
        {
            return keysDown[(int)key];
        }
        public static bool GetKeyUp(Key key)
        {
            return keysUp[(int)key];
        }

        public static bool GetMouseButton(Mouse button)
        {
            return buttons[(int)button];
        }
        public static bool GetMouseButtonDown(Mouse button)
        {
            return buttonsDown[(int)button];
        }
        public static bool GetMouseButtonUp(Mouse button)
        {
            return buttonsUp[(int)button];
        }
        #endregion

        #region Is
        public static bool IsKeyPressed(Key key)
        {
            InputState state = Glfw.GetKey(Application.Instance.Window.Handle, (Keys)key);
            return state == InputState.Press || state == InputState.Repeat;
        }
        public static bool IsMouseButtonPressed(Mouse button)
        {
            InputState state = Glfw.GetMouseButton(Application.Instance.Window.Handle, (MouseButton)button);
            return state == InputState.Press;
        }
        #endregion

        //public static Vector2 GetMousePosition()
        //{
        //    Glfw.GetCursorPosition(Application.Instance.Window.Handle, out double x, out double y);
        //    return new Vector2((float)x, (float)y);
        //}

        public static Vector2 GetMousePosition()
        {
            return mousePosition;
        }
        public static float GetMouseX()
        {
            return mousePosition.X;
        }
        public static float GetMouseY()
        {
            return mousePosition.Y;
        }

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
        public static Vector2 GetProjectedMouse(Rendering.Cameras.Camera camera, Vector2 mouse, Vector2 window, Vector3 position, Quaternion rotation)
        {
            float currentX = (mouse.X / window.X) * 2.0f - 1.0f;
            float currentY = (mouse.Y / window.Y) * 2.0f - 1.0f;
            Vector3 projected = Vector3.Transform(new Vector3(currentX, -currentY, 0), Matrix4x4Extension.Invert(Matrix4x4.CreateTranslation(-position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(rotation)) * camera.ProjectionMatrix));
            return new Vector2(projected.X, projected.Y);
        }
    }
}
