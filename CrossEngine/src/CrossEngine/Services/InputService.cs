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
    internal class InputService : UpdatedService
    {
        public event OnEventFunction Event;
        Queue<Event> _events = new Queue<Event>();

        public override void OnStart()
        {
            Manager.GetService<WindowService>().Event += HandleEvent;
            
        }

        public override void OnDestroy()
        {
            Manager.GetService<WindowService>().Event -= HandleEvent;
        }

        public override void OnUpdate()
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
