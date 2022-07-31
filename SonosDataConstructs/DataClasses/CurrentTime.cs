namespace SonosData.DataClasses
{
    /// <summary>
    /// Klasse die vom Sonos geliefert wird, wenn eine 
    /// </summary>
    public class CurrentTime
    {
        /// <summary>
        /// UTC Zeitformat
        /// </summary>
        public string CurrentUTCTime { get; set; } = string.Empty;
        /// <summary>
        /// Berechnet auf Lokal
        /// </summary>
        public string CurrentLocalTime { get; set; } = string.Empty;
        /// <summary>
        /// Aktuelle Zeitzone unbekanntes Format
        /// </summary>
        public string CurrentTimeZone { get; set; } = string.Empty;

        public SonosTimeZoneData TimeZoneData { get; set; } = new();
        /// <summary>
        /// Index der Generationen
        /// </summary>
        public int CurrentTimeGeneration { get; set; } = 0;
    }
}
