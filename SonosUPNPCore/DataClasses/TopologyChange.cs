using System;

namespace SonosUPNPCore.DataClasses
{
    public class TopologyChange
    {
        public int SoftwareVersion { get; set; }
        public Boolean ActiveSubscription { get; set; } = false;
        public Boolean UseAlarmClock { get; set; } = false;
        public Boolean UseMediaServer { get; set; } = false;
    }
}
