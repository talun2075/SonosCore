using SonosUPnP;
using SonosConst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SonosSQLiteWrapper.Interfaces;
using HomeLogging;
using Sonos.Classes.Interfaces;

namespace Sonos.Classes
{
    public class MusicPictures : IMusicPictures
    {
        #region PublicMethoden
        private readonly ISQLiteWrapper sw;
        private readonly List<String> CoverPaths = new();
        private readonly ILogging _logging;
        private readonly ISonosDiscovery _sonos;

        public MusicPictures(ISQLiteWrapper sQLiteWrapper, ILogging logging, ISonosDiscovery sonos)
        {
            sw = sQLiteWrapper;
            SonosConstants.MusicPictureHashes = sw.GetMusicPictures();
            _logging = logging;
             _sonos = sonos;
        }

        public async Task<Boolean> GenerateDBContent()
        {
            List<SonosItem> tracks = await _sonos.ZoneMethods.Browsing(_sonos.Players.First(), SonosConstants.aTracks, false);
            RunIntoList(tracks);
            UpdateImagesToDatabase();
            return true;
        }
        private void UpdateImagesToDatabase()
        {
            var dbvalues = sw.GetMusicPictures();
            var pathCn = dbvalues.Columns[0].ColumnName;
            var hashCn = dbvalues.Columns[1].ColumnName;
            Boolean changes = false;
            foreach (string item in CoverPaths)
            {
                if (string.IsNullOrEmpty(item)) continue;
                var covernoversion = SonosConstants.RemoveVersionInUri(item);
                if (dbvalues.Rows.Contains(covernoversion)) continue;//vorhanden, daher weiter machen.
                changes = true;
                //GetHash
                var fixedpath = SonosConstants.AlbumArtToFile(item);
                string hash = "";
                if (File.Exists(fixedpath))
                {
                    try
                    {
                        if (fixedpath.EndsWith(".aiff")) continue;
                        hash = MP3.TagLibDelivery.GetPictureHash(fixedpath);
                    }
                    catch (Exception ex)
                    {
                        _logging.ServerErrorsAdd("MusicPictures.ReplaceAlbumArt:" + fixedpath, ex,"MusicPictures");
                        continue;
                    }
                    //insert new row
                    var row = dbvalues.NewRow();
                    row[pathCn] = covernoversion;
                    row[hashCn] = hash;//Hash ermitteln
                    dbvalues.Rows.Add(row);
                }
            }
            if (changes)
            {
                sw.Update();
                SonosConstants.MusicPictureHashes = sw.GetMusicPictures();
            }

        }
        /// <summary>
        /// verarbeitet eine Liste von items zu Hashwerten.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<SonosItem> UpdateItemListToHashPath(List<SonosItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    try
                    {
                        SonosItemHelper.UpdateItemToHashPath(item);
                    }
                    catch (Exception ex)
                    {
                        _logging.ServerErrorsAdd("UpdateItemListToHashPath", ex, "MusicPictures");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("UpdateItemListToHashPath2", ex, "MusicPictures");
            }

            return items;
        }

        #endregion PublicMethoden
        #region PrivateMethoden

        private void RunIntoList(IEnumerable<SonosItem> lis)
        {
            foreach (SonosItem item in lis)
            {
                if (!string.IsNullOrEmpty(item.AlbumArtURI) && !CoverPaths.Contains(item.AlbumArtURI))
                    CoverPaths.Add(item.AlbumArtURI);
            }
        }
        #endregion PrivateMethoden

    }
}
