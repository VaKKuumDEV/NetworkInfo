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
        public string _operatorName = "Загрузка...";
        public string OperatorName
        {
            get => _operatorName;
            set => SetProperty(ref _operatorName, value);
        }

        public Command LoadPageCommand { get; }
        public CustomMap Map { get; private set; }
        public ObservableCollection<PreparedData> Points { get; private set; } = new ObservableCollection<PreparedData>();
        private bool MapLoaded { get; set; } = false;
        private bool UpdateProcess { get; set; } = false;

        public MapViewModel()
        {
            Title = "Map";

            Map = new CustomMap
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                IsShowingUser = true,
            };

            Map.ZoomChanged += new EventHandler<MapZoomChangedEventArgs>(async (sender, args) =>
            {
                if (MapLoaded && !UpdateProcess)
                {
                    Models.NetworkInfo networkInfo = await DependencyService.Get<INetworkState>().GetNetworkInfo();
                    OperatorName = networkInfo.Name;
                    await UpdatePoints(args.Span, networkInfo.Name);
                }
            });

            LoadPageCommand = new Command(() => ExecuteLoadPageCommand());
            ExecuteLoadPageCommand();
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
                    Map.MoveToRegion(mapSpan);
                }
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

        public async Task UpdatePoints(MapSpan span, string op) 
        {
            UpdateProcess = true;
            Map.Pins.Clear();
            Map.MapElements.Clear();
            List<PreparedData> points = await WebApi.GetLastRaw(op, span.Center.Latitude, span.Center.Longitude, (int)span.Radius.Meters);

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

                    Map.MapElements.Add(circle);
                }

                string pinLabel = "Средняя скорость " + point.Speed.ToString("0.00") + "Кб/сек";
                Pin pin = new Pin() { Position = new Position(point.Lat, point.Long), Label = pinLabel };
                
                Map.Pins.Add(pin);
            }

            UpdateProcess = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
