using NetworkInfo.Elements;
using NetworkInfo.Models;
using NetworkInfo.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace NetworkInfo.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        public ObservableCollection<string> _networks = new ObservableCollection<string>();
        public ObservableCollection<string> Networks
        {
            get => _networks;
            set => SetProperty(ref _networks, value);
        }

        public CustomMap NetworksMap { get; private set; }
        private bool MapLoaded { get; set; } = false;
        private bool UpdateProcess { get; set; } = false;
        private Picker NetworksPicker { get; }

        public MapViewModel(ref Picker networksPicker)
        {
            Title = "Map";
            NetworksPicker = networksPicker;

            NetworksMap = new CustomMap
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                IsShowingUser = true,
            };

            NetworksMap.ZoomChanged += new EventHandler<MapZoomChangedEventArgs>(async (sender, args) =>
            {
                if (MapLoaded && !UpdateProcess && NetworksPicker.SelectedIndex != -1 && NetworksPicker.SelectedIndex < Networks.Count)
                {
                    Models.NetworkInfo networkInfo = await DependencyService.Get<INetworkState>().GetNetworkInfo();
                    await UpdatePoints(args.Span, Networks[NetworksPicker.SelectedIndex], networkInfo.Type);
                }
            });

            NetworksPicker.SelectedIndexChanged += NetworksPicker_SelectedIndexChanged;

            ExecuteLoadPageCommand();
        }

        private async void NetworksPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = NetworksPicker.SelectedIndex;
            if (MapLoaded && !UpdateProcess && index != -1 && index < Networks.Count)
            {
                Models.NetworkInfo networkInfo = await DependencyService.Get<INetworkState>().GetNetworkInfo();
                await UpdatePoints(new MapSpan(NetworksMap.VisibleRegion.Center, NetworksMap.VisibleRegion.LatitudeDegrees, NetworksMap.VisibleRegion.LatitudeDegrees), Networks[index], networkInfo.Type);
            }
        }

        private async void ExecuteLoadPageCommand()
        {
            IsBusy = true;

            try
            {
                bool permissionOkay = true;
                if (Device.RuntimePlatform == Device.Android)
                {
                    if (await Permissions.CheckStatusAsync<Permissions.Phone>() != PermissionStatus.Granted) permissionOkay = false;
                    if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted) permissionOkay = false;
                }

                if (permissionOkay)
                {
                    Location location = await ((App)Application.Current).GetCurrentLocation();

                    MapLoaded = true;
                    MapSpan mapSpan = new MapSpan(new Position(location.Latitude, location.Longitude), 0.1, 0.1);
                    NetworksMap.MoveToRegion(mapSpan);
                }

                Models.NetworkInfo networkInfo = await DependencyService.Get<INetworkState>().GetNetworkInfo();
                List<string> serverNetworks = await WebApi.GetOperators();
                Networks = new ObservableCollection<string>(serverNetworks);

                int operatorIndex;
                if ((operatorIndex = Networks.IndexOf(networkInfo.Name)) == -1)
                {
                    Networks.Add(networkInfo.Name);
                    NetworksPicker.SelectedIndex = Networks.Count - 1;
                }
                else NetworksPicker.SelectedIndex = operatorIndex;
            }
            catch (Exception ex)
            {
                ShowDialogMessage(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdatePoints(MapSpan span, string op, Models.NetworkInfo.NetworkTypes type) 
        {
            UpdateProcess = true;
            NetworksMap.Pins.Clear();
            NetworksMap.MapElements.Clear();
            List<PreparedData> points = await WebApi.GetLastRaw(op, span.Center.Latitude, span.Center.Longitude, (int)span.Radius.Meters, type);

            foreach (PreparedData point in points)
            {
                if (point.Radius > 0)
                {
                    string fillColor = "#BFff6b90";
                    if (point.Level == PreparedData.SignalLevels.LOW) fillColor = "#BFfd90ad";
                    else if (point.Level == PreparedData.SignalLevels.MEDIUM) fillColor = "#BFb5ffa4";
                    else if (point.Level == PreparedData.SignalLevels.HIGH) fillColor = "#BF8cf289";
                    else if (point.Level == PreparedData.SignalLevels.VERY_HIGH) fillColor = "#BF73e1ad";

                    Circle circle = new Circle()
                    {
                        Radius = Distance.FromMeters(point.Radius),
                        StrokeWidth = 2,
                        StrokeColor = Color.FromHex("#1BA1E2"),
                        FillColor = Color.FromHex(fillColor),
                        Center = new Position(point.Lat, point.Long),
                    };

                    NetworksMap.MapElements.Add(circle);
                }

                string pinLabel = "Средняя скорость " + point.Speed.ToString("0.00") + "Кб/сек";
                Pin pin = new Pin() { Position = new Position(point.Lat, point.Long), Label = pinLabel };

                NetworksMap.Pins.Add(pin);
            }

            UpdateProcess = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
