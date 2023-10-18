using Android.App;
using NetworkInfo.Droid.Services;
using NetworkInfo.Services;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceService))]
namespace NetworkInfo.Droid.Services
{
    public class DeviceService : IDevice
    {
        public string GetDeviceId()
        {
            return Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
        }
    }
}