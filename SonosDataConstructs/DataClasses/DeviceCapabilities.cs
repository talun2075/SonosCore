namespace SonosData.DataClasses
{
    public class DeviceCapabilities
    {
        public List<string> PlayMedia { get; set; } = new();
        public string RecMedia { get; set; } = "";
        public string RecQualityModes { get; set; } = "";
    }
}
