using CrossEngine.Display;
using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Debugging
{
    public class OverlayService : Service
    {
        readonly List<Overlay> _overlays = new List<Overlay>();
        private float _lastWidth, _lastHeight;

        public override void OnAttach()
        {
            var rs = Manager.GetService<RenderService>();
            var ws = Manager.GetService<WindowService>();
            rs.Draw += OnRender;
            ws.WindowEvent += OnEvent;
            ws.Execute(() => OnResize(ws.MainWindow.Width, ws.MainWindow.Height));
        }

        public override void OnDetach()
        {
            var rs = Manager.GetService<RenderService>();
            var ws = Manager.GetService<WindowService>();
            rs.Draw -= OnRender;
            ws.WindowEvent -= OnEvent;
        }

        private void OnEvent(Window ws, Event e)
        {
            if (e is WindowResizeEvent wre)
                OnResize(wre.Width, wre.Height);
        }

        private void OnResize(float width, float height)
        {
            _lastWidth = width;
            _lastHeight = height;
            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Resize(width, height);
            }
        }

        void OnRender(RenderService rs)
        {
            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Draw();
            }
        }

        public void AddOverlay(Overlay overlay)
        {
            _overlays.Add(overlay);
            overlay.Resize(_lastWidth, _lastHeight);
        }
        public void RemoveOverlay(Overlay overlay) => _overlays.Remove(overlay);

        public override void OnStart()
        {
            
        }

        public override void OnDestroy()
        {
            
        }
    }
}
