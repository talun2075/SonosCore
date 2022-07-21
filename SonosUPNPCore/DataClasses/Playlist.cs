using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonosUPnP.DataClasses
{
    /// <summary>
    /// Sonos CurrentPlaylist eines Players
    /// </summary>
    public class Playlist
    {
        public int NumberReturned { get; set; } = 0;
        public int TotalMatches { get; set; } = 0;
        public Boolean IsEmpty
        {
            get
            {
                return PlayListItems.Count == 0;
            }
        }
        public Boolean PlayListItemsHashChecked { get; set; } = false;
        public List<SonosItem> PlayListItems { get; } = new List<SonosItem>();
        /// <summary>
        /// Referenz das als Playlist genommen wird.
        /// </summary>
        public string FillPlaylistObject { get; set; } = BrowseObjects.CurrentPlaylist;
        /// <summary>
        /// Füllt die Wiedergabeliste aufgrund des Übergebenen Players
        /// </summary>
        /// <param name="pl"></param>
        public async Task<Boolean> FillPlaylist(SonosPlayer pl)
        {

            Boolean retval = true;
            try
            {
                TotalMatches = -1;
                NumberReturned = 0;
                while ((NumberReturned != TotalMatches || TotalMatches == -1) && pl.ContentDirectory != null)
                {
                    var browseresults = await pl.ContentDirectory.Browse(FillPlaylistObject, NumberReturned);
                    NumberReturned += browseresults.NumberReturned;
                    if (browseresults.TotalMatches > 0)
                    {
                        TotalMatches = browseresults.TotalMatches;
                    }
                    if (browseresults.Result.Any())
                    {
                        PlayListItems.AddRange(browseresults.Result);
                    }
                    else
                    {
                        break; //kein Ergebnis, daher abbrechen.
                    }
                }
            }
            catch (Exception ex)
            {
                pl.ServerErrorsAdd("Playlist:FillPlaylist:Block1", "Playlist", ex);
                retval = false;
            }

            //Eintrag in der Liste vorhanden
            if (TotalMatches == 0 && PlayListItems.Count == 0)
            {
                PlayListItems.Add(new SonosItem() { Album = SonosConstants.empty, Artist = SonosConstants.empty, Title = SonosConstants.empty });
                return false;
            }
            return retval;
        }
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