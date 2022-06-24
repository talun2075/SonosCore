using SonosUPnP;
using SonosUPnP.DataClasses;

namespace Sonos.Classes.Events
{
    public class Notification
    {
        public Notification(SonosEnums.EventingEnums ee, SonosPlayer pl)
        {
            EventType = ee;
            Player = pl;
        }
        public Notification(SonosEnums.EventingEnums ee, SonosDiscovery sd)
        {
            EventType = ee;
            Discovery = sd;
        }
        public SonosPlayer Player { get; set; } = null;
        public string Message { get; set; }
        public SonosDiscovery Discovery { get; set; } = null;
        public SonosEnums.EventingEnums EventType { get; set; }
    }
}