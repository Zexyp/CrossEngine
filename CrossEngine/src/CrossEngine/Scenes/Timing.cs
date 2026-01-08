namespace CrossEngine.Scenes;

public ref struct Timing
{
    public double Scale { get; set; } = 1;
    
    public double Elapsed { get; internal set; }
    public double Delta { get; internal set; }
    public double UnscaledElapsed { get; internal set; }
    public double UnscaledDelta { get; internal set; }

    public double FixedElapsed { get; internal set; }
    public double FixedDelta { get; internal set; }
    public double FixedUnscaledElapsed { get; internal set; }
    public double FixedUnscaledDelta { get; set; } = 1d / 60; // fixed timestep can be configured using this

    public float DeltaF => (float)Delta;
    public float ElapsedF => (float)Elapsed;
    public float UnscaledDeltaF => (float)UnscaledDelta;

    public Timing()
    {
        
    }
}