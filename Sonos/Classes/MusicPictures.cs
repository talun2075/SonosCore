using SonosUPnP;
using SonosConst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SonosSQLite;

namespace Sonos.Classes
{
    public static class MusicPictures
    {
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
            await DatabaseWrapper.UpdateDB(SonosHelper.Logger);
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
                        await SonosItemHelper.UpdateItemToHashPath(item);
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
                if (!string.IsNullOrEmpty(item.AlbumArtURI) && !DatabaseWrapper.DBPrepareList.Contains(item.AlbumArtURI))
                    DatabaseWrapper.DBPrepareList.Add(item.AlbumArtURI);
                //Datenladen. 
                if (!string.IsNullOrEmpty(item.ContainerID) && item.Title != SonosConstants.aALL)
                    await RunIntoList(await SonosHelper.Sonos.ZoneMethods.Browsing(SonosHelper.Sonos.Players.First(), item.ContainerID, false));
            }
            return lis;
        }
        #endregion PrivateMethoden

    }
}
