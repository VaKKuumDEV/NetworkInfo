namespace NetworkInfo.Models
{
    public readonly struct InternetInfo
    {
        public int TotalKiloBytes { get; }
        public double ElapsedTime { get; }
        public double Speed { get; }

        public InternetInfo(int totalKiloBytes, double elapsedTime, double speed)
        {
            TotalKiloBytes = totalKiloBytes;
            ElapsedTime = elapsedTime;
            Speed = speed;
        }
    }
}
