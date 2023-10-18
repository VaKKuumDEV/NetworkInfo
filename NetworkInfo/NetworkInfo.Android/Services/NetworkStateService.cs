using Android.App;
using Android.Content;
using Android.Media.TV;
using Android.OS;
using Android.Telephony;
using NetworkInfo.Droid.Services;
using NetworkInfo.Models;
using NetworkInfo.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkStateService))]
namespace NetworkInfo.Droid.Services
{
    public class NetworkStateService : INetworkState
    {
        public async Task<List<StationInfo>> GetStations()
        {
            List<StationInfo> cellList = new List<StationInfo>();
            await Task.Run(() =>
            {
                TelephonyManager telephonyManager = (TelephonyManager)Application.Context.GetSystemService(Context.TelephonyService);

                PhoneType phoneTypeInt = telephonyManager.PhoneType;
                string phoneType = null;
                phoneType = phoneTypeInt == PhoneType.Gsm ? "gsm" : phoneType;
                phoneType = phoneTypeInt == PhoneType.Cdma ? "cdma" : phoneType;

                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
                    List<NeighboringCellInfo> neighCells = new List<NeighboringCellInfo>(telephonyManager.NeighboringCellInfo);
                    for (int i = 0; i < neighCells.Count; i++)
                    {
                        try
                        {
                            NeighboringCellInfo thisCell = neighCells[i];
                            cellList.Add(new StationInfo(thisCell.Cid, thisCell.Lac, thisCell.Rssi));
                        }
                        catch (Exception) { }
                    }
                }
                else
                {
                    List<CellInfo> infos = new List<CellInfo>(telephonyManager.AllCellInfo);
                    for (int i = 0; i < infos.Count; ++i)
                    {
                        try
                        {
                            CellInfo info = infos[i];
                            if (info.GetType() == typeof(CellInfoGsm))
                            {
                                CellSignalStrengthGsm gsm = ((CellInfoGsm)info).CellSignalStrength;
                                CellIdentityGsm identityGsm = ((CellInfoGsm)info).CellIdentity;
                                cellList.Add(new StationInfo(identityGsm.Cid, identityGsm.Lac, gsm.Dbm));
                            }
                            else if (info.GetType() == typeof(CellInfoLte))
                            {
                                CellSignalStrengthLte lte = ((CellInfoLte)info).CellSignalStrength;
                                CellIdentityLte identityLte = ((CellInfoLte)info).CellIdentity;
                                cellList.Add(new StationInfo(identityLte.Ci, identityLte.Tac, lte.Dbm));
                            }
                        }
                        catch (Exception) { }
                    }
                }
            });

            return cellList;
        }

        public async Task<Models.NetworkInfo> GetNetworkInfo()
        {
            TelephonyManager telephonyManager = (TelephonyManager)Application.Context.GetSystemService(Context.TelephonyService);
            string operatorName = telephonyManager.NetworkOperatorName;

            List<StationInfo> stations = await GetStations();
            int strength = 0;
            if (stations.Count > 0) strength = stations[0].Dbm;
            return new Models.NetworkInfo(operatorName, strength);
        }
    }
}