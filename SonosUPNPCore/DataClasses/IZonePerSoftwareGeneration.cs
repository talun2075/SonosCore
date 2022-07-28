using OSTL.UPnP;
using SonosUPnP.Props;
using System;

namespace SonosUPNPCore.DataClasses
{
    public interface IZonePerSoftwareGeneration
    {
        ZoneProperties ZoneProperties { get; set; }

        event EventHandler<ZonePerSoftwareGeneration> GlobalSonosChange;

        void StartSubscription(UPnPDevice device);
    }
}