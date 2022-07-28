using Microsoft.Extensions.Configuration;
using System.Data;
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
        public const string GästezimmerName = "Gästezimmer";
        public const int GästezimmerVolume = 1;
        public const string SchlafzimmerName = "Schlafzimmer";
        public const int SchlafzimmerVolume = 9;
        public const string IanzimmerName = "Ian";
        public const int IanzimmerVolume = 10;
        public const string FinnzimmerName = "Finn";
        public const int FinnzimmerVolume = 10;
        public const string ArbeitszimmerName = "Arbeit";
        public const int ArbeitzimmerVolume = 10;
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

        //public static Dictionary<string, string> MusicPictureHashes { get; set; } = new();
        public static DataTable MusicPictureHashes { get; set; } = new();

        public static IConfiguration Configuration { get; set; }


        /// <summary>
        /// Entfernt vom übergebenen link die Version Parameter wie &v=xxx
        /// Der Parameter muss am ende stehen. 
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public static String RemoveVersionInUri(string cover)
        {
            if (string.IsNullOrEmpty(cover)) return "";
            if (!cover.Contains("&")) return cover;
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
                _uri = _uri.Replace("getaa?u=" + SonosConstants.xfilecifs, "");
                _uri = SonosConstants.RemoveVersionInUri(_uri);
                _uri = _uri.Replace("/", @"\");
                return _uri.Replace("\\\\\\", "\\\\");

            }
            catch
            {
                return _uri;
            }
        }
    }
}