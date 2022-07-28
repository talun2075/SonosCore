using Sonos.Classes.Enums;

namespace Sonos.Classes.Interfaces
{
    public interface IPlayerPropertiesRequest
    {
        PlayerDevicePropertiesTypes type { get; set; }
        string uuid { get; set; }
        string value { get; set; }
    }
}