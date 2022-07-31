namespace SonosData.DataClasses
{
    public class SonosTimeZoneData
    {
        /// <summary>
        /// ID in der Liste
        /// </summary>
        public int ID { get; set; } = -1;
        /// <summary>
        /// von Sonos Intern verwendeter String
        /// </summary>
        public string InternalString { get; set; } = "";
        /// <summary>
        /// Externer String, der Angezeigt wird
        /// </summary>
        public string ExternalString { get; set; } = "";
        /// <summary>
        /// Sommer Winterzeit
        /// </summary>
        public bool AutoAdjustDst { get; set; }
    }
}
