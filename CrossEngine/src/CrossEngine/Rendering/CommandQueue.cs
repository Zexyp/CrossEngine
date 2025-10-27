using System.Collections.Concurrent;

namespace CrossEngine.Rendering;

public interface IRenderCommand
{
    void Execute(GraphicsContext ctx);
}

public class CommandQueue
{
    private readonly ConcurrentQueue<IRenderCommand> _commands = new();
    
    public void Submit(IRenderCommand command)
    {
        _commands.Enqueue(command);
    }
    
    public void Flush(GraphicsContext ctx)
    {
        while (_commands.TryDequeue(out var cmd))
        {
            cmd.Execute(ctx);
        }
    }
}