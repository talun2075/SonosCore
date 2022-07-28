using Sonos.Classes.Enums;
using Sonos.Classes.Interfaces;

namespace Sonos.Classes
{
    public class PlayerPropertiesRequest : IPlayerPropertiesRequest
    {
        public string uuid { get; set; }
        public string value { get; set; }
        public PlayerDevicePropertiesTypes type { get; set; }
    }
}
