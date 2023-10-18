using Matcha.BackgroundService;
using NetworkInfo.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NetworkInfo
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        public async Task<Location> GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);
                if (location != null) return location;
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
                MainThread.BeginInvokeOnMainThread(() => DependencyService.Get<IMessage>().ShortAlert("Ваше устройство не поддерживается"));
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
                MainThread.BeginInvokeOnMainThread(() => DependencyService.Get<IMessage>().ShortAlert("Функция геопозиционирования отключена в настройках"));
            }
            catch (PermissionException)
            {
                // Handle permission exception
                MainThread.BeginInvokeOnMainThread(() => DependencyService.Get<IMessage>().ShortAlert("Отсутствуют необходимые разрешения"));
            }
            catch (Exception ex)
            {
                // Unable to get location
                MainThread.BeginInvokeOnMainThread(() => DependencyService.Get<IMessage>().ShortAlert("Непредвиденная ошибка: " + ex.Message));
            }

            return null;
        }

        protected override async void OnStart()
        {
            bool permissionsOkay = true;
            if (Device.RuntimePlatform == Device.Android)
            {
                if (await Permissions.CheckStatusAsync<Permissions.Phone>() != PermissionStatus.Granted)
                {
                    var permissionStatus = await Permissions.RequestAsync<Permissions.Phone>();
                    if (permissionStatus != PermissionStatus.Granted) permissionsOkay = false;
                }

                if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
                {
                    var permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (permissionStatus != PermissionStatus.Granted) permissionsOkay = false;
                }
            }

            if (permissionsOkay)
            {
                BackgroundAggregatorService.Add(() => new NetworkInfoService(3));
            }
            else
            {
                await Current.MainPage.DisplayAlert("Внимание", "Необходимые права не были предоставлены приложению. Функционал ограничен.", "Окей");
            }

            BackgroundAggregatorService.StartBackgroundService();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
