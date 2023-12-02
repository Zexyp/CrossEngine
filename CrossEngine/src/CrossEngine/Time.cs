namespace CrossEngine
{
    public static class Time
    {
        public static double MaximumDelta { get; set; } = 1d / 3;
        public static double Scale { get; set; } = 1;
        
        public static double Elapsed { get; internal set; }
        public static double Delta { get; internal set; }
        public static double UnscaledElapsed { get; internal set; }
        public static double UnscaledDelta { get; internal set; }

        public static double FixedElapsed { get; internal set; }
        public static double FixedDelta { get; internal set; }
        public static double FixedUnscaledElapsed { get; internal set; }
        public static double FixedUnscaledDelta { get; set; } = 1d / 60; // fixed timestep can be configured using this

        public static float DeltaF => (float)Delta;
        public static float ElapsedF => (float)Elapsed;
        public static float UnscaledDeltaF => (float)UnscaledDelta;
    }
}
