using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SonosConst;
using SonosData.DataClasses;
using SonosData;
using SonosData.Enums;

namespace SonosUPNPCore.Classes
{
    public class DiscoveryZoneProperties
    {
        private static ZonePerSoftwareGeneration ZPG1 = new();
        private static ZonePerSoftwareGeneration ZPG2 = new();
        public DiscoveryZoneProperties(ZonePerSoftwareGeneration zpg1, ZonePerSoftwareGeneration zpg2)
        {
            ZPG1 = zpg1;
            ZPG2 = zpg2;
        }

        #region AlarmClock
        /// <summary>
        /// Datumsformat: YMD / MDY / DMY
        /// </summary>
        public Dictionary<SoftwareGeneration, string> DateFormat
        {
            get
            {
                return new Dictionary<SoftwareGeneration, string>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.DateFormat }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.DateFormat } };
            }
        }
        /// <summary>
        /// Zeitformat 12H oder 24H
        /// </summary>

        public Dictionary<SoftwareGeneration, string> TimeFormat
        {
            get
            {
                return new Dictionary<SoftwareGeneration, string>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.TimeFormat }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.TimeFormat } };
            }
        }
        /// <summary>
        /// Unbekannt
        /// </summary>
        public Dictionary<SoftwareGeneration, int> TimeGeneration
        {
            get
            {
                return new Dictionary<SoftwareGeneration, int>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.TimeGeneration }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.TimeGeneration } };
            }
        }
        /// <summary>
        /// Wenn leer wird die Zeit Manuell gestellt ansonsten vom Server geholt.
        /// </summary>
        public Dictionary<SoftwareGeneration, string> TimeServer
        {
            get
            {
                return new Dictionary<SoftwareGeneration, string>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.TimeServer }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.TimeServer } };
            }
        }
        /// <summary>
        /// InternalString der SonosTimeZone Klasse
        /// </summary>
        public Dictionary<SoftwareGeneration, string> TimeZone
        {
            get
            {
                return new Dictionary<SoftwareGeneration, string>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.TimeZone }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.TimeZone } };
            }
        }
        /// <summary>
        /// Index AKtualisierungszeitpunkt im Format HH:MM:SS
        /// </summary>
        public Dictionary<SoftwareGeneration, string> DailyIndexRefreshTime
        {
            get
            {
                return new Dictionary<SoftwareGeneration, string>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.DailyIndexRefreshTime }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.DailyIndexRefreshTime } };
            }
        }
        /// <summary>
        /// Wird bei Änderungen hochgezählt
        /// </summary>
        public Dictionary<SoftwareGeneration, int> AlarmListVersion
        {
            get
            {
                return new Dictionary<SoftwareGeneration, int>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.AlarmListVersion }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.AlarmListVersion } };
            }
        }
        /// <summary>
        /// Aktuell hinterlegte Zeit im System.
        /// </summary>
        public Dictionary<SoftwareGeneration, CurrentTime> CurrentSonosTime
        {
            get
            {
                return new Dictionary<SoftwareGeneration, CurrentTime>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.CurrentSonosTime }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.CurrentSonosTime } };
            }
        }
        #endregion AlarmClock
        #region Eigene
        [IgnoreDataMember]
        public List<Alarm> ListOfAlarms
        {
            get
            {
                List<Alarm> retval = ZPG1.ZoneProperties.ListOfAlarms;
                foreach (Alarm item in ZPG2.ZoneProperties.ListOfAlarms)
                {
                    var tempitem = ZPG1.ZoneProperties.ListOfAlarms.FirstOrDefault(x => x.ID == item.ID);
                    if (tempitem == null)
                    {
                        retval.Add(item);
                    }
                }
                return retval;
            }
        }
        /// <summary>
        /// Liste Aller Favoriten
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfFavorites
        {
            get
            {
                List<SonosItem> retval = ZPG1.ZoneProperties.ListOfFavorites;
                foreach (SonosItem item in ZPG2.ZoneProperties.ListOfFavorites)
                {
                    var tempitem = ZPG1.ZoneProperties.ListOfFavorites.FirstOrDefault(x => x.ContainerID == item.ContainerID && x.ItemID == item.ItemID);
                    if (tempitem == null)
                    {
                        retval.Add(item);
                    }
                }
                return retval;
            }
        }
        /// <summary>
        /// Liefert die Software Version um einen entsprechenden Player raus zu holen bei Playlisten, die per S1 oder S2 aufgeteillt sind.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public SoftwareGeneration GetSoftwareVersionForPlaylistEntry(string val)
        {
            if (val.StartsWith(SonosConstants.FV2))
            {
                var pl = ZPG1.ZoneProperties.ListOfFavorites.FirstOrDefault(x => x.ItemID == val);
                if (pl == null) return SoftwareGeneration.ZG2;
            }

            if (val.StartsWith(SonosConstants.SQ))
            {
                var pl = ZPG1.ZoneProperties.ListOfSonosPlaylist.FirstOrDefault(x => x.ContainerID == val);
                if (pl == null) return SoftwareGeneration.ZG2;
            }
            return SoftwareGeneration.ZG2;
        }
        /// <summary>
        /// Liste der SonosWiedergabeliste
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfSonosPlaylist
        {
            get
            {
                List<SonosItem> retval = ZPG1.ZoneProperties.ListOfSonosPlaylist;
                foreach (SonosItem item in ZPG2.ZoneProperties.ListOfSonosPlaylist)
                {
                    var tempitem = ZPG1.ZoneProperties.ListOfSonosPlaylist.FirstOrDefault(x => x.ContainerID == item.ContainerID && x.ItemID == item.ItemID);
                    if (tempitem == null)
                    {
                        retval.Add(item);
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Liste der Importierten Wiedergabelisten
        /// </summary>
        [IgnoreDataMember]
        public List<SonosItem> ListOfImportedPlaylist
        {
            get
            {
                List<SonosItem> retval = ZPG1.ZoneProperties.ListOfImportedPlaylist;
                foreach (SonosItem item in ZPG2.ZoneProperties.ListOfImportedPlaylist)
                {
                    var tempitem = ZPG1.ZoneProperties.ListOfImportedPlaylist.FirstOrDefault(x => x.ContainerID == item.ContainerID && x.ItemID == item.ItemID);
                    if (tempitem == null)
                    {
                        retval.Add(item);
                    }
                }
                return retval;
            }
        }

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
        public Dictionary<SoftwareGeneration, int> SavedQueuesUpdateID
        {
            get
            {
                return new Dictionary<SoftwareGeneration, int>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.SavedQueuesUpdateID }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.SavedQueuesUpdateID } };
            }
        }
        public Dictionary<SoftwareGeneration, int> ShareListUpdateID
        {
            get
            {
                return new Dictionary<SoftwareGeneration, int>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.ShareListUpdateID }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.ShareListUpdateID } };
            }
        }

        public bool ShareIndexInProgress { get; set; } = ZPG1.ZoneProperties.ShareIndexInProgress == true || ZPG2.ZoneProperties.ShareIndexInProgress == true;
        public Dictionary<SoftwareGeneration, string> ShareIndexLastError
        {
            get
            {
                return new Dictionary<SoftwareGeneration, string>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.ShareIndexLastError }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.ShareIndexLastError } };
            }
        }

        public Dictionary<SoftwareGeneration, int> FavoritesUpdateID
        {
            get
            {
                return new Dictionary<SoftwareGeneration, int>() { { SoftwareGeneration.ZG1, ZPG1.ZoneProperties.FavoritesUpdateID }, { SoftwareGeneration.ZG2, ZPG2.ZoneProperties.FavoritesUpdateID } };
            }
        }
        /// <summary>
        /// Liste aller Playlisten gefüllt mit Einträgen. (Unabhängig der Software Gen)
        /// </summary>
        [IgnoreDataMember]
        //public List<Playlist> ListOfAllFilledPlaylist { get; set; } = new List<Playlist>();
        /// <summary>
        /// Liste der Zonen (Unabhängig der Software Gen)
        /// </summary>
        public ZoneGroupStateList ZoneGroupState { get; set; } = new ZoneGroupStateList();
        /// <summary>
        /// Pro Player ein Dictionary mit den URI´s und der Song der Abgespielt wurde.
        /// Dient dazu beim erneuten Aufruf den Song entsprechend zu setzen.
        /// (Unabhängig der Software Gen)
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> PlayerPlayedPlaylist { get; set; } = new Dictionary<string, Dictionary<string, int>>();
        #endregion Eigene
    }
}
