using NetworkInfo.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkInfo.Services
{
    public static class WebApi
    {
        public static async Task<List<PreparedData>> GetLastRaw(string op, double lat, double lon, int radius, Models.NetworkInfo.NetworkTypes type)
        {
            JObject answer = await Utils.ApiGet(new Dictionary<string, string>()
            {
                ["method"] = "map",
                ["operator"] = op,
                ["lat"] = lat.ToString("0.000000", CultureInfo.InvariantCulture),
                ["long"] = lon.ToString("0.000000", CultureInfo.InvariantCulture),
                ["radius"] = radius.ToString(),
                ["type"] = ((int)type).ToString(),
            });

            JArray rawsArray = answer.Value<JArray>("points");
            List<PreparedData> results = new List<PreparedData>();
            if (rawsArray != null && rawsArray.Count > 0)
            {
                foreach(JObject rawObject in rawsArray.Cast<JObject>())
                {
                    PreparedData castedData = rawObject.ToObject<PreparedData>();
                    results.Add(castedData);
                }
            }

            return results;
        }

        public static async Task<List<string>> GetOperators()
        {
            JObject answer = await Utils.ApiGet(new Dictionary<string, string>()
            {
                ["method"] = "operators",
            });

            JArray operatorsArray = answer.Value<JArray>("operators");
            List<string> results = new List<string>(operatorsArray.Cast<JValue>().Select(item => item.Value<string>()));

            return results;
        }

        public static async Task Send(PreparedData data)
        {
            JObject answer = await Utils.ApiPost(new Dictionary<string, object>()
            {
                ["method"] = "send",
                ["operator"] = data.Operator,
                ["device"] = data.Device,
                ["lat"] = data.Lat.ToString("0.000000", CultureInfo.InvariantCulture),
                ["long"] = data.Long.ToString("0.000000", CultureInfo.InvariantCulture),
                ["speed"] = data.Speed.ToString("0.000000", CultureInfo.InvariantCulture),
                ["strength"] = data.Strength.ToString(),
                ["type"] = ((int)data.Type).ToString(),
            });
        }
    }
}
