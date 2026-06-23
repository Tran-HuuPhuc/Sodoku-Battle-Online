```text
Sodoku-Battle-Online/
├── .git/
├── Code/
│   └── src/
│       └── SudokuBattleOnline/
│           │
│           ├── SudokuBattleOnline.sln
│           │
│           ├── Client/
│           │   │
│           │   ├── Client.csproj
│           │   ├── Program.cs
│           │   ├── AppSession.cs
│           │   │
│           │   ├── Assets/
│           │   │   ├── Images/
│           │   │   ├── Icons/
│           │   │   └── Sounds/
│           │   │
│           │   ├── Forms/
│           │   │   ├── LoginForm.cs
│           │   │   ├── LoginForm.Designer.cs
│           │   │   ├── LoginForm.resx
│           │   │   │
│           │   │   ├── RegisterForm.cs
│           │   │   ├── RegisterForm.Designer.cs
│           │   │   ├── RegisterForm.resx
│           │   │   │
│           │   │   ├── MainMenuForm.cs
│           │   │   ├── MainMenuForm.Designer.cs
│           │   │   ├── MainMenuForm.resx
│           │   │   │
│           │   │   ├── LobbyForm.cs
│           │   │   ├── LobbyForm.Designer.cs
│           │   │   ├── LobbyForm.resx
│           │   │   │
│           │   │   ├── RoomForm.cs
│           │   │   ├── RoomForm.Designer.cs
│           │   │   ├── RoomForm.resx
│           │   │   │
│           │   │   ├── ProfileForm.cs
│           │   │   ├── ProfileForm.Designer.cs
│           │   │   ├── ProfileForm.resx
│           │   │   │
│           │   │   ├── RankingForm.cs
│           │   │   ├── RankingForm.Designer.cs
│           │   │   ├── RankingForm.resx
│           │   │   │
│           │   │   ├── MatchHistoryForm.cs
│           │   │   ├── MatchHistoryForm.Designer.cs
│           │   │   ├── MatchHistoryForm.resx
│           │   │   │
│           │   │   ├── BestScoreForm.cs
│           │   │   ├── BestScoreForm.Designer.cs
│           │   │   ├── BestScoreForm.resx
│           │   │   │
│           │   │   ├── SinglePlayerForm.cs
│           │   │   ├── SinglePlayerForm.Designer.cs
│           │   │   ├── SinglePlayerForm.resx
│           │   │   │
│           │   │   ├── MultiplayerGameForm.cs
│           │   │   ├── MultiplayerGameForm.Designer.cs
│           │   │   └── MultiplayerGameForm.resx
│           │   │
│           │   ├── Game/
│           │   │   ├── SudokuGenerator.cs
│           │   │   └── SudokuValidator.cs
│           │   │
│           │   └── Network/
│           │       └── ClientConnection.cs
│           │
│           ├── Server/
│           │   │
│           │   ├── Server.csproj
│           │   ├── Program.cs
│           │   │
│           │   ├── Database/
│           │   │   ├── DatabaseContext.cs
│           │   │   ├── UserRepository.cs
│           │   │   ├── MatchRepository.cs
│           │   │   ├── RankingRepository.cs
│           │   │   └── BestRecordRepository.cs
│           │   │
│           │   ├── Models/
│           │   │   ├── UserEntity.cs
│           │   │   ├── MatchEntity.cs
│           │   │   ├── RoomEntity.cs
│           │   │   └── BestRecordEntity.cs
│           │   │
│           │   ├── Network/
│           │   │   ├── TcpServer.cs
│           │   │   ├── ClientSession.cs
│           │   │   ├── PacketRouter.cs
│           │   │   └── PacketHandler.cs
│           │   │
│           │   ├── Services/
│           │   │   └── AuthService.cs
│           │   │
│           │   ├── GameManager/
│           │   │   └── GameManager.cs
│           │   │
│           │   ├── RoomManager/
│           │   │   └── RoomManager.cs
│           │   │
│           │   └── Matchmaking/
│           │       └── MatchmakingManager.cs
│           │
│           └── Shared/
│               │
│               ├── Shared.csproj
│               │
│               ├── Packets/
│               │   ├── BasePacket.cs
│               │   ├── LoginPacket.cs
│               │   ├── RegisterPacket.cs
│               │   ├── UserProfilePacket.cs
│               │   ├── RankingPacket.cs
│               │   ├── MatchHistoryPacket.cs
│               │   ├── BestScorePacket.cs
│               │   ├── MatchResultPacket.cs
│               │   └── RoomPacket.cs
│               │
│               ├── Enums/
│               │   ├── Difficulty.cs
│               │   ├── PlayerStatus.cs
│               │   └── RoomStatus.cs
│               │
│               ├── Constants/
│               │   └── PacketTypes.cs
│               │
│               └── Models/
│                   ├── RoomInfo.cs
│                   └── PlayerInfo.cs
├── DOCX/
│   ├── Assignment_Phrase1.docx
│   ├── Assignment_Phrase2.docx
│   └── Assignment_Phrase3.docx
├── Extra/
│   └── gitkeep.git
├── PPTX/
│   └── gitkeep.git
├── .gitignore
└── README.md