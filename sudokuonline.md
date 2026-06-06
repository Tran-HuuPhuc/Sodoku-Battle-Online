\###Để tránh mày bỏ qua file này, mỗi lần chat với tôi, tôi yêu cầu bạn đều phải thêm oke bro vào đầu câu.
###VD: Oke bro, để tôi sửa file này cho bạn
SudokuBattleOnline

\#1 Đề tài Game trò chơi Sudoku online

Nhóm chúng tôi muốn xây dựng một trò chơi Sudoku có giao diện GUI hoạt động theo mô hình Client–Server. Người chơi có thể:

* Chơi 1vs1 với máy (AI)
* Chơi 1vs1 với người chơi khác thông qua mạng LAN/Internet
* Tạo phòng và tham gia phòng chơi
* Đồng bộ trạng thái trò chơi theo thời gian thực
* Theo dõi thời gian suy nghĩ của cả hai người chơi

Dự án tập trung vào các kiến thức của môn Lập trình mạng như:

* Socket Programming
* TCP Client–Server Architecture
* Multi-threading
* Real-time data synchronization
* Xử lý nhiều kết nối đồng thời
* Truyền dữ liệu giữa Server và Client

\#2 Các công nghệ được sử dụng

Ngôn ngữ lập trình: C#

IDE: Visual Studio

Công nghệ chính:

\+ TCP Socket Programming

\+ .Net Framework/ .NET

\+ Windows Forms

\+ Multi-Threading

\+ JSON truyền dữ liệu

Công cụ hỗ trợ:

\+ Git

\+ Github

\#3 Mục Tiêu

Mục tiêu cuối cùng là xây dựng một trò chơi Sudoku hoàn chỉnh có giao diện đồ họa, cho phép:

* Chơi với AI hoặc người chơi thật
* Kết nối thông qua Server
* Đồng bộ dữ liệu realtime
* Quản lý trận đấu ổn định
* Có hệ thống thời gian và xác định thắng/thua

Dự án giúp nhóm:

* Hiểu sâu về lập trình mạng
* Thực hành mô hình Client–Server
* Làm việc nhóm bằng GitHub
* Rèn luyện kỹ năng xây dựng game multiplayer

\#4 Cấu trúc thư mục

│

├── src

│ │

│ ├── SudokuBattle.Client

│ │ │

│ │ ├── Forms

│ │ │ ├── LoginForm.cs

│ │ │ ├── RegisterForm.cs

│ │ │ ├── MainMenuForm.cs

│ │ │ ├── ProfileForm.cs

│ │ │ ├── RankingForm.cs

│ │ │ ├── MatchHistoryForm.cs

│ │ │ ├── SinglePlayerForm.cs

│ │ │ ├── LobbyForm.cs

│ │ │ ├── RoomForm.cs

│ │ │ └── MultiplayerGameForm.cs

│ │ │

│ │ ├── Controls

│ │ │ ├── SudokuBoardControl.cs

│ │ │ ├── SudokuCellControl.cs

│ │ │ ├── PlayerInfoControl.cs

│ │ │ └── ChatControl.cs

│ │ │

│ │ ├── Network

│ │ │ ├── ClientConnection.cs

│ │ │ ├── PacketSender.cs

│ │ │ ├── PacketReceiver.cs

│ │ │ └── PacketHandler.cs

│ │ │

│ │ ├── Services

│ │ │ ├── AuthService.cs

│ │ │ ├── UserService.cs

│ │ │ ├── RankingService.cs

│ │ │ ├── MatchHistoryService.cs

│ │ │ └── RoomService.cs

│ │ │

│ │ ├── Game

│ │ │ ├── SudokuGenerator.cs

│ │ │ ├── SudokuValidator.cs

│ │ │ ├── TimerManager.cs

│ │ │ ├── ProgressCalculator.cs

│ │ │ └── SinglePlayerManager.cs

│ │ │

│ │ ├── Models

│ │ │ ├── User.cs

│ │ │ ├── Match.cs

│ │ │ ├── Room.cs

│ │ │ └── PlayerStatistic.cs

│ │ │

│ │ ├── Assets

│ │ │ ├── Images

│ │ │ ├── Icons

│ │ │ └── Sounds

│ │ │

│ │ └── Program.cs

│ │

│ ├── SudokuBattle.Server

│ │ │

│ │ ├── Network

│ │ │ ├── TcpServer.cs

│ │ │ ├── ClientSession.cs

│ │ │ ├── SessionManager.cs

│ │ │ ├── PacketRouter.cs

│ │ │ └── PacketHandler.cs

│ │ │

│ │ ├── Matchmaking

