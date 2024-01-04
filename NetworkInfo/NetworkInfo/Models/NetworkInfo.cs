using System;

namespace NetworkInfo.Models
{
    public readonly struct NetworkInfo
    {
        public enum NetworkTypes
        {
            NOT_CONNECTED,
            WIFI,
            GSM,
            EDGE,
            LTE,
            NR,
        };

        public string Name { get; }
        public int Strength { get; }
        public NetworkTypes Type { get; }

        public NetworkInfo(string name, int strength, NetworkTypes type)
        {
            Name = name;
            Strength = strength;
            Type = type;
        }
    }
}
