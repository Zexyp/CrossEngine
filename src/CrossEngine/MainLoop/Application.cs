using GLFW;

using CrossEngine.Rendering.Display;
using CrossEngine.InputSystem;

namespace CrossEngine.MainLoop
{
    public abstract class Application
    {
        protected int initialWindowWidth;
        protected int initialWindowHeight;
        protected string initialWindowTitle;

        public Application(int initialWindowWidth, int initialWindowHeight, string initialWindowTitle)
        {
            this.initialWindowWidth = initialWindowWidth;
            this.initialWindowHeight = initialWindowHeight;
            this.initialWindowTitle = initialWindowTitle;
        }

        public void Run()
        {
            Init();

            DisplayManager.CreateWindow(initialWindowWidth, initialWindowHeight, initialWindowTitle);

            Input.Init();

            LoadContent();

            while (!Glfw.WindowShouldClose(DisplayManager.Window))
            {
                Time.Update(Glfw.Time);

                Input.Update();

                Update();

                Input.EndFrame();

                Glfw.PollEvents();

                Render();

                Glfw.SwapBuffers(DisplayManager.Window);
            }

            UnloadContent();

            DisplayManager.CloseWindow();
        }

        protected abstract void Init();
        protected abstract void LoadContent();
        protected abstract void UnloadContent();

        protected abstract void Update();
        protected abstract void Render();
    }
}
