using System;

namespace CrossEngineRuntime
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var app = new RuntimeApplication();
            app.Run();
        }
    }
}
