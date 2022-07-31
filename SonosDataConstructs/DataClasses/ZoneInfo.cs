namespace SonosData.DataClasses
{
    public class ZoneInfo
    {
        public string SerialNumber { get; set; } = "";
        public string SoftwareVersion { get; set; } = "";
        public string DisplaySoftwareVersion { get; set; } = "";
        public string HardwareVersion { get; set; } = string.Empty;
        public string IPAddress { get; set; } = "";
        public string MACAddress { get; set; } = "";
        public string CopyrightInfo { get; set; } = "";
        public string ExtraInfo { get; set; } = "";
        public string Flags { get; set; } = "";
        public string HTAudioIn { get; set; } = "";
    }
}