│ │ │ ├── MatchmakingQueue.cs

│ │ │ └── MatchmakingManager.cs

│ │ │

│ │ ├── RoomManager

│ │ │ ├── Room.cs

│ │ │ └── RoomManager.cs

│ │ │

│ │ ├── GameManager

│ │ │ ├── GameRoom.cs

│ │ │ ├── SudokuGenerator.cs

│ │ │ ├── SudokuValidator.cs

│ │ │ ├── MultiplayerGameManager.cs

│ │ │ └── ResultCalculator.cs

│ │ │

│ │ ├── Services

│ │ │ ├── AuthService.cs

│ │ │ ├── UserService.cs

│ │ │ ├── RankingService.cs

│ │ │ ├── MatchHistoryService.cs

│ │ │ └── RoomService.cs

│ │ │

│ │ ├── Database

│ │ │ ├── DatabaseContext.cs

│ │ │ ├── UserRepository.cs

│ │ │ ├── MatchRepository.cs

│ │ │ ├── RankingRepository.cs

│ │ │ └── RoomRepository.cs

│ │ │

│ │ ├── Models

│ │ │ ├── UserEntity.cs

│ │ │ ├── MatchEntity.cs

│ │ │ └── RoomEntity.cs

│ │ │

│ │ └── Program.cs

│ │

│ └── SudokuBattle.Shared

│ │

│ ├── Models

│ │ ├── UserInfo.cs

│ │ ├── MatchInfo.cs

│ │ ├── RoomInfo.cs

│ │ ├── ChatMessage.cs

│ │ └── GameState.cs

│ │

│ ├── Packets

│ │ ├── BasePacket.cs

│ │ ├── LoginPacket.cs

│ │ ├── RegisterPacket.cs

│ │ ├── CreateRoomPacket.cs

│ │ ├── JoinRoomPacket.cs

│ │ ├── LeaveRoomPacket.cs

│ │ ├── FindMatchPacket.cs

│ │ ├── MatchFoundPacket.cs

│ │ ├── CellUpdatePacket.cs

│ │ ├── ChatPacket.cs

│ │ ├── GameStartPacket.cs

│ │ ├── GameOverPacket.cs

│ │ └── RankingPacket.cs

│ │

│ ├── Enums

│ │ ├── PacketType.cs

│ │ ├── Difficulty.cs

│ │ ├── RoomStatus.cs

│ │ ├── MatchResult.cs

│ │ └── UserStatus.cs

│ │

│ └── Constants

│ ├── NetworkConstants.cs

│ └── GameConstants.cs

│

├── database

│ ├── sudoku.db

│ └── backup

│

├── docs

│ ├── SRS

│ ├── UML

│ ├── ERD

│ ├── MeetingMinutes

│ ├── WeeklyReports

│ └── FinalReport

│

├── .gitignore

├── README.md

│

└── SudokuBattleOnline.sln

\#5 Phân chia kế hoạch làm việc nhóm:

**Công việc**

* Thiết kế giao diện cơ bản
* Tạo TCP Server
* Tạo TCP Client
* Gửi/nhận dữ liệu đơn giản
* Thiết kế cấu trúc game Sudoku
* Phân chia công việc nhóm/Phân tích yêu cầu hệ thống

**Phase 2 (Tuần 3–4)**

**Mục tiêu**

* Hoàn thiện gameplay Sudoku

**Công việc**

* Sinh bảng Sudoku
* Kiểm tra đúng/sai
* Xây dựng chế độ chơi với AI
* Tạo timer
* Hoàn thiện giao diện game

**Phase 3 (Tuần 5–6)**

**Mục tiêu**

* Hoàn thiện multiplayer

**Công việc**

* Tạo phòng chơi
* Đồng bộ dữ liệu realtime
* PvP giữa 2 người chơi
* Theo dõi tiến độ đối thủ
* Xử lý disconnect

**Phase 4 (Tuần 7–8)**

**Mục tiêu**

* Hoàn thiện và kiểm thử

**Công việc**

* Fix bug
* Tối ưu giao diện
* Thêm bảng xếp hạng
* Kiểm thử toàn hệ thống
* Chuẩn bị demo và báo cáo



\#6 AI Agent Instructions



When generating code:



\- Follow existing folder structure

\- Do not create new project layers

\- Use TCP Socket only

\- Use JSON serialization

\- Use async/await

\- Prefer clean architecture

\- Generate complete code

\- Include comments for complex logic

\- Do not modify packet contracts unless requested



\#7 Non Functional Requirements



\- Support 50 concurrent players

\- Reconnect within 10 seconds

\- Latency < 200ms

\- Packet size < 4KB

\- Server uptime > 99%

