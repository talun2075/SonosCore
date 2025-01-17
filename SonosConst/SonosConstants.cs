using System.Web;

namespace SonosConst
{
    public static class SonosConstants
    {
        private const int relativerVolWert = 10;
        public const string KücheName = "Küche";
        public const int KücheVolume = relativerVolWert + 4;
        public const string EsszimmerName = "Esszimmer";
        public const int EsszimmerVolume = relativerVolWert + 11;
        public const string WohnzimmerName = "Wohnzimmer";
        public const int WohnzimmerVolume = relativerVolWert + 64;
        public const string SpielzimmerName = "Spielen";
        public const int SpielzimmerVolume = 1;
        public const string SchlafzimmerName = "Schlafen";
        public const int SchlafzimmerVolume = 9;
        public const string IanzimmerName = "Ian";
        public const int IanzimmerVolume = 6;
        public const string FinnzimmerName = "Finn";
        public const int FinnzimmerVolume = 6;
        public const string ArbeitszimmerName = "Arbeit";
        public const int ArbeitzimmerVolume = 15;
        public const string defaultPlaylist = "3 Sterne Beide";
        /// <summary>
        /// Wird genommen, wenn man dem Player sagen möchte, dass er eine Playlist von einem Player (sich selber eingeschlossen) abspielen soll.
        /// </summary>
        public const string xrinconqueue = "x-rincon-queue:";
        /// <summary>
        /// Verweise vom Sonos auf das Filesystem. 
        /// </summary>
        public const string xfilecifs = "x-file-cifs:";
        /// <summary>
        /// Wird benutzt um Bei Kommunikation zwischen Client und Server Dummy WErte zu übergeben
        /// </summary>
        public const string empty = "leer";
        /// <summary>
        /// Ist ein Stream von einem Player z.B: bei Audio Eingang
        /// </summary>
        public const string xrinconstream = "x-rincon-stream:";
        /// <summary>
        /// Ist ein Stream von externer Quelle wie z.B. Radio
        /// </summary>
        public const string xsonosapistream = "x-sonosapi-stream";

        public const string xrinconplaylist ="x-rincon-playlist:";

        public const string xsonoshttp = "x-sonos-http";
        public const string AudioEingang = "Audio Eingang";
        /// <summary>
        /// Fürs Browsen der Favoriten
        /// </summary>
        public const string FV2 = "FV:2";
        /// <summary>
        /// Fürs Browsen von Sonos Playlists
        /// </summary>
        public const string SQ = "SQ:";
        /// <summary>
        /// Wird genutzt um einen Player einen anderen zuzuführen.
        /// </summary>
        public const string xrincon = "x-rincon:";
        /// <summary>
        /// Fürs Browsen nach Genres
        /// </summary>
        public const string aGenre = "A:GENRE";
        /// <summary>
        /// Fürs Browsen nach Interpreten
        /// </summary>
        public const string aAlbumArtist = "A:ALBUMARTIST";
        /// <summary>
        /// Browsing for Tracks
        /// </summary>
        public const string aTracks = "A:TRACKS";
        /// <summary>
        /// Browsen nach Playlisten
        /// </summary>
        public const string aPlaylists = "A:PLAYLISTS";
        /// <summary>
        /// Erster Wert beim Browsen
        /// </summary>
        public const string aALL = "All";
        /// <summary>
        /// Nicht implementiert
        /// </summary>
        public const string NotImplemented = "NOT_IMPLEMENTED";
        /// <summary>
        /// Wird verwendet beim Sleeptimer
        /// </summary>
        public const string Off = "aus";
        /// <summary>
        /// Ist der Wert des Sleeptimers, der kommt, wenn deaktiviert.
        /// </summary>
        public const string SleepTimerOffValueFromServer = "00:00:00";

        public const string CoverHashPathForBrowser = "/hashimages/"; //todo: appconfig.json
        /// <summary>
        /// Entfernt vom übergebenen link die Version Parameter wie &v=xxx
        /// Der Parameter muss am ende stehen. 
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public static String RemoveVersionInUri(string cover)
        {
            if (string.IsNullOrEmpty(cover)) return "";
            if (!cover.Contains('&')) return cover;
            var sublen = cover.Length - (cover.Length - cover.LastIndexOf("&"));
            cover = cover.Substring(0, sublen);
            return cover;
        }

        /// <summary>
        /// Für ein Album Cover den Pfad zur Datei erstellen
        /// </summary>
        /// <param name="_uri"></param>
        /// <returns></returns>
        public static String AlbumArtToFile(string _uri)
        {
            try
            {
                if (string.IsNullOrEmpty(_uri)) return String.Empty;
                _uri = HttpUtility.UrlDecode(_uri);
                _uri = Uri.UnescapeDataString(_uri);
                _uri = _uri.Replace("getaa?u=" + xfilecifs, "");
                _uri = RemoveVersionInUri(_uri);
                _uri = _uri.Replace("/", @"\");
                return _uri.Replace("\\\\\\", "\\\\");

            }
            catch
            {
                return _uri;
            }
        }

        /// <summary>
        /// Der Übergebene Container wird, zu einer gültigen URI. 
        /// </summary>
        /// <param name="_cont">Container ID die zu einer URI werden soll.</param>
        /// <param name="playerid">Id des SonosPlayers in der Liste um die UUID zu bestimmen</param>
        /// <returns>URI</returns>
        public static String ContainertoURI(string _cont, string playerid)
        {
            //Kein Filter angesetzt
            string rinconpl = String.Empty;
            if (_cont.StartsWith("S:"))
            {
                rinconpl = _cont.Replace("S:", xfilecifs); //Playlist
            }
            if (_cont.StartsWith(xfilecifs))
            {
                rinconpl = _cont; //Song
            }
            if (String.IsNullOrEmpty(rinconpl))
            {
                rinconpl = xrinconplaylist + playerid + "#" + _cont; //Container
            }
            return rinconpl;
        }
        /// <summary>
        /// Ersetzt den Pfad für die MP3 Verarbeitung
        /// </summary>
        /// <param name="_uri"></param>
        /// <returns></returns>
        public static String URItoPath(string _uri)
        {
            try
            {
                if (string.IsNullOrEmpty(_uri)) return String.Empty;
                _uri = _uri.Replace(SonosConstants.xfilecifs, "");
                _uri = Uri.UnescapeDataString(_uri);
                return _uri.Replace("/", "\\");
            }
            catch
            {
                return _uri;
            }
        }
    }
}