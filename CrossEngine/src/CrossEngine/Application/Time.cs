namespace CrossEngine
{
    public static class Time
    {
        public static double DeltaTime { get; private set; }
        public static float DeltaTimeF { get => (float)DeltaTime; }
        public static double TotalElapsedSeconds { get; private set; }

        public static void Update(double newTime)
        {
            DeltaTime = newTime - TotalElapsedSeconds;
            TotalElapsedSeconds = newTime;
        }
    }
}
