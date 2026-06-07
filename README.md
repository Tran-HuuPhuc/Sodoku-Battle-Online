```text
SudokuBattleOnline
│
├── src
│   │
│   ├── SudokuBattle.Client
│   │   │
│   │   ├── Forms
│   │   │   ├── LoginForm.cs
│   │   │   ├── RegisterForm.cs
│   │   │   ├── MainMenuForm.cs
│   │   │   ├── ProfileForm.cs
│   │   │   ├── RankingForm.cs
│   │   │   ├── MatchHistoryForm.cs
│   │   │   ├── SinglePlayerForm.cs
│   │   │   ├── LobbyForm.cs
│   │   │   ├── RoomForm.cs
│   │   │   └── MultiplayerGameForm.cs
│   │   │
│   │   ├── Controls
│   │   │   ├── SudokuBoardControl.cs
│   │   │   ├── SudokuCellControl.cs
│   │   │   ├── PlayerInfoControl.cs
│   │   │   └── ChatControl.cs
│   │   │
│   │   ├── Network
│   │   │   ├── ClientConnection.cs
│   │   │   ├── PacketSender.cs
│   │   │   ├── PacketReceiver.cs
│   │   │   └── PacketHandler.cs
│   │   │
│   │   ├── Services
│   │   │   ├── AuthService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── RankingService.cs
│   │   │   ├── MatchHistoryService.cs
│   │   │   └── RoomService.cs
│   │   │
│   │   ├── Game
│   │   │   ├── Board.cs
│   │   │   ├── Cell.cs
│   │   │   ├── SudokuGenerator.cs
│   │   │   ├── SudokuValidator.cs
│   │   │   ├── TimerManager.cs
│   │   │   ├── ProgressCalculator.cs
│   │   │   └── SinglePlayerManager.cs
│   │   │
│   │   ├── Models
│   │   │   ├── User.cs
│   │   │   ├── Match.cs
│   │   │   ├── Room.cs
│   │   │   └── PlayerStatistic.cs
│   │   │
│   │   ├── Assets
│   │   │   ├── Images
│   │   │   ├── Icons
│   │   │   └── Sounds
│   │   │
│   │   └── Program.cs
│   │
│   ├── SudokuBattle.Server
│   │   │
│   │   ├── Network
│   │   │   ├── TcpServer.cs
│   │   │   ├── ClientSession.cs
│   │   │   ├── SessionManager.cs
│   │   │   ├── PacketRouter.cs
│   │   │   └── PacketHandler.cs
│   │   │
│   │   ├── Matchmaking
│   │   │   ├── MatchmakingQueue.cs
│   │   │   └── MatchmakingManager.cs
│   │   │
│   │   ├── RoomManager
│   │   │   ├── Room.cs
│   │   │   └── RoomManager.cs
│   │   │
│   │   ├── GameManager
│   │   │   ├── GameRoom.cs
│   │   │   ├── SudokuGenerator.cs
│   │   │   ├── SudokuValidator.cs
│   │   │   ├── MultiplayerGameManager.cs
│   │   │   └── ResultCalculator.cs
│   │   │
│   │   ├── Services
│   │   │   ├── AuthService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── RankingService.cs
│   │   │   ├── MatchHistoryService.cs
│   │   │   └── RoomService.cs
│   │   │
│   │   ├── Database
│   │   │   ├── DatabaseContext.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── MatchRepository.cs
│   │   │   ├── RankingRepository.cs
│   │   │   └── RoomRepository.cs
│   │   │
│   │   ├── Models
│   │   │   ├── UserEntity.cs
│   │   │   ├── MatchEntity.cs
│   │   │   └── RoomEntity.cs
│   │   │
│   │   └── Program.cs
│   │
│   └── SudokuBattle.Shared
│       │
│       ├── Models
│       │   ├── UserInfo.cs
│       │   ├── MatchInfo.cs
│       │   ├── RoomInfo.cs
│       │   ├── ChatMessage.cs
│       │   └── GameState.cs
│       │
│       ├── Packets
│       │   ├── BasePacket.cs
│       │   ├── LoginPacket.cs
│       │   ├── RegisterPacket.cs
│       │   ├── CreateRoomPacket.cs
│       │   ├── JoinRoomPacket.cs
│       │   ├── LeaveRoomPacket.cs
│       │   ├── FindMatchPacket.cs
│       │   ├── MatchFoundPacket.cs
│       │   ├── CellUpdatePacket.cs
│       │   ├── ChatPacket.cs
│       │   ├── GameStartPacket.cs
│       │   ├── GameOverPacket.cs
│       │   └── RankingPacket.cs
│       │
│       ├── Enums
│       │   ├── PacketType.cs
│       │   ├── Difficulty.cs
│       │   ├── RoomStatus.cs
│       │   ├── MatchResult.cs
│       │   └── UserStatus.cs
│       │
│       └── Constants
│           ├── NetworkConstants.cs
│           └── GameConstants.cs
│
├── database
│   ├── sudoku.db
│   └── backup
│
├── docs
│   ├── SRS
│   ├── UML
│   ├── ERD
│   ├── MeetingMinutes
│   ├── WeeklyReports
│   └── FinalReport
│
├── .gitignore
├── README.md
│
└── SudokuBattleOnline.sln
