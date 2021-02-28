using System;
using GLFW;

using System.Collections.Generic;
using System.Numerics;

using CrossEngine.Rendering.Display;

namespace CrossEngine.InputSystem
{
    public static unsafe class Input
    {
        public static void Init()
        {
            InitCallbacks();
        }

        private static List<int> currentKeys = new List<int> { };
        private static List<int> downKeys = new List<int> { };

        private static List<int> currentButtons = new List<int> { };
        private static List<int> downButtons = new List<int> { };

        public static void Update()
        {
            // keyboard
            downKeys.Clear();

            //downKeys = new List<int>(currentKeys); // maybe mem leak, but passed the test :D
            foreach (int item in currentKeys) // more stable speed
            {
                downKeys.Add(item);
            }

            currentKeys.Clear();

            // mouse
            downButtons.Clear();

            foreach (int item in currentButtons)
            {
                downButtons.Add(item);
            }

            currentButtons.Clear();
        }

        public static void InitCallbacks()
        {
            // k
            DisplayManager.WindowCallbackHolder.SetKeyCallback(KeyCallback);
            // m
            DisplayManager.WindowCallbackHolder.SetCursorPositionCallback(MousePositionCallback);
            DisplayManager.WindowCallbackHolder.SetMouseButtonCallback(MouseButtonCallback);
            DisplayManager.WindowCallbackHolder.SetScrollCallback(MouseScrollCallback);
        }

        #region Keyboard
        private const int k_nrKeys = 348;
        private static bool[] k_keysPressed = new bool[k_nrKeys]; // not the most efficient since the keys enum starts at 32

        public static void KeyCallback(Window window, GLFW.Keys key, int scanCode, InputState action, ModifierKeys mods)
        {
            if (key < 0)
                return;
            if (action == InputState.Press)
            {
                k_keysPressed[(int)key] = true;
                currentKeys.Add((int)key);
            }
            else if (action == InputState.Release)
                k_keysPressed[(int)key] = false;
        }

        #region Getters
        public static bool GetKey(InputSystem.Key key) 
        {
            return k_keysPressed[(int)key]; 
        }
        public static bool GetKeyDown(InputSystem.Key key)
        {
            return downKeys.Contains((int)key);
        }
        #endregion
        #endregion

        #region Mouse
        private static double m_scrollX = 0.0;
        private static double m_scrollY = 0.0;
        private static double m_posX = 0.0;
        private static double m_posY = 0.0;
        private static double m_lastX = 0.0;
        private static double m_lastY = 0.0;

        private static bool[] m_mouseButtonsPressed = new bool[3];

        static bool m_isDragging;

        public static void MousePositionCallback(Window window, double newX, double newY)
        {
            m_lastX = m_posX;
            m_lastY = m_posY;
            m_posX = newX;
            m_posY = -newY; // is negative cuz opengl

            m_isDragging = m_mouseButtonsPressed[0] || m_mouseButtonsPressed[1] || m_mouseButtonsPressed[2];
        }

        public static void MouseButtonCallback(Window window, GLFW.MouseButton button, InputState action, ModifierKeys mods)
        {
            if (action == InputState.Press)
            {
                if ((int)button < m_mouseButtonsPressed.Length)
                    m_mouseButtonsPressed[(int)button] = true; // might cause problems
                currentButtons.Add((int)button);
            }
            else if (action == InputState.Release)
            {
                if ((int)button < m_mouseButtonsPressed.Length)
                {
                    m_mouseButtonsPressed[(int)button] = false;
                    m_isDragging = false;
                }
            }
        }

        public static void MouseScrollCallback(Window window, double offsetX, double offsetY)
        {
            m_scrollX = offsetX;
            m_scrollY = offsetY;

            // event
            Events.SendOnMouseScrolledEvent((float)offsetX, (float)offsetY);
        }

        public static void EndFrame()
        {
            m_scrollX = 0;
            m_scrollY = 0;
            m_lastX = m_posX;
            m_lastY = m_posY;
        }

        #region Getters
        public static float GetMouseX() { return (float)m_posX; }
        public static float GetMouseY() { return (float)m_posY; }
        public static Vector2 GetMousePos() { return new Vector2((float)m_posX, (float)m_posY); }
        public static float GetMouseDX() { return (float)(m_lastX - m_posX); }
        public static float GetMouseDY() { return (float)(m_lastY - m_posY); }
        public static float GetScrollX() { return (float)m_scrollX; }
        public static float GetScrollY() { return (float)m_scrollY; }
        public static bool MouseIsDragging() { return m_isDragging; }
        public static bool GetMouseButton(InputSystem.MouseButton button)
        {
            if ((int)button < m_mouseButtonsPressed.Length)
                return m_mouseButtonsPressed[(int)button];

            return false;
        }
        public static bool GetButtonDown(InputSystem.MouseButton button)
        {
            return downButtons.Contains((int)button);
        }
        #endregion
        #endregion

        //public class KeyListener
        //{
        //    
        //}
        //
        //public class MouseListener
        //{
        //    
        //}

        /*
        private static readonly int[] keyCodes = (int[])Enum.GetValues(typeof(InputSystem.Keys));

        // this is not terrible
        private static List<int> currentKeys = new List<int> { };
        private static List<int> downKeys = new List<int> { };

        public static void Update()
        {
            downKeys.Clear();
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (!currentKeys.Contains(keyCodes[i]))
                {
                    if (GetKey((InputSystem.Keys)keyCodes[i]))
                        downKeys.Add(keyCodes[i]);
                }
            }
            currentKeys.Clear();
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (GetKey((InputSystem.Keys)keyCodes[i]))
                    currentKeys.Add(keyCodes[i]);
            }
        }

        public static bool GetKey(InputSystem.Keys keycode)
        {
            InputState state = Glfw.GetKey(DisplayManager.Window, (GLFW.Keys)keycode);
            return state == InputState.Press || state == InputState.Repeat;
        }

        public static bool GetKeyDown(InputSystem.Keys keycode)
        {
            return downKeys.Contains((int)keycode);
        }

        public static bool GetMouseButton(InputSystem.MouseButton button)
        {
            InputState state = Glfw.GetMouseButton(DisplayManager.Window, (GLFW.MouseButton)button);
            return state == InputState.Press;
        }

        public static (float, float) GetMousePosition()
        {
            double xpos, ypos;
            Glfw.GetCursorPosition(DisplayManager.Window, out xpos, out ypos);
            return ((float)xpos, (float)ypos);
        }

        public static float GetMouseX(int button)
        {
            var (x, y) = GetMousePosition();
            return x;
        }

        public static float GetMouseY(int button)
        {
            var (x, y) = GetMousePosition();
            return y;
        }
        */
    }
}
