using HomeLogging;
using Microsoft.Extensions.Configuration;
using SonosConst;
using SonosSQLiteWrapper.Interfaces;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;

namespace SonosSQLiteWrapper
{
    /// <summary>
    /// Manages the tasks database
    /// </summary>
    public class SQLiteWrapper : ISQLiteWrapper
    {
        private readonly SQLiteConnection sqlite;
        private readonly SQLiteDataAdapter adapter;
        private readonly SQLiteCommandBuilder builder;
        private readonly ILogging _logging;
        private readonly string cs = "";
        private const string musictable = "musicpictures";
        /// <summary>
        /// Creates a timesheet object that determines if the filepath exists.
        /// If the filepath exists then it opens a database.
        /// If it does not exist it creates a database.
        /// </summary>
        /// <param name="filePath">The path to open or create a database file at.</param>
        public SQLiteWrapper(IConfiguration configuration, ILogging logging)
        {
            //CREATE TABLE musicpictures (path text PRIMARY KEY,hash text NOT NULL)
            cs = @"URI=file:" + configuration["MusicPictureDBPath"];
            if (Debugger.IsAttached)
                cs = @"URI=file:"+ configuration["MusicPictureDBPathDebug"];
            _logging = logging;
            sqlite = new SQLiteConnection(cs);
            if(!tableAlreadyExists(sqlite, musictable))
            {
                Createtable(sqlite, musictable);
            } 
            adapter = new SQLiteDataAdapter("Select path,hash,extension from "+ musictable, sqlite);
            builder = new SQLiteCommandBuilder(adapter);
            try
            {
                OpenDatabase();
                PrepareQueries();
                adapter.Fill(MusicPictures);
                PreparePrimaryKeys();
                Close();

            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("SQLiteWrapper:Ctor", ex, "SQLiteWrapper");
            }
        }

        private bool tableAlreadyExists(SQLiteConnection openConnection, string tableName)
        {
            var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tableName + "';";
            OpenDatabase();
            if (openConnection.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand(sql, openConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                return false;
            }
            else
            {
                throw new ArgumentException("Data.ConnectionState must be open");
            }
        }
        private void Createtable(SQLiteConnection openConnection, string tableName)
        {
            var sql = "CREATE TABLE "+ tableName + " (path text PRIMARY KEY,hash text NOT NULL,extension text NOT NULL)";
            OpenDatabase();
            if (openConnection.State == ConnectionState.Open)
            {
                SQLiteCommand command = new SQLiteCommand(sql, openConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                throw new ArgumentException("Data.ConnectionState must be open");
            }
        }
        /// <summary>
        /// Return the task table
        /// </summary>
        /// <returns></returns>
        public DataTable MusicPictures { get; set; } = new();

        public void Update()
        {
            OpenDatabase();
            adapter.Update(MusicPictures);
            MusicPictures.Clear();
            adapter.Fill(MusicPictures);
            Close();
        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        private void Close()
        {
            try
            {
                if (sqlite.State != ConnectionState.Closed)
                    sqlite.Close();
            }
            catch (SQLiteException ex)
            {
                _logging.ServerErrorsAdd("CloseDatabase", ex, "SQLiteWrapper");
            }
        }

        /// <summary>
        /// Creates the database connection
        /// </summary>
        private void OpenDatabase()
        {
            try
            {
                if (sqlite.State != ConnectionState.Open)
                    sqlite.Open();
            }
            catch (SQLiteException ex)
            {
                _logging.ServerErrorsAdd("OpenDatabase", ex, "SQLiteWrapper");
            }
        }

        /// <summary>
        /// Prepares the queries used to manipulate the timesheet
        /// </summary>
        private void PrepareQueries()
        {
            try
            {
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();
                adapter.InsertCommand = builder.GetInsertCommand();
            }
            catch (Exception ex)
            {
                _logging.ServerErrorsAdd("PrepareQueries", ex, "SQLiteWrapper");
            }
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
