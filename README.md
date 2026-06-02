```text
SudokuBattleOnline
│
├── src
│   │
│   ├── SudokuBattle.Client
│   │   ├── Forms
│   │   │   ├── Login
│   │   │   ├── Register
│   │   │   ├── MainMenu
│   │   │   ├── SinglePlayer
│   │   │   ├── Multiplayer
│   │   │   ├── Ranking
│   │   │   ├── History
│   │   │   └── Profile
│   │   │
│   │   ├── Controls
│   │   │   ├── SudokuBoard
│   │   │   ├── SudokuCell
│   │   │   └── ChatBox
│   │   │
│   │   ├── Services
│   │   │   ├── AuthService
│   │   │   ├── GameService
│   │   │   ├── RankingService
│   │   │   └── HistoryService
│   │   │
│   │   ├── Network
│   │   │   ├── ClientConnection
│   │   │   ├── PacketHandler
│   │   │   └── NetworkManager
│   │   │
│   │   ├── Game
│   │   │   ├── SudokuGenerator
│   │   │   ├── SudokuValidator
│   │   │   ├── TimerManager
│   │   │   └── ProgressTracker
│   │   │
│   │   ├── Models
│   │   └── Assets
│   │
│   ├── SudokuBattle.Server
│   │   ├── Network
│   │   │   ├── TcpServer
│   │   │   ├── ClientSession
│   │   │   └── PacketRouter
│   │   │
│   │   ├── Matchmaking
│   │   │   └── MatchmakingManager
│   │   │
│   │   ├── RoomManager
│   │   │   ├── Room
│   │   │   └── RoomManager
│   │   │
│   │   ├── GameManager
│   │   │   ├── SudokuGenerator
│   │   │   ├── SudokuValidator
│   │   │   ├── GameRoom
│   │   │   └── GameManager
│   │   │
│   │   ├── Services
│   │   │   ├── AuthService
│   │   │   ├── RankingService
│   │   │   └── HistoryService
│   │   │
│   │   ├── Database
│   │   │   ├── DatabaseContext
│   │   │   ├── UserRepository
│   │   │   ├── MatchRepository
│   │   │   └── RoomRepository
│   │   │
│   │   └── Models
│   │
│   ├── SudokuBattle.Shared
│   │   ├── Models
│   │   │   ├── User
│   │   │   ├── Match
│   │   │   ├── Room
│   │   │   └── GameState
│   │   │
│   │   ├── Packets
│   │   │   ├── LoginPacket
│   │   │   ├── RegisterPacket
│   │   │   ├── MatchPacket
│   │   │   ├── RoomPacket
│   │   │   ├── CellUpdatePacket
│   │   │   ├── ChatPacket
│   │   │   └── ResultPacket
│   │   │
│   │   ├── Enums
│   │   │   ├── PacketType
│   │   │   ├── Difficulty
│   │   │   ├── RoomStatus
│   │   │   └── MatchResult
│   │   │
│   │   └── Constants
│   │       ├── NetworkConstants
│   │       └── GameConstants
│
├── docs
│   ├── UML
│   ├── ERD
│   ├── Report
│   └── MeetingNotes
│
├── database
│   └── sudoku.db
│
├── .gitignore
│
├── README.md
│
└── SudokuBattleOnline.sln
