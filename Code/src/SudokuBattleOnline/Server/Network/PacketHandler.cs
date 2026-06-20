using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Database;
using Server.Models;
using Server.Services;
using Shared.Enums;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Xử lý logic nghiệp vụ cho từng loại gói tin nhận được từ client.
    /// Mọi dữ liệu tài khoản, hồ sơ, lịch sử và bảng xếp hạng đều xử lý qua SQLite phía Server.
    /// </summary>
    public class PacketHandler
    {
        private readonly SessionManager _sessionManager;
        private readonly DatabaseContext _databaseContext;
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        private readonly MatchRepository _matchRepository;
        private readonly RankingRepository _rankingRepository;

        public PacketHandler(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
            _databaseContext = new DatabaseContext("database/sudoku.db");
            _databaseContext.Initialize();

            _authService = new AuthService(_databaseContext);
            _userRepository = new UserRepository(_databaseContext);
            _matchRepository = new MatchRepository(_databaseContext);
            _rankingRepository = new RankingRepository(_databaseContext);
        }

        // ═══════════════════════════════════════════════
        //  XÁC THỰC (Authentication)
        // ═══════════════════════════════════════════════

        public async Task HandleLoginAsync(ClientSession session, LoginPacket packet)
        {
            Console.WriteLine($"[LOGIN] {session} yêu cầu đăng nhập: Username='{packet.Username}'");

            if (string.IsNullOrWhiteSpace(packet.Username) || string.IsNullOrWhiteSpace(packet.Password))
            {
                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Success = false,
                    Message = "Username và Password không được để trống."
                });
                return;
            }

            string username = packet.Username.Trim();
            string password = packet.Password.Trim();

            if (_sessionManager.IsUserOnline(username))
            {
                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Success = false,
                    Message = "Tài khoản này đang được đăng nhập ở nơi khác."
                });
                return;
            }

            bool isValid = _authService.Login(username, password, out string loginMessage);

            if (isValid)
            {
                session.Username = username;
                session.IsAuthenticated = true;

                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Username = username,
                    Success = true,
                    Message = loginMessage
                });
                Console.WriteLine($"[LOGIN] ✓ {session} đăng nhập thành công.");
            }
            else
            {
                await session.SendPacketAsync(new LoginPacket
                {
                    PacketType = "LOGIN_RESULT",
                    Success = false,
                    Message = loginMessage
                });
                Console.WriteLine($"[LOGIN] ✗ {session} đăng nhập thất bại: {loginMessage}");
            }
        }

        public async Task HandleRegisterAsync(ClientSession session, RegisterPacket packet)
        {
            Console.WriteLine($"[REGISTER] {session} yêu cầu đăng ký: Username='{packet.Username}'");

            if (string.IsNullOrWhiteSpace(packet.Username) || string.IsNullOrWhiteSpace(packet.Password))
            {
                await session.SendPacketAsync(new RegisterPacket
                {
                    PacketType = "REGISTER_RESULT",
                    Success = false,
                    Message = "Username và Password không được để trống."
                });
                return;
            }

            if (packet.Password != packet.ConfirmPassword)
            {
                await session.SendPacketAsync(new RegisterPacket
                {
                    PacketType = "REGISTER_RESULT",
                    Success = false,
                    Message = "Mật khẩu xác nhận không khớp."
                });
                return;
            }

            bool created = _authService.Register(packet.Username, packet.Password, out string registerMessage);

            await session.SendPacketAsync(new RegisterPacket
            {
                PacketType = "REGISTER_RESULT",
                Success = created,
                Message = registerMessage
            });

            if (created)
                Console.WriteLine($"[REGISTER] ✓ Đăng ký thành công cho '{packet.Username}'.");
            else
                Console.WriteLine($"[REGISTER] ✗ Đăng ký thất bại cho '{packet.Username}': {registerMessage}");
        }

        // ═══════════════════════════════════════════════
        //  HỒ SƠ NGƯỜI CHƠI
        // ═══════════════════════════════════════════════

        public async Task HandleProfileAsync(ClientSession session, UserProfilePacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            string username = session.Username!;
            var user = _userRepository.FindByUsername(username);

            if (user == null)
            {
                await session.SendPacketAsync(new UserProfilePacket
                {
                    PacketType = "PROFILE_RESULT",
                    Success = false,
                    Message = "Không tìm thấy hồ sơ người dùng trên Server."
                });
                return;
            }

            await session.SendPacketAsync(new UserProfilePacket
            {
                PacketType = "PROFILE_RESULT",
                Success = true,
                Message = "Lấy hồ sơ thành công.",
                Id = user.Id,
                Username = user.Username,
                Elo = user.Elo,
                TotalWins = user.TotalWins,
                TotalLosses = user.TotalLosses,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        // ═══════════════════════════════════════════════
        //  PHÒNG CHƠI (Room Management)
        // ═══════════════════════════════════════════════

        public async Task HandleCreateRoomAsync(ClientSession session, CreateRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} tạo phòng: '{packet.RoomName}'");

            await session.SendPacketAsync(new CreateRoomPacket
            {
                PacketType = "CREATE_ROOM_RESULT",
                RoomName = packet.RoomName,
                Success = true,
                Message = $"Đã tạo phòng '{packet.RoomName}' thành công."
            });
        }

        public async Task HandleJoinRoomAsync(ClientSession session, JoinRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} tham gia phòng: {packet.RoomId}");

            await session.SendPacketAsync(new JoinRoomPacket
            {
                PacketType = "JOIN_ROOM_RESULT",
                RoomId = packet.RoomId,
                Success = true,
                Message = $"Đã tham gia phòng {packet.RoomId}."
            });
        }

        public async Task HandleLeaveRoomAsync(ClientSession session, LeaveRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} rời phòng: {packet.RoomId}");

            session.CurrentRoomId = null;

            await session.SendPacketAsync(new LeaveRoomPacket
            {
                PacketType = "LEAVE_ROOM_RESULT",
                RoomId = packet.RoomId,
                Success = true,
                Message = "Đã rời phòng."
            });
        }

        // ═══════════════════════════════════════════════
        //  TRẬN ĐẤU (Matchmaking & Gameplay)
        // ═══════════════════════════════════════════════

        public async Task HandleFindMatchAsync(ClientSession session, FindMatchPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[MATCH] {session} yêu cầu tìm trận.");

            await session.SendPacketAsync(new FindMatchPacket
            {
                PacketType = "FIND_MATCH",
                Success = true,
                Message = "Đang tìm đối thủ... Vui lòng chờ."
            });
        }

        public async Task HandleCellUpdateAsync(ClientSession session, CellUpdatePacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[GAME] {session} cập nhật ô [{packet.Row},{packet.Col}] = {packet.Value}");
            await Task.CompletedTask;
        }

        public async Task HandleSaveMatchResultAsync(ClientSession session, SaveMatchResultPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            string username = session.Username!;
            string opponent = string.IsNullOrWhiteSpace(packet.Opponent) ? "Single Player" : packet.Opponent.Trim();
            string result = string.IsNullOrWhiteSpace(packet.Result) ? "Completed" : packet.Result.Trim();

            if (!Enum.TryParse(packet.Difficulty, true, out Difficulty difficulty))
                difficulty = Difficulty.Medium;

            bool isWin = result.Equals("Win", StringComparison.OrdinalIgnoreCase);
            bool isLose = result.Equals("Lose", StringComparison.OrdinalIgnoreCase);
            int eloDelta = isWin ? 15 : isLose ? -10 : 0;

            var match = new MatchEntity
            {
                Player1 = username,
                Player2 = opponent,
                Winner = isWin ? username : string.Empty,
                Difficulty = difficulty,
                DurationSeconds = packet.TimeSeconds,
                EloChangeP1 = eloDelta,
                EloChangeP2 = 0,
                PlayedAt = DateTime.UtcNow
            };

            _matchRepository.SaveMatch(match);

            if (isWin)
                _userRepository.UpdateStats(username, 15, true);
            else if (isLose)
                _userRepository.UpdateStats(username, -10, false);

            if (packet.TimeSeconds > 0 && (isWin || result.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
                _rankingRepository.TryUpdateBestRecord(username, difficulty, packet.TimeSeconds);

            await session.SendPacketAsync(new SaveMatchResultPacket
            {
                PacketType = "SAVE_MATCH_RESULT",
                Success = true,
                Message = "Server đã lưu kết quả vào SQLite.",
                Opponent = opponent,
                Result = result,
                Difficulty = difficulty.ToString(),
                Score = packet.Score,
                TimeSeconds = packet.TimeSeconds
            });
        }

        // ═══════════════════════════════════════════════
        //  CHAT
        // ═══════════════════════════════════════════════

        public async Task HandleChatAsync(ClientSession session, ChatPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[CHAT] {session}: {packet.Content}");
            await Task.CompletedTask;
        }

        // ═══════════════════════════════════════════════
        //  RANKING + HISTORY
        // ═══════════════════════════════════════════════

        public async Task HandleRankingAsync(ClientSession session, RankingPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[RANKING] {session} yêu cầu bảng xếp hạng.");

            List<RankingEntry> rankings = _userRepository.GetTopPlayers(50)
                .Select((user, index) => new RankingEntry
                {
                    Rank = index + 1,
                    Username = user.Username,
                    RankPoint = user.Elo,
                    WinCount = user.TotalWins,
                    MatchCount = user.TotalWins + user.TotalLosses
                })
                .ToList();

            await session.SendPacketAsync(new RankingPacket
            {
                PacketType = "RANKING",
                Success = true,
                Message = "Lấy bảng xếp hạng thành công.",
                Rankings = rankings
            });
        }

        public async Task HandleMatchHistoryAsync(ClientSession session, MatchHistoryPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            string username = session.Username!;
            var history = _matchRepository.GetMatchHistory(username, 50)
                .Select(m => new MatchHistoryEntry
                {
                    Id = m.Id,
                    Player1 = m.Player1,
                    Player2 = m.Player2,
                    Winner = m.Winner,
                    Difficulty = m.Difficulty.ToString(),
                    DurationSeconds = m.DurationSeconds,
                    EloChangeP1 = m.EloChangeP1,
                    EloChangeP2 = m.EloChangeP2,
                    PlayedAt = m.PlayedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            await session.SendPacketAsync(new MatchHistoryPacket
            {
                PacketType = "MATCH_HISTORY_RESULT",
                Success = true,
                Message = "Lấy lịch sử đấu thành công.",
                History = history
            });
        }

        // ═══════════════════════════════════════════════
        //  HEARTBEAT (Ping/Pong)
        // ═══════════════════════════════════════════════

        public async Task HandlePingAsync(ClientSession session)
        {
            await session.SendPacketAsync(new BasePacket { PacketType = "PONG" });
        }

        private async Task<bool> RequireAuthAsync(ClientSession session)
        {
            if (!session.IsAuthenticated)
            {
                await session.SendPacketAsync(new BasePacket
                {
                    PacketType = "AUTH_REQUIRED",
                    Success = false,
                    Message = "Bạn cần đăng nhập trước khi thực hiện thao tác này."
                });
                Console.WriteLine($"[AUTH] {session} bị từ chối: chưa đăng nhập.");
                return false;
            }
            return true;
        }
    }
}
