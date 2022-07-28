//using System.Data.SQLite;
//using HomeLogging;
//using SonosConst;

//namespace SonosSQLite
//{
//    public static class DatabaseWrapper
//    {
//        private static SQLiteConnection conn;
//        private static SQLiteCommand cmd;
//        private static SQLiteCommand rcmd;
//        private static string cs = @"URI=file:" + SonosConstants.Configuration["MusicPictureDBPath"];


//        public static Logging Logger { get; set; } = new Logging();
//        public static Dictionary<string, string> DBPathCheckSum { get; set; } = new();
//        public static List<string> DBPrepareList { get; set; } = new();


//        public async static Task<Boolean> FillMusicPictureHashes()
//        {
//            //SonosConstants.MusicPictureHashes = await ReadHashes();
//            return true;//todo: try catch
//        }

//        public async static Task<Boolean> UpdateDB(Logging aLogger)
//        {
//            ReplaceAlbumArt();
//           // SonosConstants.MusicPictureHashes = DBPathCheckSum;
//            return await UpdateHashes(DBPathCheckSum);
//        }

//        /// <summary>
//        /// Durchgeht die DBPreparelist und liest die Checksum pro Cover aus.
//        /// </summary>
//        private static void ReplaceAlbumArt()
//        {
//            //übergebene liste durchlaufen und entsprechend die pfade anpassen.
//            //evtl weiter granulieren, das hier nur durchlaufen wird und in einer anderen methode die pfade angepasst wird. 

//            foreach (string item in DatabaseWrapper.DBPrepareList)
//            {
//                var cover = item;
//                var withoutVersion = SonosConstants.RemoveVersionInUri(cover);
//                if (!DatabaseWrapper.DBPathCheckSum.ContainsKey(withoutVersion))
//                {
//                    //Wert nicht vorhanden somit laden.
//                    var fixedpath = SonosConstants.AlbumArtToFile(cover);
//                    if (File.Exists(fixedpath))
//                    {
//                        try
//                        {
//                            if (fixedpath.EndsWith(".aiff")) continue;
//                            var hash = MP3.TagLibDelivery.GetPictureHash(fixedpath);
//                            DatabaseWrapper.DBPathCheckSum.Add(withoutVersion, hash);
//                        }
//                        catch (Exception ex)
//                        {
//                            Logger.ServerErrorsAdd("MusicPictures.ReplaceAlbumArt:" + fixedpath, ex);
//                            continue;
//                        }
//                    }
//                }
//            }
//        }

//        private static async Task<Boolean> Open()
//        {
//            try
//            {
//#if DEBUG
//                cs = @"URI=file:C:\\talun\\musicpictures.db";
//#endif
//                conn = new SQLiteConnection(cs);
//                await conn.OpenAsync();
//                cmd = new SQLiteCommand(conn)
//                {
//                    CommandText = "CREATE TABLE IF NOT EXISTS musicpictures (path text PRIMARY KEY,hash text NOT NULL);"
//                };
//                await cmd.ExecuteNonQueryAsync();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Logger.ServerErrorsAdd("databaseWrapper.Open", ex);
//                return false;
//            }

//        }
//        /// <summary>
//        /// Liest alle Bilder aus der Datenbank.
//        /// </summary>
//        /// <returns></returns>
//        private static async Task<Dictionary<string, string>> ReadHashes()
//        {
//            try
//            {
//                Dictionary<string, string> retval = new();
//                if (cmd == null || conn == null || conn.State != System.Data.ConnectionState.Open) await Open();
//                rcmd = new SQLiteCommand(conn)
//                {
//                    CommandText = "Select * from musicpictures"
//                };
//                var rdr = await rcmd.ExecuteReaderAsync();
//                //var nameOrdinal = rdr.GetOrdinal("name");
//                while (rdr.Read())
//                {
//                    var path = rdr.GetString(0);
//                    //var name = rdr.GetString(nameOrdinal);
//                    var hash = rdr.GetString(1);
//                    retval.Add(path, hash);
//                }
//                return retval;
//            }
//            catch (Exception ex)
//            {
//                Logger.ServerErrorsAdd("databaseWrapper.ReadHashes", ex);
//                return new Dictionary<string, string>();
//            }
//            finally
//            {
//                Close();
//            }
//        }

//        private static void Close()
//        {
//            if (conn != null && conn.State != System.Data.ConnectionState.Closed)
//                conn.Close();

//        }

//        /// <summary>
//        /// Aktualisiert Bilder in die DB
//        /// </summary>
//        private static async Task<Boolean> UpdateHashes(Dictionary<string, string> pic)
//        {
//            try
//            {
//                foreach (var item in pic)
//                {
//                    try
//                    {
//                        if (cmd == null || conn == null || conn.State != System.Data.ConnectionState.Open) await Open();
//                        cmd.CommandText = "REPLACE INTO musicpictures(path,hash) VALUES('" + item.Key + "','" + item.Value + "')";
//                        await cmd.ExecuteNonQueryAsync();
//                    }
//                    catch (Exception ex)
//                    {
//                        Logger.ServerErrorsAdd("databaseWrapper.UpdateHashes on Item:" + item.Key, ex);
//                        continue;
//                    }
//                }

//                return true;
//            }
//            catch (Exception ex)
//            {
//                Logger.ServerErrorsAdd("databaseWrapper.UpdateHashes", ex);
//                return false;
//            }
//            finally
//            {
//                Close();
//            }
//        }

//    }
//}