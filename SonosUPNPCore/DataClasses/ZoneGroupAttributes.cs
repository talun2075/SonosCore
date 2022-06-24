using System;
using System.Collections.Generic;

namespace SonosUPnP.DataClasses
{
    public class ZoneGroupAttributes
    {
        /// <summary>
        /// Name der Gruppe
        /// </summary>
        public String GroupName { get; set; }
        /// <summary>
        /// ID der Lokalen Gruppe
        /// </summary>
        public String GroupID { get; set; }
        /// <summary>
        /// Liste mit allen Playern
        /// </summary>
        public List<String> ZonePlayerUUID { get; set; }
        public String MuseHouseholdId { get; set; }
    }
}
