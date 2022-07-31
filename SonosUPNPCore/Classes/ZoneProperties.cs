using SonosData;
using SonosData.DataClasses;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SonosUPNPCore.Classes
{
    public class ZoneProperties
    {
        #region AlarmClock
        /// <summary>
        /// Datumsformat: YMD / MDY / DMY
        /// </summary>
        public string DateFormat { get; set; } = string.Empty;
        /// <summary>
        /// Zeitformat 12H oder 24H
        /// </summary>
        public string TimeFormat { get; set; } = string.Empty;
        /// <summary>
        /// Unbekannt
        /// </summary>
        public int TimeGeneration { get; set; } = 0;
        /// <summary>
        /// Wenn leer wird die Zeit Manuell gestellt ansonsten vom Server geholt.
        /// </summary>
        public string TimeServer { get; set; } = string.Empty;
        /// <summary>
        /// InternalString der SonosTimeZone Klasse
        /// </summary>
        public string TimeZone { get; set; } = string.Empty;
        /// <summary>
        /// Index AKtualisierungszeitpunkt im Format HH:MM:SS
        /// </summary>
        public string DailyIndexRefreshTime { get; set; } = string.Empty;
        /// <summary>
        /// Wird bei Änderungen hochgezählt
        /// </summary>
        public int AlarmListVersion { get; set; } = 0;
        /// <summary>
        /// Aktuell hinterlegte Zeit im System.
        /// </summary>
        public CurrentTime CurrentSonosTime { get; set; } = new CurrentTime();
        #endregion AlarmClock
        #region Eigene
        [IgnoreDataMember]
        public List<Alarm> ListOfAlarms { get; set; } = new List<Alarm>();
        /// <summary>
        /// Liste Aller Favoriten
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfFavorites { get; set; } = new List<SonosItem>();

        /// <summary>
        /// Liste der SonosWiedergabeliste
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfSonosPlaylist { get; set; } = new List<SonosItem>();
        /// <summary>
        /// Liste der Importierten Wiedergabelisten
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfImportedPlaylist { get; set; } = new List<SonosItem>();
        /// <summary>
        /// Liste der Sonos und Importierten Wiedergabelisten
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfAllPlaylist
        {
            get
            {
                var k = ListOfImportedPlaylist.Union(ListOfSonosPlaylist).ToList();
                foreach (SonosItem si in k)
                {
                    if (si.Uri.Contains(".m3u"))
                    {
                        si.Description = "M3U";
                    }
                    else
                    {
                        si.Description = "Sonos";
                    }
                    if (si.Title.EndsWith(".m3u"))
                    {
                        si.Title = si.Title.Replace(".m3u", "");
                    }
                }
                return k.OrderBy(x => x.Title).ToList();
            }
        }
        public int SavedQueuesUpdateID { get; set; } = 0;
        public int ShareListUpdateID { get; set; } = 0;
        public bool ShareIndexInProgress { get; set; } = false;
        public string ShareIndexLastError { get; set; } = string.Empty;
        public int FavoritesUpdateID { get; set; } = 0;
        /// <summary>
        /// Liste aller Playlisten gefüllt mit Einträgen.
        /// </summary>
        [IgnoreDataMember]
        public List<Playlist> ListOfAllFilledPlaylist { get; set; } = new List<Playlist>();
        /// <summary>
        /// Liste der Zonen
        /// </summary>
        public ZoneGroupStateList ZoneGroupState { get; set; } = new ZoneGroupStateList();
        /// <summary>
        /// Pro Player ein Dictionary mit den URI´s und der Song der Abgespielt wurde.
        /// Dient dazu beim erneuten Aufruf den Song entsprechend zu setzen.
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> PlayerPlayedPlaylist { get; set; } = new Dictionary<string, Dictionary<string, int>>();
        #endregion Eigene
    }
}
