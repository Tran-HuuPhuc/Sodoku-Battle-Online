using Microsoft.Data.Sqlite;
using Server.Models;
using Shared.Enums;

namespace Server.Database
{
    /// <summary>Thao tác CSDL cho bảng Matches.</summary>
    public class MatchRepository
    {
        private readonly DatabaseContext _ctx;
        public MatchRepository(DatabaseContext ctx) => _ctx = ctx;

        // ── Lưu kết quả trận ──────────────────────────────────────────────

        public int SaveMatch(MatchEntity match)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Matches
                    (Player1, Player2, Winner, Difficulty, DurationSeconds, EloChangeP1, EloChangeP2, PlayedAt)
                VALUES
                    ($p1, $p2, $w, $diff, $dur, $e1, $e2, $at);
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$p1", match.Player1);
            cmd.Parameters.AddWithValue("$p2", match.Player2);
            cmd.Parameters.AddWithValue("$w", match.Winner);
            cmd.Parameters.AddWithValue("$diff", match.Difficulty.ToString());
            cmd.Parameters.AddWithValue("$dur", match.DurationSeconds);
            cmd.Parameters.AddWithValue("$e1", match.EloChangeP1);
            cmd.Parameters.AddWithValue("$e2", match.EloChangeP2);
            cmd.Parameters.AddWithValue("$at", match.PlayedAt.ToString("o"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ── Lịch sử của 1 người chơi ──────────────────────────────────────

        public List<MatchEntity> GetMatchHistory(string username, int limit = 20)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Player1, Player2, Winner, Difficulty,
                       DurationSeconds, EloChangeP1, EloChangeP2, PlayedAt
                FROM   Matches
                WHERE  Player1 = $u COLLATE NOCASE
                   OR  Player2 = $u COLLATE NOCASE
                ORDER  BY PlayedAt DESC
                LIMIT  $n";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$n", limit);

            var list = new List<MatchEntity>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapMatch(reader));
            return list;
        }

        // ── Helper ────────────────────────────────────────────────────────

        private static MatchEntity MapMatch(SqliteDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Player1 = r.GetString(1),
            Player2 = r.GetString(2),
            Winner = r.GetString(3),
            Difficulty = Enum.Parse<Difficulty>(r.GetString(4)),
            DurationSeconds = r.GetInt32(5),
            EloChangeP1 = r.GetInt32(6),
            EloChangeP2 = r.GetInt32(7),
            PlayedAt = DateTime.Parse(r.GetString(8))
        };
    }
}

