using SonosData.Enums;

namespace SonosData.DataClasses
{
    public class ZoneGroupMember
    {
        public string UUID { get; set; } = "";
        public string Location { get; set; } = "";
        public string ZoneName { get; set; } = "";

        public string Icon { get; set; } = "";

        public int Configuration { get; set; }

        public string SoftwareVersion { get; set; } = "";
        public SoftwareGeneration SoftwareGeneration { get; set; }
        public string MinCompatibleVersion { get; set; } = "";
        public string LegacyCompatibleVersion { get; set; } = "";
        public int BootSeq { get; set; }
        public bool TVConfigurationError { get; set; }= false;
        public bool HdmiCecAvailable { get; set; } = false;
        public bool WirelessMode { get; set; } = false;
        public bool WirelessLeafOnly { get; set; } = false;
        public bool HasConfiguredSSID { get; set; } = false;
        public bool VoiceConfigState { get; set; } = false;
        public int ChannelFreq { get; set; }
        public bool BehindWifiExtender { get; set; } = false;
        public bool WifiEnabled { get; set; } = false;
        public bool Orientation { get; set; } = false;
        public int RoomCalibrationState { get; set; }
        public int SecureRegState { get; set; }
        public bool MicEnabled { get; set; } = false;
        public bool AirPlayEnabled { get; set; } = false;
        public bool IdleState { get; set; } = false;
        public string MoreInfo { get; set; } = "";
    }
}
