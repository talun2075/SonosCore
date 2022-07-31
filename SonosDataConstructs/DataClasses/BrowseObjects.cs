namespace SonosData.DataClasses
{
    public static class BrowseObjects
    {
        /// <summary>
        /// Aktuelle Wiedergabeliste
        /// </summary>
        public static string CurrentPlaylist => "Q:0";
        /// <summary>
        /// Favoriten auslesen
        /// </summary>
        public static string Favorites => "FV:2";
        /// <summary>
        /// Sonos Wiedergabelisten
        /// </summary>
        public static string SonosPlaylist => "SQ:";
        /// <summary>
        /// Importierte Wiedergabelisten
        /// </summary>
        public static string ImportetPlaylist => "A:PLAYLISTS";
        /// <summary>
        /// Alle Shares
        /// </summary>
        public static string Shares => "S:";
        /// <summary>
        /// Auflistung für alle Listen wie Artist, Genre oder Album
        /// </summary>
        public static string SearchRoot => "A:";

    }
}
