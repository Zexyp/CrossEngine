using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CrossEngine.Display;
using CrossEngine.Rendering;
using CrossEngine.Profiling;
using CrossEngine.Services;
using CrossEngine.Events;
using CrossEngine.Utils;
using CrossEngine.Platform;
using CrossEngine.Assets.Loaders;

namespace CrossEngine.Services
{
    public class RenderService : Service, IScheduledService
    {
        public RendererApi RendererApi { get; private set; }
        public event Action<RenderService> Frame;
        public event Action<RenderService> BeforeFrame;
        public event Action<RenderService> AfterFrame;
        public bool IgnoreRefresh { get; set; } = false;

        readonly SingleThreadedTaskScheduler _scheduler = new SingleThreadedTaskScheduler();
        bool _running = false;
        GraphicsContext _context;
        GraphicsApi _api;

        public RenderService()
        {
            _api = PlatformHelper.GetGraphicsApi();
        }

        public override void OnAttach()
        {
            var ws = Manager.GetService<WindowService>();
            ws.Execute(Setup);
        }

        public override void OnDetach()
        {
            var ws = Manager.GetService<WindowService>();
            ws.Execute(Destroy);
        }

        public Task Execute(Action action) => _scheduler.Schedule(action);
        public TaskScheduler GetScheduler() => _scheduler;

        private void Setup()
        {
            var ws = Manager.GetService<WindowService>();
            ws.WindowEvent += OnWindowEvent;
            ws.WindowUpdate += OnWindowUpdate;
            
            _context = ws.MainWindow.Context;
            _context.Init();

            RendererApi.Init();
            RendererApi.SetClearColor(new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            RendererApi.SetViewport(0, 0, ws.MainWindow.Width, ws.MainWindow.Height);

            Prepare();
        }

        private void Destroy()
        {
            Shutdown();

            var ws = Manager.GetService<WindowService>();
            ws.WindowEvent -= OnWindowEvent;
            ws.WindowUpdate -= OnWindowUpdate;
            
            ws.MainWindow.Context.Dispose();

            RendererApi.Dispose();
            RendererApi = null;
            
            _context.Dispose();
            _context = null;
        }

        private void OnWindowEvent(Window w, Event e)
        {
            // this is supposed to fix state when resizing window
            // it's unfortunate that when just holding the window nothing happens
            // also used when window dictates it's own redrawing
            if (!IgnoreRefresh && (e is WindowRefreshEvent))
                DrawPresent(w.Context);
            if (e is WindowResizeEvent wre)
                RendererApi.SetViewport(0, 0, wre.Width, wre.Height);
        }

        private void OnWindowUpdate(Window w)
        {
            DrawPresent(w.Context);
        }

        private void DrawPresent(GraphicsContext context)
        {
            Profiler.BeginScope("Render");
            
            context.MakeCurrent();

            _scheduler.RunOnCurrentThread();

            BeforeFrame?.Invoke(this);
            Frame?.Invoke(this);
            AfterFrame?.Invoke(this);

            Profiler.EndScope();

            Profiler.BeginScope("Swap");

            context.SwapBuffers();

            Profiler.EndScope();
        }

        private void Prepare()
        {
            Renderer2D.Init(RendererApi);
            LineRenderer.Init(RendererApi);
            
            TextRendererUtil.Init();

            _scheduler.RunOnCurrentThread();
        }

        private void Shutdown()
        {
            _scheduler.RunOnCurrentThread();

            Renderer2D.Shutdown();
            LineRenderer.Shutdown();
            
            TextRendererUtil.Shutdown();
        }

        public override void OnStart()
        {
            RendererApi = RendererApi.Create(_api);
            TextureLoader.InternalInit();
        }

        public override void OnDestroy()
        {
            
        }
    }
}
