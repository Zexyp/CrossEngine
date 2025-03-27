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
using CrossEngine.Loaders;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Buffers;
using System.Numerics;

namespace CrossEngine.Services
{
    public class RenderService : Service, IScheduledService
    {
        public bool IgnoreRefresh { get; set; } = false;
        public ScreenSurface MainSurface => _surface;

        readonly SingleThreadedTaskScheduler _scheduler = new SingleThreadedTaskScheduler();
        GraphicsContext _context;
        GraphicsApi _api;
        ScreenSurface _surface = new ScreenSurface();

        public RenderService()
        {
            _api = PlatformHelper.GetGraphicsApi();
        }

        public override void OnAttach()
        {
            var ws = Manager.GetService<WindowService>();
            ws.Execute(Setup);

            CallingThreadSetup();
        }

        public override void OnDetach()
        {
            CallingThreadDestroy();

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
            _context.Api = RendererApi.Create(_api);
            _context.Init();

            _context.Api.Init();
            _context.Api.SetClearColor(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            _context.Api.SetViewport(0, 0, ws.MainWindow.Width, ws.MainWindow.Height);
            
            _surface.Context = _context;
            _surface.DoResize(ws.MainWindow.Width, ws.MainWindow.Height);

            Prepare();
        }

        private void Destroy()
        {
            Shutdown();

            var ws = Manager.GetService<WindowService>();
            ws.WindowEvent -= OnWindowEvent;
            ws.WindowUpdate -= OnWindowUpdate;
            
            ws.MainWindow.Context.Dispose();

            _context.Api.Dispose();
            _context.Api = null;
            
            _context.Dispose();
            _context = null;
        }

        private void CallingThreadSetup()
        {
            ShaderPreprocessor.ServiceRequest = action => Execute(action);
            TextureLoader.ServiceRequest = action => Execute(action);
            MeshLoader.ServiceRequest = action => Execute(action);
        }

        private void CallingThreadDestroy()
        {
            MeshLoader.ServiceRequest = null;
            TextureLoader.ServiceRequest = null;
            ShaderPreprocessor.ServiceRequest = null;
        }

        private void OnWindowEvent(Window w, Event e)
        {
            GraphicsContext.Current = w.Context;

            // this is supposed to fix state when resizing window
            // it's unfortunate that when just holding the window nothing happens
            // also used when window dictates it's own redrawing
            if (!IgnoreRefresh && (e is WindowRefreshEvent))
                DrawPresent(w.Context);

            if (e is WindowResizeEvent wre)
            {
                w.Context.Api.SetViewport(0, 0, wre.Width, wre.Height);
                _surface.DoResize(wre.Width, wre.Height);
            }
            
            GraphicsContext.Current = null;
        }

        private void OnWindowUpdate(Window w)
        {
            GraphicsContext.Current = w.Context;
            DrawPresent(w.Context);
            GraphicsContext.Current = null;
        }

        private void DrawPresent(GraphicsContext context)
        {
            Profiler.BeginScope("Render");
            
            context.MakeCurrent();

            _scheduler.RunOnCurrentThread();

            _surface.DoUpdate();

            Profiler.EndScope();

            Profiler.BeginScope("Swap");

            context.SwapBuffers();

            Profiler.EndScope();
        }

        private void Prepare()
        {
            void OnServiceRequest(Action action) => action.Invoke();

            ShaderPreprocessor.ServiceRequest = OnServiceRequest;
            ShaderPreprocessor.Init();
            TextureLoader.ServiceRequest = OnServiceRequest;
            TextureLoader.Init();
            MeshLoader.ServiceRequest = OnServiceRequest;

            Renderer2D.Init(_context.Api);
            LineRenderer.Init(_context.Api);
            TextRendererUtil.Init();

            _scheduler.RunOnCurrentThread();
        }

        private void Shutdown()
        {
            _scheduler.RunOnCurrentThread();

            TextRendererUtil.Shutdown();
            LineRenderer.Shutdown();
            Renderer2D.Shutdown();

            MeshLoader.ServiceRequest = null;
            TextureLoader.Shutdown();
            TextureLoader.ServiceRequest = null;
            ShaderPreprocessor.Shutdown();
            ShaderPreprocessor.ServiceRequest = null;
        }

        private void OnInternalServiceReqest(Action action) => Execute(action);

        public override void OnStart()
        {
            
        }

        public override void OnDestroy()
        {

        }

        public class ScreenSurface : ISurface
        {
            public WeakReference<Framebuffer> Buffer => null;
            public Vector2 Size { get; private set; }
            public GraphicsContext Context { get; set; }

            public event Action<ISurface, float, float> Resize;
            public event Action<ISurface> BeforeUpdate;
            public event Action<ISurface> Update;
            public event Action<ISurface> AfterUpdate;

            public void DoResize(float width, float height)
            {
                Size = new(width, height);
                Resize?.Invoke(this, width, height);
            }

            public void DoUpdate()
            {
                BeforeUpdate?.Invoke(this);
                Update?.Invoke(this);
                AfterUpdate?.Invoke(this);
            }
        }
    }
}
