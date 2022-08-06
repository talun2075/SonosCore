using SonosConst;
using SonosSQLiteWrapper.Interfaces;
using HomeLogging;
using System.Data;
using SonosData;

namespace SonosSQLiteWrapper
{
    public class MusicPictures : IMusicPictures
    {
        #region PublicMethoden
        private readonly ISQLiteWrapper sw;
        private readonly List<string> CoverPaths = new();
        private readonly ILogging _logging;

        public MusicPictures(ISQLiteWrapper sQLiteWrapper, ILogging logging)
        {
            sw = sQLiteWrapper;
            _logging = logging;
        }
        public DataTable CurrentMusicPictures => sw.MusicPictures;
        public bool GenerateDBContent(List<SonosItem> tracks)
        {
            RunIntoList(tracks);
            UpdateImagesToDatabase();
            return true;
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
                if (sw.MusicPictures != null && sw.MusicPictures.Rows.Count > 0)
                {

                    foreach (var item in items)
                    {
                        try
                        {
                            UpdateItemToHashPath(item);
                        }
                        catch (Exception ex)
                        {
                            _logging.ServerErrorsAdd("UpdateItemListToHashPath", ex, "MusicPictures");
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("UpdateItemListToHashPath2", ex, "MusicPictures");
            }

            return items;
        }
        public SonosItem UpdateItemToHashPath(SonosItem item)
        {
            try
            {
                if (sw.MusicPictures != null && sw.MusicPictures.Rows.Count > 0)
                {
                    if(item == null)
                    {
                        return new SonosItem();
                    }
                    if (string.IsNullOrEmpty(item.AlbumArtURI) || item.AlbumArtURI.StartsWith(SonosConstants.CoverHashPathForBrowser)) return item;
                    var covershort = SonosConstants.RemoveVersionInUri(item.AlbumArtURI);
                    if (sw.MusicPictures.Rows.Contains(covershort))
                    {
                        var row = sw.MusicPictures.Rows.Find(covershort);
                        object? hash = null;
                        if(row != null && row.ItemArray.Length >0)
                            hash = row.ItemArray[1];
                        if (hash != null)
                            item.AlbumArtURI = SonosConstants.CoverHashPathForBrowser + hash + ".png";
                    }
                }
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("UpdateItemToHashPath", ex, "MusicPictures");
            }
            return item;
        }
        #endregion PublicMethoden
        #region PrivateMethoden
        private void UpdateImagesToDatabase()
        {
            var dbvalues = sw.MusicPictures;
            var pathCn = dbvalues.Columns[0].ColumnName;
            var hashCn = dbvalues.Columns[1].ColumnName;
            bool changes = false;
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
                        _logging.ServerErrorsAdd("MusicPictures.ReplaceAlbumArt:" + fixedpath, ex, "MusicPictures");
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
            }

        }
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
