using NetworkInfo.Droid.Services;
using NetworkInfo.Services;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using NetworkInfo.Models;

[assembly: Xamarin.Forms.Dependency(typeof(InternetService))]
namespace NetworkInfo.Droid.Services
{
    public class InternetService: IInternet
    {
        public async Task<InternetInfo> GetSpeed()
        {
            DateTime dt1 = DateTime.Now;
            var client = new HttpClient();
            byte[] data = await client.GetByteArrayAsync("https://gtwallpaper.org/sites/default/files/wallpaper/159920/cloudy-forest-mountains-and-lake-wallpapers-159920-6602-8111228.png");
            DateTime dt2 = DateTime.Now;

            InternetInfo info = new InternetInfo(data.Length / 1024, (dt2 - dt1).TotalSeconds, Math.Round(data.Length / 1024 / (dt2 - dt1).TotalSeconds, 2));
            return info;
        }
    }
}