namespace CrossEngine.MainLoop
{
    public static class Time
    {
        public static double DeltaTime { get; private set; }
        public static double TotalElapsedSeconds { get; private set; }

        public static void Update(double newTime)
        {
            DeltaTime = newTime - TotalElapsedSeconds;
            TotalElapsedSeconds = newTime;
        }
    }
}
