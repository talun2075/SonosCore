namespace SonosData.DataClasses
{
    public class BrowseResults
    {
        /// <summary>
        /// Liste der Gefundenen Elemente
        /// </summary>
        public List<SonosItem> Result { get; set; } = new List<SonosItem>();
        /// <summary>
        /// Anzahl der gelieferten Ergebnisse
        /// </summary>
        public int NumberReturned { get; set; }
        /// <summary>
        /// Anzahl der gesamten Ergebnisse
        /// </summary>
        public int TotalMatches { get; set; }
        public ushort UpdateID { get; set; }
    }
}
