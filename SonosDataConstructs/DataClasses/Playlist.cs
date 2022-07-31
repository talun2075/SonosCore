namespace SonosData.DataClasses
{
    /// <summary>
    /// Sonos CurrentPlaylist eines Players
    /// </summary>
    public class Playlist
    {
        public int NumberReturned { get; set; } = 0;
        public int TotalMatches { get; set; } = 0;
        public bool IsEmpty
        {
            get
            {
                return PlayListItems.Count == 0;
            }
        }
        public bool PlayListItemsHashChecked { get; set; } = false;
        public List<SonosItem> PlayListItems { get; } = new List<SonosItem>();
        /// <summary>
        /// Referenz das als Playlist genommen wird.
        /// </summary>
        public string FillPlaylistObject { get; set; } = BrowseObjects.CurrentPlaylist;
        /// <summary>
        /// Resetet die Playlist komplett
        /// </summary>
        public void ResetPlaylist()
        {
            FillPlaylistObject = BrowseObjects.CurrentPlaylist;
            NumberReturned = 0;
            TotalMatches = 0;
            PlayListItems.Clear();
        }
    }
}