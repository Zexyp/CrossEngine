using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Inputs
{
    public class Mouse
    {
        Vector2 mousePosition;
        Vector2 mouseDelta;
        Vector2 mouseLastPosition;
        Vector2 scrollDelta;

        readonly HashSet<Button> buttons = new();
        readonly HashSet<Button> buttonsDown = new();
        readonly HashSet<Button> buttonsUp = new();

        public Vector2 CursorPosition { get => mousePosition; }
        public Vector2 CursorDelta { get => mouseDelta; }
        public Vector2 ScrollDelta { get => scrollDelta; }

        public void Update()
        {
            mouseDelta = Vector2.Zero;
            mouseLastPosition = mousePosition;

            scrollDelta = Vector2.Zero;

            buttonsDown.Clear();
            buttonsUp.Clear();
        }

        public void Add(Button button)
        {
            if (!buttons.Contains(button))
                buttonsDown.Add(button);
            buttons.Add(button);
        }

        public void Remove(Button button)
        {
            buttons.Remove(button);
            buttonsDown.Remove(button);
            buttonsUp.Add(button);
        }

        public void Position(Vector2 pos)
        {
            mousePosition = pos;
            mouseDelta = mousePosition - mouseLastPosition;
        }

        public void Scroll(Vector2 scroll)
        {
            scrollDelta += scroll;
        }

        public bool Get(Button button) => buttons.Contains(button);
        public bool GetUp(Button button) => buttonsUp.Contains(button);
        public bool GetDown(Button button) => buttonsDown.Contains(button);
    }
}
