namespace SonosData.DataClasses
{
    public class TopologyChange
    {
        public int SoftwareVersion { get; set; }
        public bool ActiveSubscription { get; set; } = false;
        public bool UseAlarmClock { get; set; } = false;
        public bool UseMediaServer { get; set; } = false;
    }
}
