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

        public OverlayService()
        {
            
        }

        public OverlayService(params Overlay[] overlays)
        {
            for (int i = 0; i < overlays.Length; i++)
            {
                AddOverlay(overlays[i]);
            }
        }

        public override void OnAttach()
        {
            var rs = Manager.GetService<RenderService>();
            rs.MainSurface.AfterUpdate += OnRender;
            rs.MainSurface.Resize += OnResize;
            rs.Execute(() => OnResize(rs.MainSurface, rs.MainSurface.Size.X, rs.MainSurface.Size.Y));
        }

        public override void OnDetach()
        {
            var rs = Manager.GetService<RenderService>();
            rs.MainSurface.AfterUpdate -= OnRender;
            rs.MainSurface.Resize -= OnResize;

        }

        private void OnResize(ISurface surface, float width, float height)
        {
            _lastWidth = width;
            _lastHeight = height;
            for (int i = 0; i < _overlays.Count; i++)
            {
                _overlays[i].Resize(width, height);
            }
        }

        void OnRender(ISurface surface)
        {
            surface.Context.Api.SetViewport(0, 0, (uint)_lastWidth, (uint)_lastHeight);
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
