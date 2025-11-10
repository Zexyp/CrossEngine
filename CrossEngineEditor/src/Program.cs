using CrossEngine.Debugging;
using CrossEngine.Rendering;
using CrossEngineEditor;

namespace CrossEngineEditor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = new EditorApplication();
            app.Run();

            GpuGC.PrintCollected();
        }
    }
}
