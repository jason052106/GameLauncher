using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;

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
                                            ExecutablePath TEXT NOT NULL,
                                            CoverImagePath TEXT,
                                            Category TEXT,
                                            Publisher TEXT)";

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
        public void AddGame(string name, string path, string coverPath ="")
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Games (Name, ExecutablePath, CoverImagePath) VALUES (@Name, @Path, @CoverPath)";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Path", path);
                    cmd.Parameters.AddWithValue("@CoverPath", coverPath);
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
                string query = "SELECT Id, Name, ExecutablePath, CoverImagePath, Category, Publisher FROM Games";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        games.Add(new Game(
                     Convert.ToInt32(reader["Id"]),
                     reader["Name"].ToString(),
                     reader["ExecutablePath"].ToString(),
                     reader["CoverImagePath"].ToString(),
                     reader["Category"].ToString(),   // 讀取分類
                     reader["Publisher"].ToString()   // 讀取發行商
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

        public int GetTotalPlayTimeSeconds(int gameId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT SUM(DurationSeconds) FROM PlaySessions WHERE GameId = @GameId";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GameId", gameId);
                    object result = cmd.ExecuteScalar();

                    // 如果還沒有遊玩紀錄，SUM 會回傳 DBNull，此時回傳 0 秒
                    return result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public DataTable GetPlayHistory(int gameId)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                // 將秒數轉換、時間格式化，讓顯示更美觀
                string query = @"SELECT 
                            PlayDate AS [遊玩日期時間], 
                            (DurationSeconds || ' 秒') AS [遊玩時間] 
                         FROM PlaySessions 
                         WHERE GameId = @GameId 
                         ORDER BY PlayDate DESC";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GameId", gameId);
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public void UpdateGameInfo(Game game)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                // 更新特定 Id 的遊戲資料
                string query = @"UPDATE Games 
                         SET Name = @Name, 
                             Category = @Category, 
                             Publisher = @Publisher, 
                             CoverImagePath = @CoverImagePath 
                         WHERE Id = @Id";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", game.Name);
                    cmd.Parameters.AddWithValue("@Category", game.Category);
                    cmd.Parameters.AddWithValue("@Publisher", game.Publisher);
                    cmd.Parameters.AddWithValue("@CoverImagePath", game.CoverImagePath);
                    cmd.Parameters.AddWithValue("@Id", game.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetAllCategories()
        {
            List<string> categories = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                // 抓取不重複的分類，且排除空值或空字串
                string query = "SELECT DISTINCT Category FROM Games WHERE Category IS NOT NULL AND Category != ''";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader["Category"].ToString());
                    }
                }
            }
            return categories;
        }
    }
}
