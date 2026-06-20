using Microsoft.Data.Sqlite;
using Server.Models;
using Shared.Enums;

namespace Server.Database
{
    /// <summary>Thao tác CSDL cho bảng BestRecords và truy vấn bảng xếp hạng ELO.</summary>
    public class RankingRepository
    {
        private readonly DatabaseContext _ctx;
        public RankingRepository(DatabaseContext ctx) => _ctx = ctx;

        // ── Kỷ lục tốt nhất (best time) ───────────────────────────────────

        /// <summary>
        /// Cập nhật kỷ lục nếu thời gian mới nhanh hơn kỷ lục cũ.
        /// </summary>
        public bool TryUpdateBestRecord(string username, Difficulty difficulty, int durationSeconds)
        {
            using var conn = _ctx.CreateConnection();

            // Lấy kỷ lục hiện tại
            using var selectCmd = conn.CreateCommand();
            selectCmd.CommandText = @"
                SELECT BestTimeSeconds FROM BestRecords
                WHERE  Username = $u COLLATE NOCASE AND Difficulty = $d
                LIMIT  1";
            selectCmd.Parameters.AddWithValue("$u", username);
            selectCmd.Parameters.AddWithValue("$d", difficulty.ToString());

            var existing = selectCmd.ExecuteScalar();
            bool isNewRecord = existing == null || durationSeconds < Convert.ToInt32(existing);

            if (!isNewRecord) return false;

            // UPSERT
            using var upsertCmd = conn.CreateCommand();
            upsertCmd.CommandText = @"
                INSERT INTO BestRecords (Username, Difficulty, BestTimeSeconds, AchievedAt)
                VALUES ($u, $d, $t, $at)
                ON CONFLICT(Username, Difficulty)
                DO UPDATE SET BestTimeSeconds = excluded.BestTimeSeconds,
                              AchievedAt      = excluded.AchievedAt";
            upsertCmd.Parameters.AddWithValue("$u", username);
            upsertCmd.Parameters.AddWithValue("$d", difficulty.ToString());
            upsertCmd.Parameters.AddWithValue("$t", durationSeconds);
            upsertCmd.Parameters.AddWithValue("$at", DateTime.UtcNow.ToString("o"));
            upsertCmd.ExecuteNonQuery();
            return true;
        }

        /// <summary>Kỷ lục cá nhân của 1 người chơi (tất cả độ khó).</summary>
        public List<BestRecordEntity> GetPersonalBests(string username)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Username, Difficulty, BestTimeSeconds, AchievedAt
                FROM   BestRecords
                WHERE  Username = $u COLLATE NOCASE
                ORDER  BY Difficulty";
            cmd.Parameters.AddWithValue("$u", username);

            var list = new List<BestRecordEntity>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapRecord(reader));
            return list;
        }

        /// <summary>Bảng xếp hạng top-N theo thời gian nhanh nhất cho 1 độ khó.</summary>
        public List<BestRecordEntity> GetLeaderboard(Difficulty difficulty, int limit = 20)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Username, Difficulty, BestTimeSeconds, AchievedAt
                FROM   BestRecords
                WHERE  Difficulty = $d
                ORDER  BY BestTimeSeconds ASC
                LIMIT  $n";
            cmd.Parameters.AddWithValue("$d", difficulty.ToString());
            cmd.Parameters.AddWithValue("$n", limit);

            var list = new List<BestRecordEntity>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapRecord(reader));
            return list;
        }

        // ── Helper ────────────────────────────────────────────────────────

        private static BestRecordEntity MapRecord(SqliteDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Username = r.GetString(1),
            Difficulty = Enum.Parse<Difficulty>(r.GetString(2)),
            BestTimeSeconds = r.GetInt32(3),
            AchievedAt = DateTime.Parse(r.GetString(4))
        };
    }
}
