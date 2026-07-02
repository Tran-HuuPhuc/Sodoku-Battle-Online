using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Database;
using Server.Models;
using Server.Services;
using Shared.Enums;
using SudokuBattleOnline.Shared.Packets;
using SudokuBattle.Server.Matchmaking;
using SudokuBattle.Server.Rooms;
using Server.GameManager;
using Shared.Models;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Xử lý logic nghiệp vụ cho từng loại gói tin nhận được từ client.
    /// Mọi dữ liệu tài khoản, hồ sơ, lịch sử và bảng xếp hạng đều xử lý qua SQLite phía Server.
    /// </summary>
    public class PacketHandler
    {
        private readonly SessionManager _sessionManager;
        private readonly MatchmakingQueue _matchmakingQueue;
        private readonly DatabaseContext _databaseContext;
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        private readonly MatchRepository _matchRepository;
        private readonly RankingRepository _rankingRepository;
        private readonly RoomManager _roomManager;

        public PacketHandler(SessionManager sessionManager, MatchmakingQueue matchmakingQueue, RoomManager roomManager)
        {
            _sessionManager = sessionManager;
            _matchmakingQueue = matchmakingQueue;
            _roomManager = roomManager;

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
                _ = CheckAndHandlePlayerReconnectAsync(session);
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

            var room = _roomManager.CreateRoom(packet.RoomName ?? "Room");
            var (success, message) = room.AddMember(session);

            await session.SendPacketAsync(new CreateRoomPacket
            {
                PacketType = "CREATE_ROOM_RESULT",
                RoomName = packet.RoomName,
                RoomId = room.Id,
                Success = success,
                Message = message
            });
            if (success)
            {
                Console.WriteLine($"[ROOM] {session} đã tạo và vào phòng {room.Id} ('{room.Name}').");
                await BroadcastRoomUpdateAsync(room);
            }
        }

        public async Task HandleJoinRoomAsync(ClientSession session, JoinRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[ROOM] {session} tham gia phòng: {packet.RoomId}");

            if (!_roomManager.TryGetRoom(packet.RoomId, out var room) || room == null)
            {
                await session.SendPacketAsync(new JoinRoomPacket
                {
                    PacketType = "JOIN_ROOM_RESULT",
                    RoomId = packet.RoomId,
                    Success = false,
                    Message = "Không tìm thấy phòng."
                });
                return;
            }

            var (success, message) = room.AddMember(session);
            await session.SendPacketAsync(new JoinRoomPacket
            {
                PacketType = "JOIN_ROOM_RESULT",
                RoomId = packet.RoomId,
                Success = success,
                Message = message
            });

            if (success)
            {
                await room.BroadcastExceptAsync(new ChatPacket
                {
                    PacketType = "ROOM_SYSTEM",
                    Content = $"{session.Username ?? session.SessionId} đã vào phòng."
                }, session.SessionId);
                Console.WriteLine($"[ROOM] {session} join phòng {room.Id}.");
                await BroadcastRoomUpdateAsync(room);
            }

        }

        public async Task HandleLeaveRoomAsync(ClientSession session, LeaveRoomPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            if (!_roomManager.TryGetRoom(packet.RoomId, out var room) || room == null)
            {
                await session.SendPacketAsync(new LeaveRoomPacket
                {
                    PacketType = "LEAVE_ROOM_RESULT",
                    RoomId = packet.RoomId,
                    Success = false,
                    Message = "Không tìm thấy phòng."
                });
                return;
            }

            if (room.IsGameStarted)
            {
                await HandlePlayerForfeitAsync(room, session.Username ?? session.SessionId);
            }

            var (success, message) = room.RemoveMember(session);
            await session.SendPacketAsync(new LeaveRoomPacket
            {
                PacketType = "LEAVE_ROOM_RESULT",
                RoomId = packet.RoomId,
                Success = success,
                Message = message
            });

            if (success)
            {
                await room.BroadcastAsync(new ChatPacket
                {
                    PacketType = "ROOM_SYSTEM",
                    Content = $"{session.Username ?? session.SessionId} đã rời phòng."
                });
                Console.WriteLine($"[ROOM] {session} rời phòng {room.Id}.");
                await BroadcastRoomUpdateAsync(room);
            }
        }

        private async Task BroadcastRoomUpdateAsync(Room room)
        {
            var players = room.Members.Select(m => m.Username ?? m.SessionId).ToList();
            var host = room.Host?.Username ?? string.Empty;
            var guest = room.Members.FirstOrDefault(m => m.SessionId != room.Host?.SessionId)?.Username ?? string.Empty;

            var updatePacket = new RoomUpdatePacket
            {
                RoomId = room.Id,
                HostUsername = host,
                GuestUsername = guest,
                Players = players
            };

            await room.BroadcastAsync(updatePacket);
        }

        // ═══════════════════════════════════════════════
        //  TRẬN ĐẤU (Matchmaking & Gameplay)
        // ═══════════════════════════════════════════════

        public async Task HandleFindMatchAsync(ClientSession session, FindMatchPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            Console.WriteLine($"[MATCH] {session} yêu cầu tìm trận.");
            
            _matchmakingQueue.Enqueue(session);

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

            if (string.IsNullOrEmpty(session.CurrentRoomId))
            {
                Console.WriteLine($"[GAME LỖI] {session} gửi cập nhật ô nhưng không ở trong phòng nào.");
                return;
            }

            if (!_roomManager.TryGetRoom(session.CurrentRoomId, out var room) || room == null)
            {
                Console.WriteLine($"[GAME LỖI] {session} gửi cập nhật ô nhưng phòng {session.CurrentRoomId} không tồn tại.");
                return;
            }

            if (room.GameRoom == null || room.GameManager == null)
            {
                Console.WriteLine($"[GAME LỖI] {session} gửi cập nhật ô nhưng GameRoom hoặc GameManager chưa được khởi tạo.");
                return;
            }

            int playerNumber = 0;
            if (room.GameRoom.Player1.Username == session.Username)
                playerNumber = 1;
            else if (room.GameRoom.Player2.Username == session.Username)
                playerNumber = 2;

            if (playerNumber == 0)
            {
                Console.WriteLine($"[GAME LỖI] {session} không thuộc danh sách người chơi của phòng.");
                return;
            }

            bool isCorrect = room.GameManager.SubmitMove(playerNumber, packet.Row, packet.Col, packet.Value);

            var response = new CellUpdatePacket
            {
                PacketType = "CELL_UPDATE",
                Row = packet.Row,
                Col = packet.Col,
                Value = packet.Value,
                Username = session.Username ?? string.Empty,
                Success = true,
                IsCorrect = isCorrect,
                Player1Progress = room.GameRoom.GameState.Player1Progress,
                Player2Progress = room.GameRoom.GameState.Player2Progress,
                Player1Mistakes = room.GameRoom.Player1.Mistakes,
                Player2Mistakes = room.GameRoom.Player2.Mistakes
            };

            await room.BroadcastAsync(response);

            if (room.GameManager.IsGameFinished())
            {
                room.GameRoom.IsStarted = false;
                room.IsGameStarted = false;

                string winnerName = room.GameManager.GetWinnerName();
                string reason;
                if (room.GameRoom.Player1.Mistakes >= 5 && winnerName == room.GameRoom.Player2.Username)
                    reason = $"{room.GameRoom.Player1.Username} nhập sai 5 lần nên thua trận.";
                else if (room.GameRoom.Player2.Mistakes >= 5 && winnerName == room.GameRoom.Player1.Username)
                    reason = $"{room.GameRoom.Player2.Username} nhập sai 5 lần nên thua trận.";
                else
                    reason = winnerName == "Draw" ? "Trận đấu hòa!" : $"Chúc mừng {winnerName} đã chiến thắng!";

                var gameOverPacket = new GameOverPacket
                {
                    RoomId = room.Id,
                    WinnerUsername = winnerName,
                    Reason = reason,
                    Player1Progress = room.GameRoom.GameState.Player1Progress,
                    Player2Progress = room.GameRoom.GameState.Player2Progress
                };

                await room.BroadcastAsync(gameOverPacket);

                try
                {
                    bool isP1Win = winnerName == room.GameRoom.Player1.Username;
                    bool isP2Win = winnerName == room.GameRoom.Player2.Username;
                    bool isDraw = winnerName == "Draw" || string.IsNullOrEmpty(winnerName);

                    int eloP1 = isP1Win ? 15 : (isDraw ? 0 : -10);
                    int eloP2 = isP2Win ? 15 : (isDraw ? 0 : -10);

                    var match = new MatchEntity
                    {
                        Player1 = room.GameRoom.Player1.Username,
                        Player2 = room.GameRoom.Player2.Username,
                        Winner = isDraw ? string.Empty : winnerName,
                        Difficulty = Difficulty.Medium,
                        DurationSeconds = 300,
                        EloChangeP1 = eloP1,
                        EloChangeP2 = eloP2,
                        PlayedAt = DateTime.UtcNow
                    };

                    _matchRepository.SaveMatch(match);

                    _userRepository.UpdateStats(room.GameRoom.Player1.Username, eloP1, isP1Win);
                    _userRepository.UpdateStats(room.GameRoom.Player2.Username, eloP2, isP2Win);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GAME LỖI CSDL] Không thể lưu kết quả trận đấu mạng: {ex.Message}");
                }
            }
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
            int eloDelta = isWin ? difficulty.GetEloWin() : isLose ? difficulty.GetEloLose() : 0;

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
                _userRepository.UpdateStats(username, eloDelta, true);
            else if (isLose)
                _userRepository.UpdateStats(username, eloDelta, false);

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

            if (string.IsNullOrEmpty(session.CurrentRoomId))
            {
                Console.WriteLine($"[CHAT LỖI] {session} gửi tin nhắn chat nhưng không ở trong phòng.");
                return;
            }

            if (!_roomManager.TryGetRoom(session.CurrentRoomId, out var room) || room == null)
            {
                Console.WriteLine($"[CHAT LỖI] Phòng {session.CurrentRoomId} không tồn tại.");
                return;
            }

            var broadcastPacket = new ChatPacket
            {
                PacketType = "CHAT",
                Sender = session.Username ?? "Unknown",
                Content = packet.Content,
                Timestamp = DateTime.Now.ToString("HH:mm:ss")
            };

            await room.BroadcastAsync(broadcastPacket);
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

        public async Task HandleGetRoomsAsync(ClientSession session, GetRoomsPacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            var rooms = _roomManager.GetAllRooms()
                .Select(r => new RoomInfoData
                {
                    RoomId = r.Id,
                    RoomName = r.Name,
                    PlayerCount = r.Members.Length,
                    MaxPlayers = r.MaxPlayers,
                    IsStarted = r.IsGameStarted
                })
                .ToList();

            await session.SendPacketAsync(new GetRoomsPacket
            {
                PacketType = "GET_ROOMS",
                Success = true,
                Message = "Lấy danh sách phòng thành công.",
                Rooms = rooms
            });
        }

        public async Task HandleBestScoreRequestAsync(ClientSession session, BestScorePacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            try
            {
                var easy = _rankingRepository.GetLeaderboard(Difficulty.Easy, 20);
                var medium = _rankingRepository.GetLeaderboard(Difficulty.Medium, 20);
                var hard = _rankingRepository.GetLeaderboard(Difficulty.Hard, 20);

                var allRecords = easy.Concat(medium).Concat(hard)
                    .OrderBy(r => r.BestTimeSeconds)
                    .ToList();

                var scores = allRecords.Select((r, idx) => new BestScoreItem
                {
                    Rank = idx + 1,
                    Username = r.Username,
                    Difficulty = r.Difficulty.ToVietnamese(),
                    BestScore = Math.Max(0, 1000 - r.BestTimeSeconds * 2),
                    BestTimeSeconds = r.BestTimeSeconds,
                    AchievedAt = r.AchievedAt.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                await session.SendPacketAsync(new BestScorePacket
                {
                    PacketType = "BEST_SCORE_RESULT",
                    Success = true,
                    Message = "Lấy Best Score thành công.",
                    Scores = scores
                });
            }
            catch (Exception ex)
            {
                await session.SendPacketAsync(new BestScorePacket
                {
                    PacketType = "BEST_SCORE_RESULT",
                    Success = false,
                    Message = "Lỗi: " + ex.Message
                });
            }
        }

        public async Task HandleStartGameAsync(ClientSession session)
        {
            if (!await RequireAuthAsync(session)) return;

            if (string.IsNullOrEmpty(session.CurrentRoomId)) return;

            if (!_roomManager.TryGetRoom(session.CurrentRoomId, out var room) || room == null) return;

            if (room.Host?.SessionId != session.SessionId)
            {
                await session.SendPacketAsync(new BasePacket
                {
                    PacketType = "START_GAME_RESULT",
                    Success = false,
                    Message = "Chỉ chủ phòng mới có thể bắt đầu trận đấu."
                });
                return;
            }

            if (room.Members.Length < 2)
            {
                await session.SendPacketAsync(new BasePacket
                {
                    PacketType = "START_GAME_RESULT",
                    Success = false,
                    Message = "Phòng cần ít nhất 2 người chơi để bắt đầu."
                });
                return;
            }

            bool allReady = room.Members.All(m => room.ReadyStates.GetValueOrDefault(m.Username ?? m.SessionId, false));
            if (!allReady)
            {
                await session.SendPacketAsync(new BasePacket
                {
                    PacketType = "START_GAME_RESULT",
                    Success = false,
                    Message = "Cả hai người chơi cần bấm Sẵn sàng để bắt đầu."
                });
                return;
            }

            room.IsGameStarted = true;

            var player1 = room.Members[0];
            var player2 = room.Members[1];

            var gameManager = new GameManager();
            var (puzzle, solution) = gameManager.GeneratePuzzleWithSolution(Difficulty.Medium);
            int[] flatBoard = gameManager.FlattenBoard(puzzle);

            var gameRoom = new GameRoom
            {
                RoomId = room.Id,
                Player1 = new global::Shared.Models.Player { Username = player1.Username ?? "Player 1", Score = 0, Mistakes = 0 },
                Player2 = new global::Shared.Models.Player { Username = player2.Username ?? "Player 2", Score = 0, Mistakes = 0 },
                PuzzleBoard = puzzle,
                SolutionBoard = solution
            };
            var mpGameManager = new MultiplayerGameManager(gameRoom);
            mpGameManager.StartGame(puzzle, solution);

            room.GameRoom = gameRoom;
            room.GameManager = mpGameManager;

            await player1.SendPacketAsync(new GameStartPacket
            {
                RoomId = room.Id,
                Board = flatBoard,
                TimeLimitSeconds = 600,
                OpponentUsername = player2.Username ?? "Unknown",
                Player1Username = player1.Username ?? "Unknown"
            });

            await player2.SendPacketAsync(new GameStartPacket
            {
                RoomId = room.Id,
                Board = flatBoard,
                TimeLimitSeconds = 600,
                OpponentUsername = player1.Username ?? "Unknown",
                Player1Username = player1.Username ?? "Unknown"
            });

            StartGameTimeoutMonitor(room);
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

        public void StartGameTimeoutMonitor(Room room)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(600000); // 10 phút
                await HandleGameTimeoutAsync(room);
            });
        }

        public async Task HandleGameTimeoutAsync(Room room)
        {
            if (room.GameRoom == null || !room.IsGameStarted || room.GameRoom.GameState.IsFinished)
                return;

            room.GameRoom.GameState.IsFinished = true;
            room.IsGameStarted = false;
            room.GameRoom.IsStarted = false;

            // Gọi logic của GameManager để xác định người chiến thắng dựa trên Score/Mistakes
            room.GameManager.EndGame();

            string winnerUsername = room.GameManager.GetWinnerName();
            string reason = winnerUsername == "Draw" 
                ? "Hết giờ! Trận đấu kết thúc với kết quả Hòa." 
                : $"Hết giờ! {winnerUsername} hoàn thành nhiều tiến trình hơn và giành chiến thắng chung cuộc.";

            var gameOverPacket = new GameOverPacket
            {
                RoomId = room.Id,
                WinnerUsername = winnerUsername,
                Reason = reason,
                Player1Progress = room.GameRoom.GameState.Player1Progress,
                Player2Progress = room.GameRoom.GameState.Player2Progress
            };

            await room.BroadcastAsync(gameOverPacket);

            // Lưu kết quả trận đấu và ELO vào SQLite
            try
            {
                string player1 = room.GameRoom.Player1.Username;
                string player2 = room.GameRoom.Player2.Username;

                bool draw = winnerUsername == "Draw";
                bool isP1Win = winnerUsername == player1;
                
                int eloP1 = draw ? 2 : (isP1Win ? 15 : -10);
                int eloP2 = draw ? 2 : (!isP1Win ? 15 : -10);

                var match = new MatchEntity
                {
                    Player1 = player1,
                    Player2 = player2,
                    Winner = winnerUsername,
                    Difficulty = Difficulty.Medium,
                    DurationSeconds = 600,
                    EloChangeP1 = eloP1,
                    EloChangeP2 = eloP2,
                    PlayedAt = DateTime.UtcNow
                };

                _matchRepository.SaveMatch(match);
                _userRepository.UpdateStats(player1, eloP1, !draw && isP1Win);
                _userRepository.UpdateStats(player2, eloP2, !draw && !isP1Win);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TIMEOUT CSDL LỖI] {ex.Message}");
            }
        }

        public async Task HandlePlayerForfeitAsync(Room room, string forfeitedUsername)
        {
            if (room.GameRoom == null || !room.IsGameStarted || room.GameRoom.GameState.IsFinished)
                return;

            room.GameRoom.GameState.IsFinished = true;
            room.IsGameStarted = false;
            room.GameRoom.IsStarted = false;

            string winnerUsername = string.Empty;
            if (room.GameRoom.Player1.Username == forfeitedUsername)
            {
                room.GameRoom.Player1.IsWinner = false;
                room.GameRoom.Player2.IsWinner = true;
                winnerUsername = room.GameRoom.Player2.Username;
            }
            else
            {
                room.GameRoom.Player2.IsWinner = false;
                room.GameRoom.Player1.IsWinner = true;
                winnerUsername = room.GameRoom.Player1.Username;
            }

            string reason = $"Đối thủ ({forfeitedUsername}) tự ý rời trận đấu. Bạn thắng cuộc!";

            var gameOverPacket = new GameOverPacket
            {
                RoomId = room.Id,
                WinnerUsername = winnerUsername,
                Reason = reason,
                Player1Progress = room.GameRoom.GameState.Player1Progress,
                Player2Progress = room.GameRoom.GameState.Player2Progress
            };

            await room.BroadcastAsync(gameOverPacket);

            // Lưu kết quả đấu vào CSDL SQLite
            try
            {
                string player1 = room.GameRoom.Player1.Username;
                string player2 = room.GameRoom.Player2.Username;

                bool isP1Win = winnerUsername == player1;
                
                int eloP1 = isP1Win ? 15 : -10;
                int eloP2 = !isP1Win ? 15 : -10;

                var match = new MatchEntity
                {
                    Player1 = player1,
                    Player2 = player2,
                    Winner = winnerUsername,
                    Difficulty = Difficulty.Medium,
                    DurationSeconds = 600,
                    EloChangeP1 = eloP1,
                    EloChangeP2 = eloP2,
                    PlayedAt = DateTime.UtcNow
                };

                _matchRepository.SaveMatch(match);
                _userRepository.UpdateStats(player1, eloP1, isP1Win);
                _userRepository.UpdateStats(player2, eloP2, !isP1Win);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FORFEIT CSDL LỖI] {ex.Message}");
            }
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string RoomId, System.Threading.CancellationTokenSource Cts)> DisconnectedGamePlayers = new();

        public async Task HandleToggleReadyAsync(ClientSession session, BasePacket packet)
        {
            if (!await RequireAuthAsync(session)) return;

            if (string.IsNullOrEmpty(session.CurrentRoomId)) return;

            if (_roomManager.TryGetRoom(session.CurrentRoomId, out var room) && room != null)
            {
                lock (room)
                {
                    string username = session.Username ?? session.SessionId;
                    room.ReadyStates[username] = !room.ReadyStates.GetValueOrDefault(username, false);
                }
                
                await BroadcastRoomUpdateAsync(room);
            }
        }

        private async Task CheckAndHandlePlayerReconnectAsync(ClientSession session)
        {
            if (string.IsNullOrEmpty(session.Username)) return;

            if (DisconnectedGamePlayers.TryRemove(session.Username, out var state))
            {
                state.Cts.Cancel(); // Cancel forfeit timer

                if (_roomManager.TryGetRoom(state.RoomId, out var room) && room != null)
                {
                    var (success, msg) = room.RejoinMember(session);
                    if (success)
                    {
                        // Gửi lại GameStartPacket cho người chơi vừa kết nối lại
                        int[] flatBoard = new int[81];
                        for (int r = 0; r < 9; r++)
                        {
                            for (int c = 0; c < 9; c++)
                            {
                                flatBoard[r * 9 + c] = room.GameRoom.GameState.Board[r, c];
                            }
                        }

                        string opponentName = room.GameRoom.Player1.Username == session.Username 
                            ? room.GameRoom.Player2.Username 
                            : room.GameRoom.Player1.Username;

                        await session.SendPacketAsync(new GameStartPacket
                        {
                            RoomId = room.Id,
                            Board = flatBoard,
                            TimeLimitSeconds = 600,
                            OpponentUsername = opponentName,
                            Player1Username = room.GameRoom.Player1.Username
                        });

                        await room.BroadcastExceptAsync(new ChatPacket
                        {
                            PacketType = "ROOM_SYSTEM",
                            Content = $"{session.Username} đã kết nối lại trận đấu!"
                        }, session.SessionId);
                        
                        await room.BroadcastAsync(new CellUpdatePacket
                        {
                            Row = -1,
                            Col = -1,
                            Value = 0,
                            Username = session.Username,
                            Success = true,
                            IsCorrect = true,
                            Player1Progress = room.GameRoom.GameState.Player1Progress,
                            Player2Progress = room.GameRoom.GameState.Player2Progress,
                            Player1Mistakes = room.GameRoom.Player1.Mistakes,
                            Player2Mistakes = room.GameRoom.Player2.Mistakes
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Người chơi CHỦ ĐỘNG bấm "Rời trận" → đối thủ thắng ngay lập tức, không có thời gian chờ.
        /// Khác với Disconnect: không có grace period 30s.
        /// </summary>
        public async Task HandleVoluntaryForfeitAsync(ClientSession session)
        {
            if (string.IsNullOrEmpty(session.CurrentRoomId) || string.IsNullOrEmpty(session.Username)) return;

            if (_roomManager.TryGetRoom(session.CurrentRoomId, out var room) && room != null)
            {
                if (room.IsGameStarted && room.GameRoom != null && !room.GameRoom.GameState.IsFinished)
                {
                    Console.WriteLine($"[FORFEIT] {session.Username} chủ động rời trận → xử thua ngay.");
                    await HandlePlayerForfeitAsync(room, session.Username);
                }
            }
        }

        public async Task HandlePlayerDisconnectForfeitAsync(ClientSession session)

        {
            if (string.IsNullOrEmpty(session.CurrentRoomId) || string.IsNullOrEmpty(session.Username)) return;

            if (_roomManager.TryGetRoom(session.CurrentRoomId, out var room) && room != null)
            {
                if (room.IsGameStarted && room.GameRoom != null && !room.GameRoom.GameState.IsFinished)
                {
                    await room.BroadcastExceptAsync(new ChatPacket
                    {
                        PacketType = "ROOM_SYSTEM",
                        Content = $"Đối thủ ({session.Username}) mất kết nối. Đang chờ kết nối lại (30s)..."
                    }, session.SessionId);

                    var cts = new System.Threading.CancellationTokenSource();
                    DisconnectedGamePlayers[session.Username] = (room.Id, cts);

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(30000, cts.Token); // 30 giây chờ kết nối lại
                            if (DisconnectedGamePlayers.TryRemove(session.Username, out _))
                            {
                                await HandlePlayerForfeitAsync(room, session.Username);
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            // Reconnected successfully
                        }
                    });
                }
            }
        }
    }
}
