using CrossEngine.Debugging;
using CrossEngineEditor;

internal class Program
{
    static void Main(string[] args)
    {
        var app = new EditorApplication();
        app.Run();

        GPUGC.PrintCollected();
    }
}
