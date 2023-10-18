using NetworkInfo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetworkInfo.Services
{
    public interface INetworkState
    {
        Task<List<StationInfo>> GetStations();
        Task<Models.NetworkInfo> GetNetworkInfo();
    }
}
