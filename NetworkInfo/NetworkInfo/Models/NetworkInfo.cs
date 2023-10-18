namespace NetworkInfo.Models
{
    public readonly struct NetworkInfo
    {
        public string Name { get; }
        public int Strength { get; }

        public NetworkInfo(string name, int strength)
        {
            Name = name;
            Strength = strength;
        }
    }
}
