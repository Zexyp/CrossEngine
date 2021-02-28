using CrossEngine.Rendering.Display;
using CrossEngine.Layers;
using CrossEngine.InputSystem;

namespace CrossEngine.MainLoop
{
    public class LayeredApplication : Application
    {
        public LayerStack layerStack = new LayerStack();

        public LayeredApplication(Layer initialLayer, int initialWindowWidth = 640, int initialWindowHeight = 480, string initialWindowTitle = "Title") : base(initialWindowWidth, initialWindowHeight, initialWindowTitle)
        {
            layerStack.PushLayer(initialLayer);
        }

        protected override void Init()
        {
            
        }

        protected override void LoadContent()
        {
            layerStack.Awake();
        }

        protected override void Update()
        {
            layerStack.Update((float)Time.DeltaTime);
        }

        protected override void Render()
        {
            layerStack.Render();
        }

        protected override void UnloadContent()
        {
            layerStack.Die();
        }
    }
}
