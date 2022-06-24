using System;
using System.Collections.Generic;

namespace SonosUPnP.DataClasses
{
    public class DeviceCapabilities
    {
        public List<String> PlayMedia { get; set; }
        public String RecMedia { get; set; }
        public String RecQualityModes { get; set; }
    }
}
