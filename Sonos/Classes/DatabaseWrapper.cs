using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sonos.Classes
{
    public static class DatabaseWrapper
    {
        private static SQLiteConnection conn;
        private static SQLiteCommand cmd;
        private static SQLiteCommand rcmd;
        private static readonly string cs = @"URI=file:"+ SonosHelper.Configuration["MusicPictureDBPath"];
        public static async Task<Boolean> Open()
        {
            try
            {
                conn = new SQLiteConnection(cs);
                await conn.OpenAsync();
                cmd = new SQLiteCommand(conn)
                {
                    CommandText = "CREATE TABLE IF NOT EXISTS musicpictures (path text PRIMARY KEY,hash text NOT NULL);"
                };
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("databaseWrapper.Open", ex);
                return false;
            }

        }

        public static void Close()
        {
            if (conn != null || conn.State != System.Data.ConnectionState.Closed)
                conn.Close();

        }
        /// <summary>
        /// Aktualisiert ein Button in die DB
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="name"></param>
        /// <param name="batteryvalue"></param>
        /// <param name="lastaction"></param>
        /// <returns></returns>
        public static async Task<Boolean> UpdateHashes(Dictionary<string,string> pic)
        {
            try
            {
                foreach (var item in pic)
                {
                    try
                    {
                        if (cmd == null || conn == null || conn.State != System.Data.ConnectionState.Open) await Open();
                        cmd.CommandText = "REPLACE INTO musicpictures(path,hash) VALUES('" + item.Key + "','" + item.Value + "')";
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch(Exception ex)
                    {
                        SonosHelper.Logger.ServerErrorsAdd("databaseWrapper.UpdateHashes on Item:"+item.Key, ex);
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("databaseWrapper.UpdateHashes", ex);
                return false;
            }
            finally
            {
                Close();
            }
        }
        /// <summary>
        /// Liest alle Bilder aus der Datenbank.
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string,string>> ReadHashes()
        {
            try
            {
                Dictionary<string, string> retval = new();
                if (cmd == null || conn == null || conn.State != System.Data.ConnectionState.Open) await Open();
                rcmd = new SQLiteCommand(conn)
                {
                    CommandText = "Select * from musicpictures"
                };
                var rdr = await rcmd.ExecuteReaderAsync();
                //var nameOrdinal = rdr.GetOrdinal("name");
                while (rdr.Read())
                {
                    var path = rdr.GetString(0);
                    //var name = rdr.GetString(nameOrdinal);
                    var hash = rdr.GetString(1);
                    retval.Add(path, hash);
                }
                return retval;
            }
            catch (Exception ex)
            {
                SonosHelper.Logger.ServerErrorsAdd("databaseWrapper.ReadHashes", ex);
                return new Dictionary<string,string>();
            }
            finally
            {
              //  Close();
            }
        }

    }
}