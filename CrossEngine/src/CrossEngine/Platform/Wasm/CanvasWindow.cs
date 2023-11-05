using CrossEngine.Display;
using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrossEngine.Platform.Wasm
{
    class CanvasWindow : Window
    {
        // TODO: consider pop out window

        public override double Time => _time;
        public override bool ShouldClose { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public override nint Handle => throw new System.NotImplementedException();
        public override event OnEventFunction OnEvent;

        private double _time;
        private static CanvasWindow _instance;

        public CanvasWindow()
        {
            Interop.Initialize();

            Context = new EGLContext();
            Context.Init();

            _instance = this;
        }

        public override unsafe void CreateWindow()
        {
            SetupCallbacks();

            // very sketchy
            var holder = this;
            TypedReference tr = __makeref(holder);
            IntPtr ptr = **(IntPtr**)&tr;
            Emscripten.RequestAnimationFrameLoop((delegate* unmanaged<double, nint, int>)&Frame, ptr);
        }

        public override void DestroyWindow()
        {
            RemoveCallbacks();
        }

        public override void PollWindowEvents()
        {
            throw new System.NotImplementedException();
        }

        public override unsafe void SetIcon(void* data, uint width, uint height)
        {
            throw new System.NotImplementedException();
        }

        protected override (uint Width, uint Height) GetMonitorSize()
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateFullscreen()
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateSize()
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateTitle()
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateVSync()
        {
            throw new System.NotImplementedException();
        }

        private Interop.KeyDownCallback keyDown;
        private Interop.KeyUpCallback keyUp;
        private Interop.MouseMoveCallback mouseMove;
        private Interop.MouseDownCallback mouseDown;
        private Interop.MouseUpCallback mouseUp;
        private Interop.CanvasResizeCallback canvasResize;

        private Logger log = new Logger(typeof(CanvasWindow).Name);

        [UnmanagedCallersOnly]
        static private unsafe int Frame(double time, nint thiz)
        {
            _instance.OnEvent?.Invoke(new WindowRefreshEvent());

            return 1;
        }

        private void SetupCallbacks()
        {
            Interop.KeyDown += keyDown = (bool shift, bool ctrl, bool alt, bool repeat, string code) => {
                OnEvent?.Invoke(new KeyPressedEvent(JSInput.TranslateKey(code), repeat ? 1 : 0));
            };
            Interop.KeyUp += keyUp = (bool shift, bool ctrl, bool alt, string code) => {
                OnEvent?.Invoke(new KeyReleasedEvent(JSInput.TranslateKey(code)));
            };
            Interop.MouseMove += mouseMove = (float x, float y) => {
                OnEvent?.Invoke(new MouseMovedEvent(x, y));
            };
            Interop.MouseDown += mouseDown = (bool shift, bool ctrl, bool alt, int button) => {
                OnEvent?.Invoke(new MousePressedEvent(JSInput.TranslateMouse(button)));
            };
            Interop.MouseUp += mouseUp = (bool shift, bool ctrl, bool alt, int button) => {
                OnEvent?.Invoke(new MouseReleasedEvent(JSInput.TranslateMouse(button)));
            };
            Interop.CanvasResize += canvasResize = (float width, float height) => {
                OnEvent?.Invoke(new WindowResizeEvent((uint)width, (uint)height));
            };
        }

        private void RemoveCallbacks()
        {
            Interop.KeyDown -= keyDown;
            Interop.KeyUp -= keyUp;
            Interop.MouseMove -= mouseMove;
            Interop.MouseDown -= mouseDown;
            Interop.MouseUp -= mouseUp;
            Interop.CanvasResize -= canvasResize;
        }
    }
}