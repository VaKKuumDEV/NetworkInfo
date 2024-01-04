using NetworkInfo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Xamarin.Forms;
using System.Linq;
using Matcha.BackgroundService;
using System;
using System.Net;

namespace NetworkInfo.Services
{
    public class NetworkInfoService : IPeriodicTask
    {
        private static NetworkInfoService instance;
        public static NetworkInfoService Instance { get => instance; }
        public PreparedData? PreparedData { get; private set; } = null;
        public TimeSpan Interval { get; set; }
        public Location Location { get; set; } = new Location(0, 0);
        public string Address { get; set; } = "Загрузка...";
        public Models.NetworkInfo Network { get; set; } = new Models.NetworkInfo("Загрузка...", 0, Models.NetworkInfo.NetworkTypes.NOT_CONNECTED);
        public InternetInfo Internet { get; set; } = new InternetInfo(0, 0, 0);

        public NetworkInfoService(int seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
            if (instance == null) instance = this;
        }

        public async Task<bool> StartJob()
        {
            await Execute();
            return true;
        }

        public async Task Execute()
        {
            try
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    if (await Permissions.CheckStatusAsync<Permissions.Phone>() != PermissionStatus.Granted) throw new PermissionException("Phone permission not granted");
                    if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted) throw new PermissionException("Location permission not granted");
                }

                bool isAllDataOkay = true;
                try
                {
                    Location location = await ((App)Application.Current).GetCurrentLocation();
                    Location = location;

                    Geocoder geoCoder = new Geocoder();
                    Position position = new Position(location.Latitude, location.Longitude);
                    IEnumerable<string> possibleAddresses = await geoCoder.GetAddressesForPositionAsync(position);
                    string address = possibleAddresses.FirstOrDefault();
                    Address = address;
                }
                catch (Exception) { isAllDataOkay = false; }

                try
                {
                    Models.NetworkInfo networkInfo = await DependencyService.Get<INetworkState>().GetNetworkInfo();
                    Network = networkInfo;
                }
                catch (Exception) { isAllDataOkay = false; }

                try
                {
                    InternetInfo internetInfo = await DependencyService.Get<IInternet>().GetSpeed();
                    Internet = internetInfo;
                }
                catch (Exception) { isAllDataOkay = false; }

                if (isAllDataOkay)
                {
                    bool needSend = false;
                    string deviceId = DependencyService.Get<IDevice>().GetDeviceId();
                    PreparedData newData = new PreparedData(Network.Name, deviceId, Location.Latitude, Location.Longitude, Internet.Speed, Network.Strength, Network.Type);

                    if (PreparedData != null)
                    {
                        TimeSpan timeDiff = (newData.CreationDate - PreparedData.Value.CreationDate);
                        if (newData.Type != PreparedData.Value.Type) needSend = true;
                        else if (timeDiff.TotalMinutes >= 1) needSend = true;
                        else
                        {
                            Distance distance = Distance.BetweenPositions(new Position(PreparedData.Value.Lat, PreparedData.Value.Long), new Position(newData.Lat, newData.Long));
                            if (distance.Meters > 10) needSend = true;
                        }
                    }
                    else needSend = true;

                    if (needSend)
                    {
                        PreparedData = newData;
                        if (PreparedData.Value.Type != Models.NetworkInfo.NetworkTypes.NOT_CONNECTED && PreparedData.Value.Type != Models.NetworkInfo.NetworkTypes.WIFI) await WebApi.Send(PreparedData.Value);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
