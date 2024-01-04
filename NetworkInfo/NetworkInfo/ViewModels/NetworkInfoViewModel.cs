using NetworkInfo.Models;
using NetworkInfo.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using static NetworkInfo.Models.PreparedData;

namespace NetworkInfo.ViewModels
{
    public class NetworkInfoViewModel: BaseViewModel
    {
        public string _operatorName = "Загрузка...";
        public string OperatorName
        {
            get => _operatorName;
            set => SetProperty(ref _operatorName, value);
        }

        public int _strengthValue = 0;
        public int NetworkStrength
        {
            get => _strengthValue;
            set => SetProperty(ref _strengthValue, value);
        }

        public string _networkType = "NaN";
        public string NetworkType
        {
            get => _networkType;
            set => SetProperty(ref _networkType, value);
        }

        private string _address = "Загрузка...";
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private Location _location = new Location(0, 0);
        public Location Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private string _locationLat = 0.ToString("0.0000");
        private string _locationLong = 0.ToString("0.0000");
        public string LocationLat
        {
            get => _locationLat;
            set => SetProperty(ref _locationLat, value);
        }
        public string LocationLong
        {
            get => _locationLong;
            set => SetProperty(ref _locationLong, value);
        }

        private double _signalProp = 0;
        public double SignalProp
        {
            get => _signalProp;
            set => SetProperty(ref _signalProp, value);
        }

        public string _signalPropStr = "0";
        public string SignalPropStr
        {
            get => _signalPropStr;
            set => SetProperty(ref _signalPropStr, value);
        }

        private InternetInfo _internetInfo = new InternetInfo(0, 0, 0);
        public InternetInfo Internet
        {
            get => _internetInfo;
            set => SetProperty(ref _internetInfo, value);
        }

        private string _strengthImage = "signal_no.png";
        public string StrengthImage
        {
            get => _strengthImage;
            set => SetProperty(ref _strengthImage, value);
        }

        public Task LoadInfoTask { get; private set; }
        public PreparedData LastNearPoint { get; private set; }
        private bool IsWorking { get; set; } = true;

        public NetworkInfoViewModel()
        {
            Title = "NetworkInfo";
        }

        public void OnAppearing()
        {
            IsWorking = true;
            LoadInfoTask = Task.Run(async () =>
            {
                while (IsWorking)
                {
                    if (NetworkInfoService.Instance != null)
                    {
                        Location = NetworkInfoService.Instance.Location;
                        OperatorName = NetworkInfoService.Instance.Network.Name;
                        NetworkStrength = NetworkInfoService.Instance.Network.Strength;
                        Address = NetworkInfoService.Instance.Address;
                        Internet = NetworkInfoService.Instance.Internet;

                        Models.NetworkInfo.NetworkTypes networkType = NetworkInfoService.Instance.Network.Type;
                        string networkName = "Нет сети";
                        if (networkType == Models.NetworkInfo.NetworkTypes.GSM) networkName = "2G";
                        else if (networkType == Models.NetworkInfo.NetworkTypes.EDGE) networkName = "3G";
                        else if (networkType == Models.NetworkInfo.NetworkTypes.LTE) networkName = "4G";
                        else if (networkType == Models.NetworkInfo.NetworkTypes.NR) networkName = "5G";
                        else if (networkType == Models.NetworkInfo.NetworkTypes.WIFI) networkName = "WiFi";
                        NetworkType = networkName;

                        LocationLat = Location.Latitude.ToString("0.0000");
                        LocationLong = Location.Longitude.ToString("0.0000");

                        if(networkType == Models.NetworkInfo.NetworkTypes.NOT_CONNECTED || networkType == Models.NetworkInfo.NetworkTypes.WIFI) StrengthImage = "signal_no.png";
                        else
                        {
                            SignalLevels level = SignalLevels.VERY_LOW;
                            if (networkType == Models.NetworkInfo.NetworkTypes.LTE || networkType == Models.NetworkInfo.NetworkTypes.NR)
                            {
                                if (NetworkStrength <= -110 && NetworkStrength > -115) level = SignalLevels.LOW;
                                else if (NetworkStrength <= -105 && NetworkStrength > -110) level = SignalLevels.MEDIUM;
                                else if (NetworkStrength <= -100 && NetworkStrength > -105) level = SignalLevels.HIGH;
                                else if (NetworkStrength > -95) level = SignalLevels.VERY_HIGH;
                            }
                            else
                            {
                                if (NetworkStrength <= -105 && NetworkStrength > -110) level = SignalLevels.LOW;
                                else if (NetworkStrength <= -95 && NetworkStrength > -105) level = SignalLevels.MEDIUM;
                                else if (NetworkStrength <= -85 && NetworkStrength > -95) level = SignalLevels.HIGH;
                                else if (NetworkStrength > -85) level = SignalLevels.VERY_HIGH;
                            }

                            if (level == SignalLevels.VERY_LOW || level == SignalLevels.LOW) StrengthImage = "signal_1.png";
                            else if (level == SignalLevels.MEDIUM) StrengthImage = "signal_2.png";
                            else if (level == SignalLevels.HIGH) StrengthImage = "signal_3.png";
                            else if (level == SignalLevels.VERY_HIGH) StrengthImage = "signal_4.png";
                        }

                        try
                        {
                            if(networkType != Models.NetworkInfo.NetworkTypes.NOT_CONNECTED && networkType != Models.NetworkInfo.NetworkTypes.WIFI)
                            {
                                List<PreparedData> points = await WebApi.GetLastRaw(OperatorName, Location.Latitude, Location.Longitude, 1000, networkType);
                                if (points.Count > 0)
                                {
                                    points.Sort((a, b) => Distance.BetweenPositions(new Position(Location.Latitude, Location.Longitude), new Position(a.Lat, a.Long)).Meters > Distance.BetweenPositions(new Position(Location.Latitude, Location.Longitude), new Position(b.Lat, b.Long)).Meters ? 1 : -1);
                                    LastNearPoint = points[0];
                                    SignalProp = Internet.Speed / LastNearPoint.Speed;
                                    SignalPropStr = (SignalProp * 100).ToString("0");
                                }
                            }
                        }
                        catch (Exception) { }
                    }

                    await Task.Delay(1000);
                }
            });
        }

        public void OnDisappearing()
        {
            IsWorking = false;
        }
    }
}
