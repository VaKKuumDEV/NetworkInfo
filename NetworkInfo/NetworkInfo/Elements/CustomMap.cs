using NetworkInfo.Services;
using System;
using System.ComponentModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace NetworkInfo.Elements
{
    public class MapZoomChangedEventArgs
    {
        public MapSpan Span { get; }

        public MapZoomChangedEventArgs(MapSpan span)
        {
            Span = span;
        }
    }

    public class CustomMap : Xamarin.Forms.Maps.Map
    {
        private readonly struct MapInfo
        {
            public int LastRaius { get; }
            public Position LastCenter { get; }

            public MapInfo(int radius, Position position)
            {
                LastRaius = radius;
                LastCenter = position;
            }
        };

        private MapInfo? LastMapInfo { get; set; } = null;
        public event EventHandler<MapZoomChangedEventArgs> ZoomChanged;

        #region Constructor
        public CustomMap()
        {
            PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                CustomMap map = sender as CustomMap;

                if (e.PropertyName == "VisibleRegion" && map.VisibleRegion != null)
                {
                    bool needUpdate = false;
                    if (LastMapInfo == null) needUpdate = true;
                    else
                    {
                        Distance distance = Distance.BetweenPositions(LastMapInfo.Value.LastCenter, map.VisibleRegion.Center);
                        if (distance.Meters >= (0.5 * LastMapInfo.Value.LastRaius) || Math.Abs(LastMapInfo.Value.LastRaius - map.VisibleRegion.Radius.Meters) >= (0.1 * LastMapInfo.Value.LastRaius)) needUpdate = true;
                    }

                    if (needUpdate)
                    {
                        LastMapInfo = new MapInfo((int)map.VisibleRegion.Radius.Meters, map.VisibleRegion.Center);
                        ZoomChanged?.Invoke(sender, new MapZoomChangedEventArgs(map.VisibleRegion));
                    }
                }
            };
        }
        #endregion
    }
}
