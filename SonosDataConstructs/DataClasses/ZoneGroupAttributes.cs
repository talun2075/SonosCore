namespace SonosData.DataClasses
{
    public class ZoneGroupAttributes
    {
        /// <summary>
        /// Name der Gruppe
        /// </summary>
        public string GroupName { get; set; } = "";
        /// <summary>
        /// ID der Lokalen Gruppe
        /// </summary>
        public string GroupID { get; set; } = "";
        /// <summary>
        /// Liste mit allen Playern
        /// </summary>
        public List<string> ZonePlayerUUID { get; set; } = new();
        public string MuseHouseholdId { get; set; } = "";
    }
}
