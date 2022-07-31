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
        public bool TVConfigurationError { get; set; }
        public bool HdmiCecAvailable { get; set; }
        public bool WirelessMode { get; set; }
        public bool WirelessLeafOnly { get; set; }
        public bool HasConfiguredSSID { get; set; }
        public bool VoiceConfigState { get; set; }
        public int ChannelFreq { get; set; }
        public bool BehindWifiExtender { get; set; }
        public bool WifiEnabled { get; set; }
        public bool Orientation { get; set; }
        public int RoomCalibrationState { get; set; }
        public int SecureRegState { get; set; }
        public bool MicEnabled { get; set; }
        public bool AirPlayEnabled { get; set; }
        public bool IdleState { get; set; }
        public string MoreInfo { get; set; } = "";
    }
}
