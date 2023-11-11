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

        //public static double FixedElapsed;
        //public static double FixedDelta;
        //public static double FixedUnscaled;
        //public static double FixedUnscaledDelta;

        public static float DeltaF => (float)Delta;
        public static float ElapsedF => (float)Elapsed;
    }
}
