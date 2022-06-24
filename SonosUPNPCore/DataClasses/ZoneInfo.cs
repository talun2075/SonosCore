using System;

namespace SonosUPnP.DataClasses
{
    public class ZoneInfo
    {
        public String SerialNumber { get; set; }
        public String SoftwareVersion { get; set; }
        public String DisplaySoftwareVersion { get; set; }
        public String HardwareVersion { get; set; } = String.Empty;
        public String IPAddress { get; set; }
        public String MACAddress { get; set; }
        public String CopyrightInfo { get; set; }
        public String ExtraInfo { get; set; }
        public String Flags { get; set; }
        public String HTAudioIn { get; set; }
    }
}
