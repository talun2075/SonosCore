using HomeLogging;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SonosConst;
using SonosSQLiteWrapper.Interfaces;
using System.Data;
using System.Diagnostics;

namespace SonosSQLiteWrapper
{
    public class SQLiteWrapper : ISQLiteWrapper
    {
        private readonly SqliteConnection sqlite;
        private readonly ILogging _logging;
        private readonly string cs = "";
        private const string musictable = "musicpictures";

        public SQLiteWrapper(IConfiguration configuration, ILogging logging)
        {
            cs = $"Data Source={configuration["MusicPictureDBPath"]}";
            if (Debugger.IsAttached)
                cs = $"Data Source={configuration["MusicPictureDBPathDebug"]}";
            _logging = logging;
            sqlite = new SqliteConnection(cs);

            try
            {
                OpenDatabase();
                if (!TableAlreadyExists(sqlite, musictable))
                {
                    CreateTable(sqlite, musictable);
                }
                FillMusicPictures();
                PreparePrimaryKeys();
                Close();
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("SQLiteWrapper:Ctor", ex, "SQLiteWrapper");
            }
        }

        public DataTable MusicPictures { get; set; } = new();

        public void Update()
        {
            int errorCount = 0;
            const int maxConsecutiveErrors = 5;

            try
            {
                OpenDatabase();
                using var transaction = sqlite.BeginTransaction();
                using var command = sqlite.CreateCommand();

                foreach (DataRow row in MusicPictures.Rows)
                {
                    try
                    {
                        if (row.RowState == DataRowState.Added)
                        {
                            command.CommandText = "INSERT INTO musicpictures (path, hash, extension) VALUES (@path, @hash, @extension)";
                        }
                        else if (row.RowState == DataRowState.Modified)
                        {
                            command.CommandText = "UPDATE musicpictures SET hash = @hash, extension = @extension WHERE path = @path";
                        }
                        else if (row.RowState == DataRowState.Deleted)
                        {
                            command.CommandText = "DELETE FROM musicpictures WHERE path = @path";
                        }
                        else
                        {
                            continue;
                        }

                        command.Parameters.Clear();

                        if (row.HasVersion(DataRowVersion.Original))
                        {
                            command.Parameters.AddWithValue("@path", row["path", DataRowVersion.Original]);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@path", row["path"]);
                        }

                        if (row.RowState != DataRowState.Deleted)
                        {
                            if (row.HasVersion(DataRowVersion.Current))
                            {
                                command.Parameters.AddWithValue("@hash", row["hash", DataRowVersion.Current]);
                                command.Parameters.AddWithValue("@extension", row["extension", DataRowVersion.Current]);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@hash", row["hash"]);
                                command.Parameters.AddWithValue("@extension", row["extension"]);
                            }
                        }

                        command.ExecuteNonQuery();

                        // Zurücksetzen des Fehlerzählers bei erfolgreicher Verarbeitung
                        errorCount = 0;
                    }
                    catch (Exception rowEx)
                    {
                        errorCount++;
                        _logging.ServerErrorsAdd($"Update: Error processing row with path {row["path"]}", rowEx, "SQLiteWrapper");

                        if (errorCount >= maxConsecutiveErrors)
                        {
                            _logging.ServerErrorsAdd($"Update: Aborting after {maxConsecutiveErrors} consecutive errors", null, "SQLiteWrapper");
                            break;
                        }
                    }
                }

                transaction.Commit();
                MusicPictures.AcceptChanges();
                FillMusicPictures();
                Close();
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("Update: General error", ex, "SQLiteWrapper");
            }
        }


        private void Close()
        {
            try
            {
                if (sqlite.State != ConnectionState.Closed)
                    sqlite.Close();
            }
            catch (SqliteException ex)
            {
                _logging.ServerErrorsAdd("CloseDatabase", ex, "SQLiteWrapper");
            }
        }

        private void OpenDatabase()
        {
            try
            {
                if (sqlite.State != ConnectionState.Open)
                    sqlite.Open();
            }
            catch (SqliteException ex)
            {
                _logging.ServerErrorsAdd("OpenDatabase", ex, "SQLiteWrapper");
            }
        }

        private bool TableAlreadyExists(SqliteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            return command.ExecuteScalar() != null;
        }

        private void CreateTable(SqliteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE TABLE {tableName} (path TEXT PRIMARY KEY, hash TEXT NOT NULL, extension TEXT NOT NULL)";
            command.ExecuteNonQuery();
        }

        private void FillMusicPictures()
        {
            MusicPictures.Clear();
            using var command = sqlite.CreateCommand();
            command.CommandText = $"SELECT path, hash, extension FROM {musictable}";
            using var reader = command.ExecuteReader();
            MusicPictures.Load(reader);
        }

        private void PreparePrimaryKeys()
        {
            try
            {
                var keys = new DataColumn[1];
                keys[0] = MusicPictures.Columns[0];
                MusicPictures.PrimaryKey = keys;
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("Primarykeys", ex, "SQLiteWrapper");
            }
        }
    }
}
