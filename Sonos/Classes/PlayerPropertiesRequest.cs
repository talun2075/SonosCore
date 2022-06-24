namespace Sonos.Classes
{
    public class PlayerPropertiesRequest
    {
        public string uuid { get; set; }
        public string value { get; set; }
        public PlayerDevicePropertiesTypes type { get; set; }
    }

    public enum PlayerDevicePropertiesTypes
    {
        ButtonLockState,Bass,LEDState,Loudness, OutputFixed, Treble
    }
}
