using System;
using System.Threading;
using System.Threading.Tasks;
using SudokuBattle.Server.Network;
using SudokuBattleOnline.Shared.Packets;
using SudokuBattle.Server.Rooms;
using Server.GameManager;
using Shared.Enums;

namespace SudokuBattle.Server.Matchmaking
{
    /// <summary>
    /// quản lý cái loop ghép người chơi trên server này, lần lượt lấy cố định 2 người chơi từ hàng đợi và tạo trận
    /// </summary>
    public class MatchmakingManager
    {
        private readonly MatchmakingQueue _queue;
        private readonly RoomManager _roomManager;
        private bool _isRunning;

        private readonly PacketHandler _packetHandler;

        public MatchmakingManager(MatchmakingQueue queue, RoomManager roomManager, PacketHandler packetHandler)
        {
            _queue = queue;
            _roomManager = roomManager;
            _packetHandler = packetHandler;
        }

        /// <summary>
        /// bắt đầu sẽ gọi MatchmakingLoopAsync để xử lý ghép cặp liên tục
        /// </summary>
        public void Start()
        {
            _isRunning = true;
            Task.Run(MatchmakingLoopAsync);
            Console.WriteLine("[MATCHMAKING] Đã khởi động trình quản lý ghép cặp.");
        }

        /// <summary>
        /// dừng loop ghép
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        private async Task MatchmakingLoopAsync()
        {
            while (_isRunning)
            {
                try
                {
                    // loop cho tới khi có 2 người
                    if (_queue.Count >= 2)
                    {
                        var player1 = _queue.Dequeue();
                        var player2 = _queue.Dequeue();

                        if (player1 != null && player2 != null)
                        {
                            bool p1Connected = player1.IsConnected;
                            bool p2Connected = player2.IsConnected;

                            if (p1Connected && p2Connected)
                            {
                                // Tạo phòng 
                                var roomName = $"Rank Match: {player1.Username} vs {player2.Username}";
                                var room = _roomManager.CreateRoom(roomName, 2);
                                string roomId = room.Id;

                                room.AddMember(player1);
                                room.AddMember(player2);
                                room.IsGameStarted = true;

                                Console.WriteLine($"[MATCHMAKING] Đã ghép cặp '{player1.Username}' và '{player2.Username}' vào phòng {roomId}");

                                // Khởi tạo GameManager và sinh đề bài (Mặc định Medium cho Online)
                                var gameManager = new GameManager();
                                var (puzzle, solution) = gameManager.GeneratePuzzleWithSolution(Difficulty.Medium);
                                int[] flatBoard = gameManager.FlattenBoard(puzzle);

                                // Khởi tạo GameRoom và MultiplayerGameManager
                                var gameRoom = new GameRoom
                                {
                                    RoomId = roomId,
                                    Player1 = new global::Shared.Models.Player { Username = player1.Username ?? "Player 1", Score = 0, Mistakes = 0 },
                                    Player2 = new global::Shared.Models.Player { Username = player2.Username ?? "Player 2", Score = 0, Mistakes = 0 },
                                    PuzzleBoard = puzzle,
                                    SolutionBoard = solution
                                };
                                var mpGameManager = new MultiplayerGameManager(gameRoom);
                                mpGameManager.StartGame(puzzle, solution);

                                room.GameRoom = gameRoom;
                                room.GameManager = mpGameManager;

                                // Gửi GameStartPacket cho player 1
                                await player1.SendPacketAsync(new GameStartPacket
                                {
                                    RoomId = roomId,
                                    Board = flatBoard,
                                    TimeLimitSeconds = 600,
                                    OpponentUsername = player2.Username ?? "Unknown",
                                    Player1Username = player1.Username ?? "Unknown"
                                });

                                // Gửi GameStartPacket cho player 2
                                await player2.SendPacketAsync(new GameStartPacket
                                {
                                    RoomId = roomId,
                                    Board = flatBoard,
                                    TimeLimitSeconds = 600,
                                    OpponentUsername = player1.Username ?? "Unknown",
                                    Player1Username = player1.Username ?? "Unknown"
                                });

                                _packetHandler.StartGameTimeoutMonitor(room);
                            }
                            else
                            {
                                // nếu một người mất kết nối thì cho thằng kia về hàng chờ
                                if (p1Connected) _queue.Enqueue(player1);
                                if (p2Connected) _queue.Enqueue(player2);
                            }
                        }
                    }

                    // delay 1 giây để tránh cpu chạy 100%
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MATCHMAKING LỖI] {ex.Message}");
                }
            }
        }
    }
}
