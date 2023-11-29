using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Profiling;

namespace CrossEngine.Services
{
    public class InputService : Service, IUpdatedService
    {
        public event OnEventFunction Event;
        Queue<Event> _events = new Queue<Event>();

        public override void OnStart()
        {
            var ws = Manager.GetService<WindowService>();
            ws.Event += HandleEvent;
            Input.window = ws.Window;
        }

        public override void OnDestroy()
        {
            var ws = Manager.GetService<WindowService>();
            Input.window = null;
            ws.Event -= HandleEvent;
        }

        public void OnUpdate()
        {
            Input.Update();
            while (_events.TryDequeue(out var e))
            {
                new EventDispatcher(e)
                    .Dispatch((e) => Event?.Invoke(e))
                    .Dispatch(Input.OnEvent);
            }
        }

        private void HandleEvent(Event e)
        {
            _events.Enqueue(e);
        }
    }
}
