using Sonos.Classes.Interfaces;
using SonosUPnP;
using SonosUPNPCore.Enums;
using System;
using System.Threading.Tasks;

namespace Sonos.Classes
{
    public class PlayerDeviceProperties : IPlayerDeviceProperties
    {

        public async Task<PlayerDeviceProperties> FilledData(SonosPlayer sp)
        {
            if (sp == null) return null;
            if (sp.DeviceProperties == null) return null;
            if (sp.RenderingControl == null) return null;

            var lo = await sp.DeviceProperties.GetButtonLockState();
            if (Enum.TryParse(lo, out OnOffSwitch oos))
            {
                sp.PlayerProperties.ButtonLockState = oos;

            }

            ButtonLockState = sp.PlayerProperties.ButtonLockState == OnOffSwitch.On;

            var led = await sp.DeviceProperties.GetLEDState();
            if (Enum.TryParse(led, out OnOffSwitch oosled))
            {
                sp.PlayerProperties.LEDState = oosled;

            }

            LEDState = sp.PlayerProperties.LEDState == OnOffSwitch.On;
            await sp.RenderingControl.GetBass();
            await sp.RenderingControl.GetTreble();
            await sp.RenderingControl.GetHeadphoneConnected();
            await sp.RenderingControl.GetLoudness();
            await sp.RenderingControl.GetOutputFixed();
            await sp.RenderingControl.GetSupportsOutputFixed();

            Bass = sp.PlayerProperties.Bass;
            HeadphoneConnected = sp.PlayerProperties.HeadphoneConnected;
            Loudness = sp.PlayerProperties.Loudness;
            OutputFixed = sp.PlayerProperties.OutputFixed;
            SupportsOutputFixed = sp.PlayerProperties.SupportOutputFixed;
            Treble = sp.PlayerProperties.Treble;
            return this;
        }

        public bool ButtonLockState { get; set; } //GetButtonLockState
        public bool LEDState { get; set; } //GetLEDState
        public int Bass { get; set; }

        public bool HeadphoneConnected { get; set; }

        public bool Loudness { get; set; }

        public bool OutputFixed { get; set; } //set nur wenn GetSupportsOutputFixed
        public bool SupportsOutputFixed { get; set; }
        public int Treble { get; set; }


    }
}
