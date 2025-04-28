using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Display;
using CrossEngine.Events;
using CrossEngine.Services;
using CrossEngine.Profiling;

namespace CrossEngine.Inputs
{
    public class InputService : Service, IUpdatedService
    {
        readonly Queue<Event> _events = new Queue<Event>();

        readonly Keyboard keyboard = new();
        readonly Mouse mouse = new();

        public override void OnAttach()
        {
            var ws = Manager.GetService<WindowService>();
            Input.keyboard = keyboard;
            Input.mouse = mouse;
            ws.WindowEvent += HandleEvent;
        }

        public override void OnDetach()
        {
            var ws = Manager.GetService<WindowService>();
            ws.WindowEvent -= HandleEvent;
            Input.keyboard = null;
            Input.mouse = null;
        }

        public void OnUpdate()
        {
            Profiler.BeginScope();
            
            keyboard.Update();
            mouse.Update();
            while (_events.TryDequeue(out var e))
            {
                new EventDispatcher(e)
                .Dispatch<KeyPressedEvent>(OnKeyPressed)
                .Dispatch<KeyReleasedEvent>(OnKeyReleased)
                .Dispatch<MousePressedEvent>(OnMousePressed)
                .Dispatch<MouseReleasedEvent>(OnMouseReleased)
                .Dispatch<MouseScrolledEvent>(OnMouseScrolled)
                .Dispatch<MouseMovedEvent>(OnMouseMoved);
            }

            Profiler.EndScope();
        }

        private void HandleEvent(Window ws, Event e)
        {
            _events.Enqueue(e);
        }

        public override void OnStart()
        {
            
        }

        public override void OnDestroy()
        {
            
        }

        void OnKeyPressed(KeyPressedEvent e)
        {
            keyboard.Add(e.KeyCode);
        }
        void OnKeyReleased(KeyReleasedEvent e)
        {
            keyboard.Remove(e.KeyCode);
        }
        void OnMousePressed(MousePressedEvent e)
        {
            mouse.Add(e.ButtonCode);
        }
        void OnMouseReleased(MouseReleasedEvent e)
        {
            mouse.Remove(e.ButtonCode);
        }
        void OnMouseScrolled(MouseScrolledEvent e)
        {
            mouse.Scroll(new(e.X, e.Y));
        }
        void OnMouseMoved(MouseMovedEvent e)
        {
            mouse.Position(new(e.X, e.Y));
        }
    }
}
