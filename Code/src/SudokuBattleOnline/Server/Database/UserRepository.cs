using Microsoft.Data.Sqlite;
using Server.Models;

namespace Server.Database
{
    /// <summary>Thao tác CSDL cho bảng Users.</summary>
    public class UserRepository
    {
        private readonly DatabaseContext _ctx;
        public UserRepository(DatabaseContext ctx) => _ctx = ctx;

        // ── Tạo tài khoản ──────────────────────────────────────────────────

        public bool CreateUser(string username, string passwordHash)
        {
            try
            {
                using var conn = _ctx.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash)
                    VALUES ($u, $p)";
                cmd.Parameters.AddWithValue("$u", username);
                cmd.Parameters.AddWithValue("$p", passwordHash);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint
            {
                return false;   // Username đã tồn tại
            }
        }

        // ── Tìm theo username ──────────────────────────────────────────────

        public UserEntity? FindByUsername(string username)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Username, PasswordHash, Elo, TotalWins, TotalLosses, CreatedAt
                FROM   Users
                WHERE  Username = $u COLLATE NOCASE
                LIMIT  1";
            cmd.Parameters.AddWithValue("$u", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return MapUser(reader);
        }

        // ── Cập nhật ELO sau trận ──────────────────────────────────────────

        public void UpdateStats(string username, int eloDelta, bool isWin)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = isWin
                ? @"UPDATE Users
                    SET Elo = MAX(0, Elo + $delta), TotalWins = TotalWins + 1
                    WHERE Username = $u COLLATE NOCASE"
                : @"UPDATE Users
                    SET Elo = MAX(0, Elo + $delta), TotalLosses = TotalLosses + 1
                    WHERE Username = $u COLLATE NOCASE";
            cmd.Parameters.AddWithValue("$delta", eloDelta);
            cmd.Parameters.AddWithValue("$u", username);
            cmd.ExecuteNonQuery();
        }

        // ── Bảng xếp hạng top-N ───────────────────────────────────────────

        public List<UserEntity> GetTopPlayers(int limit = 50)
        {
            using var conn = _ctx.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Username, PasswordHash, Elo, TotalWins, TotalLosses, CreatedAt
                FROM   Users
                ORDER  BY Elo DESC, TotalWins DESC
                LIMIT  $n";
            cmd.Parameters.AddWithValue("$n", limit);

            var list = new List<UserEntity>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapUser(reader));
            return list;
        }

        // ── Helper ────────────────────────────────────────────────────────

        private static UserEntity MapUser(SqliteDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Username = r.GetString(1),
            PasswordHash = r.GetString(2),
            Elo = r.GetInt32(3),
            TotalWins = r.GetInt32(4),
            TotalLosses = r.GetInt32(5),
            CreatedAt = DateTime.Parse(r.GetString(6))
        };
    }
}
