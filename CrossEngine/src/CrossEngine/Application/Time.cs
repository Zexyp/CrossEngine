namespace CrossEngine
{
    public static class Time
    {
        public static double TimeScale { get; set; } = 1;
        public static double DeltaTime { get; private set; }
        public static double UnscaledDeltaTime { get; private set; }
        public static double FixedDeltaTime { get; set; } = 1d / 15;
        public static double ElapsedTime { get; private set; }

        // TODO: consider usefulness
        public static float DeltaTimeF { get => (float)DeltaTime; }
        public static float ElapsedTimeF { get => (float)ElapsedTime; }

        //public static double TimeScale { get; set; }

        internal static void Update(double newTime)
        {
            UnscaledDeltaTime = (newTime - ElapsedTime);
            DeltaTime = UnscaledDeltaTime * TimeScale;

            ElapsedTime = newTime;
        }

        internal static void Update(double newTime, double newDelta)
        {
            UnscaledDeltaTime = newDelta;
            DeltaTime = UnscaledDeltaTime * TimeScale;

            ElapsedTime = newTime;
        }
    }
}
