namespace CrossEngine
{
    public static class Time
    {
        public static double TimeScale { get; set; } = 1;
        public static double DeltaTime { get; internal set; }
        public static double UnscaledDeltaTime { get; internal set; }
        public static double FixedDeltaTime { get; set; } = 1d / 60;
        public static double ElapsedTime { get; internal set; }

        // TODO: consider usefulness
        public static float DeltaTimeF => (float)DeltaTime;
        public static float ElapsedTimeF => (float)ElapsedTime;
    }
}
