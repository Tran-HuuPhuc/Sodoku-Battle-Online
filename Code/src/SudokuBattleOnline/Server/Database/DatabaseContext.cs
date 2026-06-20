using Microsoft.Data.Sqlite;

namespace Server.Database
{
    /// <summary>
    /// Quản lý kết nối SQLite và khởi tạo schema.
    ///
    /// Sử dụng:
    ///   var ctx = new DatabaseContext("database/sudoku.db");
    ///   ctx.Initialize();   // gọi một lần khi khởi động server
    /// </summary>
    public class DatabaseContext : IDisposable
    {
        private readonly string _connectionString;

        public DatabaseContext(string dbPath = "database/sudoku.db")
        {
            // Đảm bảo thư mục tồn tại
            string? dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            _connectionString = $"Data Source={dbPath};";
        }

        /// <summary>Tạo kết nối mới (caller chịu trách nhiệm Dispose).</summary>
        public SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Tạo các bảng nếu chưa tồn tại.
        /// Gọi một lần duy nhất khi server khởi động.
        /// </summary>
        public void Initialize()
        {
            using var conn = CreateConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                PRAGMA journal_mode=WAL;
                PRAGMA foreign_keys=ON;

                -- ── Người dùng ──────────────────────────────────────────
                CREATE TABLE IF NOT EXISTS Users (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username     TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                    PasswordHash TEXT    NOT NULL,
                    Elo          INTEGER NOT NULL DEFAULT 1000,
                    TotalWins    INTEGER NOT NULL DEFAULT 0,
                    TotalLosses  INTEGER NOT NULL DEFAULT 0,
                    CreatedAt    TEXT    NOT NULL DEFAULT (datetime('now'))
                );

                -- ── Lịch sử trận đấu ─────────────────────────────────────
                CREATE TABLE IF NOT EXISTS Matches (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Player1         TEXT    NOT NULL,
                    Player2         TEXT    NOT NULL,
                    Winner          TEXT    NOT NULL DEFAULT '',
                    Difficulty      TEXT    NOT NULL DEFAULT 'Medium',
                    DurationSeconds INTEGER NOT NULL DEFAULT 0,
                    EloChangeP1     INTEGER NOT NULL DEFAULT 0,
                    EloChangeP2     INTEGER NOT NULL DEFAULT 0,
                    PlayedAt        TEXT    NOT NULL DEFAULT (datetime('now'))
                );

                CREATE INDEX IF NOT EXISTS idx_matches_player1  ON Matches(Player1);
                CREATE INDEX IF NOT EXISTS idx_matches_player2  ON Matches(Player2);
                CREATE INDEX IF NOT EXISTS idx_matches_playedat ON Matches(PlayedAt DESC);

                -- ── Kỷ lục tốt nhất ──────────────────────────────────────
                CREATE TABLE IF NOT EXISTS BestRecords (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username        TEXT    NOT NULL,
                    Difficulty      TEXT    NOT NULL,
                    BestTimeSeconds INTEGER NOT NULL,
                    AchievedAt      TEXT    NOT NULL DEFAULT (datetime('now')),
                    UNIQUE(Username, Difficulty)
                );

                CREATE INDEX IF NOT EXISTS idx_records_difficulty ON BestRecords(Difficulty, BestTimeSeconds);
            ";

            cmd.ExecuteNonQuery();
            Console.WriteLine("[DB] Database khởi tạo thành công.");
        }

        public void Dispose() { /* connection-per-operation, không có gì cần dọn */ }
    }
}

