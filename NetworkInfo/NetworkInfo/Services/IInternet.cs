using NetworkInfo.Models;
using System.Threading.Tasks;

namespace NetworkInfo.Services
{
    public interface IInternet
    {
        Task<InternetInfo> GetSpeed();
    }
}
