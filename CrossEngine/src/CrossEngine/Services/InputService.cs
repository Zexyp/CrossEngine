using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Events;
using CrossEngine.Inputs;

namespace CrossEngine.Services
{
    internal class InputService : UpdatedService
    {
        public event OnEventFunction OnEvent;
        Queue<Event> _events = new Queue<Event>();

        public override void OnStart()
        {
            var rs = Manager.GetService<RenderService>();
            rs.Execute(() =>
            {
                rs.Window.OnEvent += HandleEvent;
            });
        }

        public override void OnDestroy()
        {
            var rs = Manager.GetService<RenderService>();
            rs.Execute(() =>
            {
                rs.Window.OnEvent -= HandleEvent;
            });
        }

        public override void OnUpdate()
        {
            Input.Update();
            while (_events.TryDequeue(out var e))
            {
                new EventDispatcher(e)
                    .Dispatch((e) => OnEvent?.Invoke(e))
                    .Dispatch(Input.OnEvent);
            }
        }

        private void HandleEvent(Event e)
        {
            _events.Enqueue(e);
        }
    }
}
