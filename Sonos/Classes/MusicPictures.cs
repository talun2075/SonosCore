using SonosUPnP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sonos.Classes
{
    public static class MusicPictures
    {
        /*
         generierung /artist/genres/playlists
         bereitstellung liste mit pfaden
         replacement
         */

        #region Klassenvariablen
        private static readonly List<string> DBPrepareList = new();
        private static readonly Dictionary<string, string> DBPathCheckSum = new();
        #endregion
        #region PublicMethoden
        /// <summary>
        /// Läd artist/genres/playlistpfade und füllt DBPrepareList
        /// Ruft Danach ReplaceAlbumArt und Update DB auf
        /// </summary>
        public static async Task<Boolean> GenerateDBContent()
        {
            await SonosHelper.CheckSonosLiving();
            List<SonosItem> genres = await SonosHelper.Sonos.ZoneMethods.Browsing(SonosHelper.Sonos.Players.First(), SonosConstants.aGenre, false);
            List<SonosItem> artist = await SonosHelper.Sonos.ZoneMethods.Browsing(SonosHelper.Sonos.Players.First(), SonosConstants.aAlbumArtist, false);
            List<SonosItem> playlists = await SonosHelper.Sonos.ZoneMethods.Browsing(SonosHelper.Sonos.Players.First(), SonosConstants.aPlaylists, false);
            var allpl = genres.Union(artist).Union(playlists);
            await RunIntoList(allpl);
            ReplaceAlbumArt();
            await UpdateDB();
            return true;
        }
        public async static Task<Boolean> GetDBContent()
        {
            SonosHelper.MusicPictureHashes = await DatabaseWrapper.ReadHashes();
            return true;
        }
        /// <summary>
        /// verarbeitet eine Liste von items zu Hashwerten.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public async static Task<List<SonosItem>> UpdateItemListToHashPath(List<SonosItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    try
                    {
                        await UpdateItemToHashPath(item);
                    }
                    catch(Exception ex)
                    {
                        SonosHelper.Logger.ServerErrorsAdd("UpdateItemListToHashPath", ex, "MusicPictures");
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("UpdateItemListToHashPath2", ex, "MusicPictures");
            }

            return items;
        }
        public async static Task<SonosItem> UpdateItemToHashPath(SonosItem item)
        {
            if (!SonosHelper.MusicPictureHashes.Any()) await GetDBContent();
            if (string.IsNullOrEmpty(item.AlbumArtURI) || item.AlbumArtURI.StartsWith(SonosConstants.CoverHashPathForBrowser)) return item;
            var covershort = SonosItemHelper.RemoveVersionInUri(item.AlbumArtURI);
            if(SonosHelper.MusicPictureHashes.TryGetValue(covershort, out string hash))
            {
                item.AlbumArtURI = SonosConstants.CoverHashPathForBrowser + hash + ".png";
            }
            return item;
        }
        #endregion PublicMethoden
        #region PrivateMethoden

        /// <summary>
        /// Durchläuft die Liste inkl Kindelemente und fügt SonoItems mit Cover der DBPrepareList zu.
        /// </summary>
        /// <param name="lis"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<SonosItem>> RunIntoList(IEnumerable<SonosItem> lis)
        {
            foreach (SonosItem item in lis)
            {
                if (!string.IsNullOrEmpty(item.AlbumArtURI) && !DBPrepareList.Contains(item.AlbumArtURI))
                    DBPrepareList.Add(item.AlbumArtURI);
                //Datenladen. 
                if (!string.IsNullOrEmpty(item.ContainerID) && item.Title != SonosConstants.aALL)
                    await RunIntoList(await SonosHelper.Sonos.ZoneMethods.Browsing(SonosHelper.Sonos.Players.First(), item.ContainerID, false));
            }
            return lis;
        }
        /// <summary>
        /// Durchgeht die DBPreparelist und liest die Checksum pro Cover aus.
        /// </summary>
        private static void ReplaceAlbumArt()
        {
            //übergebene liste durchlaufen und entsprechend die pfade anpassen.
            //evtl weiter granulieren, das hier nur durchlaufen wird und in einer anderen methode die pfade angepasst wird. 

            foreach (string item in DBPrepareList)
            {
                var cover = item;
                var withoutVersion = SonosItemHelper.RemoveVersionInUri(cover);
                if (!DBPathCheckSum.ContainsKey(withoutVersion))
                {
                    //Wert nicht vorhanden somit laden.
                    var fixedpath = SonosItemHelper.AlbumArtToFile(cover);
                    if (File.Exists(fixedpath))
                    {
                        try
                        {
                            if (fixedpath.EndsWith(".aiff")) continue;
                            var hash = MP3.TagLibDelivery.GetPictureHash(fixedpath);
                            DBPathCheckSum.Add(withoutVersion, hash);
                        }
                        catch(Exception ex)
                        {
                            SonosHelper.Logger.ServerErrorsAdd("MusicPictures.ReplaceAlbumArt:"+fixedpath, ex);
                            continue;
                        }
                    }
                }
            }
        }
        private async static Task<Boolean> UpdateDB()
        {
            SonosHelper.MusicPictureHashes = DBPathCheckSum;
            return await DatabaseWrapper.UpdateHashes(DBPathCheckSum);
        }
        #endregion PrivateMethoden

    }
}
