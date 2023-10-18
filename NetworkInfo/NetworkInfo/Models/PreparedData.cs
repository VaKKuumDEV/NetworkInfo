using Newtonsoft.Json;
using System;

namespace NetworkInfo.Models
{
    public struct PreparedData
    {
        public enum SignalLevels
        {
            VERY_LOW,
            LOW,
            MEDIUM,
            HIGH,
            VERY_HIGH,
        };

        [JsonProperty("operator")] public string Operator { get; set; }
        [JsonProperty("device")] public string Device { get; set; }
        [JsonProperty("lat")] public double Lat { get; set; }
        [JsonProperty("long")] public double Long { get; set; }
        [JsonProperty("speed")] public double Speed { get; set; }
        [JsonProperty("strength")] public int Strength { get; set; }
        [JsonProperty("radius")] public double Radius { get; set; }
        [JsonIgnore] public DateTime CreationDate { get; set; }
        [JsonIgnore] public SignalLevels Level { get
            {
                SignalLevels level = SignalLevels.VERY_LOW;
                if (Strength <= -105 && Strength > -110) level = SignalLevels.LOW;
                else if (Strength <= -95 && Strength > -105) level = SignalLevels.MEDIUM;
                else if (Strength <= -85 && Strength > -95) level = SignalLevels.HIGH;
                else if (Strength > -85) level = SignalLevels.VERY_HIGH;
                return level;
            }
        }

        public PreparedData(string op, string device, double lat, double longitude, double speed, int strength)
        {
            Operator = op;
            Device = device;
            Lat = lat;
            Long = longitude;
            Speed = speed;
            Strength = strength;
            CreationDate = DateTime.Now;
            Radius = 0;
        }
    }
}
