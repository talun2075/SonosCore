namespace SonosUPnP.DataClasses
{
    public class MediaInfo
    {
        public int NumberOfTracks { get; set; } = 0;
        public string URI { get; set; } = string.Empty;
        public string URIMetaData { get; set; } = string.Empty;
        public string NextURI { get; set; } = string.Empty;
        public string PlayMedium { get; set; } = string.Empty;
        /// <summary>
        /// Prüft ob Inhalt vorhanden ist.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return NumberOfTracks == 0 && string.IsNullOrEmpty(URI) && string.IsNullOrEmpty(URIMetaData) && string.IsNullOrEmpty(NextURI) && string.IsNullOrEmpty(PlayMedium);
            }
        }
    }
}
