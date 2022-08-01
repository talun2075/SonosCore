using System.Xml.Linq;

namespace SonosData.DataClasses
{
    public class Alarm
    {
        /// <summary>
        /// ID des Weckers
        /// </summary>
        public uint ID { get; set; }
        /// <summary>
        /// Startzeit im Format HH:MM:SS
        /// </summary>
        public string StartTime { get; set; } = string.Empty;
        /// <summary>
        /// Dauer des Weckers im Format HH:MM:SS
        /// </summary>
        public string Duration { get; set; } = string.Empty;
        /// <summary>
        /// Die Häufigkeit des Weckers
        /// Erlaubt sind folgende:
        /// ONCE: Wenn das nächste mal die Startzeit erreicht wurde
        /// WEEKDAYS: Alle Werktage
        /// WEEKENDS: Samstag und Sonntag
        /// DAILY: Jeden tag
        /// ON_XXXXX: X Steht für den Wochentag als Zahl Beginnend mit 1
        /// 1=Montag
        /// 2=Dienstag
        /// 3=Mittwoch
        /// etc.
        /// Dienstags und Mittwochs wäre also ON_23
        /// </summary>
        public string Recurrence { get; set; } = string.Empty;
        /// <summary>
        /// Ist der Wecker aktiv?
        /// </summary>
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// UUID des Raums
        /// </summary>
        public string RoomUUID { get; set; } = string.Empty;
        /// <summary>
        /// Name des Raumes
        /// </summary>
        public string RoomName { get; set; } = string.Empty;
        /// <summary>
        /// URI was gestartet werden soll.
        /// Folgende Werte gehen:
        /// Sonos Alarmsignal = x-rincon-buzzer:0
        /// Sonos Playlist = z.B: file:///jffs/settings/savedqueues.rsq#48
        /// Importierte Playlist = z.B: x-file-cifs://NAS/Musik/Playlists/Aufwecken.m3u
        /// Radiosender = z.B: x-sonosapi-stream:s85114?sid=254&amp;flags=32
        /// </summary>
        public string ProgramURI { get; set; } = string.Empty;
        /// <summary>
        /// Kürzt die ProgramURI auf den Namen der Playlist.
        /// </summary>
        public string ProgramURIShort
        {
            get
            {
                if (ProgramURI.Contains('/') && ProgramURI.Contains('.'))
                {
                    int starttocut = ProgramURI.LastIndexOf("/") + 1;//34
                    int lastpartlength = ProgramURI.Length - ProgramURI.LastIndexOf(".");//4
                    int lengthtocut = ProgramURI.Length - starttocut - lastpartlength;
                    return ProgramURI.Substring(starttocut, lengthtocut);
                }
                return ProgramURI;
            }

        }
        /// <summary>
        /// Die Metadaten abhängig der ProgramURI
        /// </summary>
        public string ProgramMetaData { get; set; } = string.Empty;
        /// <summary>
        /// Die ContainerID der Playlist
        /// </summary>
        public string ContainerID { get; set; } = string.Empty;
        /// <summary>
        /// Wiedergabemodus
        /// Erlaubt sind folgende:
        /// NORMAL: Liste wird von oben nach unten gespielt.
        /// REPEAT_ALL: Liste wird immer wieder von oben nach unten gespielt
        /// SHUFFLE_NOREPEAT: Liste wird zufällig gespielt
        /// SHUFFLE: Liste wird zufällig immer und immer wieder gespielt
        /// </summary>
        public string PlayMode { get; set; } = string.Empty;
        /// <summary>
        /// Lautstärke 0-100
        /// </summary>
        public ushort Volume { get; set; }
        /// <summary>
        /// Sollen auch die anderen Player der Zone abgespielt werden?
        /// </summary>
        public bool IncludeLinkedZones { get; set; } = false;

        public static IList<Alarm> Parse(string xmlString)
        {
            var xml = XElement.Parse(xmlString);
            var items = xml.Elements("Alarm");
            var list = new List<Alarm>();

            foreach (var item in items)
            {
                var alarm = new Alarm();
                alarm.Duration = (string)item.Attribute("Duration") ?? "";
                alarm.StartTime = (string)item.Attribute("StartTime") ?? "";
                alarm.Recurrence = (string)item.Attribute("Recurrence") ?? "";
                alarm.RoomUUID = (string)item.Attribute("RoomUUID") ?? "";
                alarm.ProgramURI = (string)item.Attribute("ProgramURI") ?? "";
                alarm.ProgramMetaData = (string)item.Attribute("ProgramMetaData") ?? "";
                alarm.ContainerID = SonosItem.Parse(alarm.ProgramMetaData)[0].ItemID;
                alarm.PlayMode = (string)item.Attribute("PlayMode") ?? "";
                alarm.Volume = Convert.ToUInt16((string)item.Attribute("Volume"));
                if ((string)item.Attribute("IncludeLinkedZones") != null && (string)item.Attribute("IncludeLinkedZones") =="1")
                {
                    alarm.IncludeLinkedZones = true;
                }
                if ((string)item.Attribute("Enabled") != null && (string)item.Attribute("Enabled") == "1")
                {
                    alarm.Enabled = true;
                }
                alarm.ID = Convert.ToUInt16((string)item.Attribute("ID"));
                list.Add(alarm);
            }

            return list;
        }

    }
}
