using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace GameLauncher
{
    public class DatabaseManager
    {
        private string connectionString = "Data Source=GameLibrary.db;Version=3;";

        public DatabaseManager()
        {
            InitializeDatabase();
        }

        // 確保資料庫與資料表存在
        private void InitializeDatabase()
        {
            if (!File.Exists("GameLibrary.db"))
            {
                SQLiteConnection.CreateFile("GameLibrary.db");
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string createGamesTable = @"CREATE TABLE IF NOT EXISTS Games (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Name TEXT NOT NULL,
                                            ExecutablePath TEXT NOT NULL)";

                string createPlaySessionsTable = @"CREATE TABLE IF NOT EXISTS PlaySessions (
                                                   SessionId INTEGER PRIMARY KEY AUTOINCREMENT,
                                                   GameId INTEGER,
                                                   DurationSeconds INTEGER,
                                                   PlayDate DATETIME DEFAULT CURRENT_TIMESTAMP)";

                using (SQLiteCommand cmd = new SQLiteCommand(createGamesTable, conn)) { cmd.ExecuteNonQuery(); }
                using (SQLiteCommand cmd = new SQLiteCommand(createPlaySessionsTable, conn)) { cmd.ExecuteNonQuery(); }
            }
        }

        // 新增遊戲到資料庫
        public void AddGame(string name, string path)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Games (Name, ExecutablePath) VALUES (@Name, @Path)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Path", path);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 取得所有遊戲
        public List<Game> GetAllGames()
        {
            List<Game> games = new List<Game>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Name, ExecutablePath FROM Games";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        games.Add(new Game(
                            Convert.ToInt32(reader["Id"]),
                            reader["Name"].ToString(),
                            reader["ExecutablePath"].ToString()
                        ));
                    }
                }
            }
            return games;
        }

        // 儲存遊玩時間
        public void LogPlaySession(int gameId, int durationSeconds)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO PlaySessions (GameId, DurationSeconds) VALUES (@GameId, @Duration)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GameId", gameId);
                    cmd.Parameters.AddWithValue("@Duration", durationSeconds);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
