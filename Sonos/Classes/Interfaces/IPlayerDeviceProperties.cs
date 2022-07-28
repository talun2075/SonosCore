using SonosUPnP;
using System.Threading.Tasks;

namespace Sonos.Classes.Interfaces
{
    public interface IPlayerDeviceProperties
    {
        int Bass { get; set; }
        bool ButtonLockState { get; set; }
        bool HeadphoneConnected { get; set; }
        bool LEDState { get; set; }
        bool Loudness { get; set; }
        bool OutputFixed { get; set; }
        bool SupportsOutputFixed { get; set; }
        int Treble { get; set; }

        Task<PlayerDeviceProperties> FilledData(SonosPlayer sp);
    }
}