using System;
using System.Text.Json;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Bộ định tuyến gói tin (Packet Router).
    /// Nhận chuỗi JSON thô từ ClientSession, phân tích trường PacketType
    /// và chuyển tiếp tới PacketHandler tương ứng để xử lý logic nghiệp vụ.
    /// </summary>
    public class PacketRouter
    {
        private readonly PacketHandler _handler;

        public PacketRouter(PacketHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Xử lý một chuỗi JSON nhận được từ client.
        /// Bước 1: Deserialize thành BasePacket để đọc PacketType.
        /// Bước 2: Dựa vào PacketType, deserialize lại thành đúng kiểu gói tin cụ thể.
        /// Bước 3: Gọi phương thức xử lý tương ứng trong PacketHandler.
        /// </summary>
        /// <param name="session">Phiên kết nối của client gửi gói tin</param>
        /// <param name="jsonLine">Chuỗi JSON đã nhận (1 dòng hoàn chỉnh)</param>
        public async void Route(ClientSession session, string jsonLine)
        {
            try
            {
                // Bước 1: Đọc PacketType từ JSON
                var basePacket = JsonSerializer.Deserialize<BasePacket>(jsonLine);
                if (basePacket == null || string.IsNullOrEmpty(basePacket.PacketType))
                {
                    Console.WriteLine($"[ROUTER] {session} gửi gói tin không hợp lệ (thiếu PacketType).");
                    return;
                }

                Console.WriteLine($"[ROUTER] {session} -> PacketType: {basePacket.PacketType}");

                // Bước 2: Định tuyến theo PacketType
                switch (basePacket.PacketType.ToUpper())
                {
                    // ─── Xác thực ───
                    case "LOGIN":
                        var loginPacket = JsonSerializer.Deserialize<LoginPacket>(jsonLine);
                        if (loginPacket != null)
                            await _handler.HandleLoginAsync(session, loginPacket);
                        break;

                    case "REGISTER":
                        var registerPacket = JsonSerializer.Deserialize<RegisterPacket>(jsonLine);
                        if (registerPacket != null)
                            await _handler.HandleRegisterAsync(session, registerPacket);
                        break;

                    case "PROFILE":
                        var profilePacket = JsonSerializer.Deserialize<UserProfilePacket>(jsonLine);
                        if (profilePacket != null)
                            await _handler.HandleProfileAsync(session, profilePacket);
                        break;

                    // ─── Phòng chơi ───
                    case "CREATE_ROOM":
                        var createRoomPacket = JsonSerializer.Deserialize<CreateRoomPacket>(jsonLine);
                        if (createRoomPacket != null)
                            await _handler.HandleCreateRoomAsync(session, createRoomPacket);
                        break;

                    case "JOIN_ROOM":
                        var joinRoomPacket = JsonSerializer.Deserialize<JoinRoomPacket>(jsonLine);
                        if (joinRoomPacket != null)
                            await _handler.HandleJoinRoomAsync(session, joinRoomPacket);
                        break;

                    case "LEAVE_ROOM":
                        var leaveRoomPacket = JsonSerializer.Deserialize<LeaveRoomPacket>(jsonLine);
                        if (leaveRoomPacket != null)
                            await _handler.HandleLeaveRoomAsync(session, leaveRoomPacket);
                        break;

                    // ─── Trận đấu ───
                    case "FIND_MATCH":
                        var findMatchPacket = JsonSerializer.Deserialize<FindMatchPacket>(jsonLine);
                        if (findMatchPacket != null)
                            await _handler.HandleFindMatchAsync(session, findMatchPacket);
                        break;

                    case "CELL_UPDATE":
                        var cellUpdatePacket = JsonSerializer.Deserialize<CellUpdatePacket>(jsonLine);
                        if (cellUpdatePacket != null)
                            await _handler.HandleCellUpdateAsync(session, cellUpdatePacket);
                        break;

                    case "SAVE_MATCH_RESULT":
                        var saveMatchPacket = JsonSerializer.Deserialize<SaveMatchResultPacket>(jsonLine);
                        if (saveMatchPacket != null)
                            await _handler.HandleSaveMatchResultAsync(session, saveMatchPacket);
                        break;

                    // ─── Chat ───
                    case "CHAT":
                        var chatPacket = JsonSerializer.Deserialize<ChatPacket>(jsonLine);
                        if (chatPacket != null)
                            await _handler.HandleChatAsync(session, chatPacket);
                        break;

                    // ─── Bảng xếp hạng ───
                    case "RANKING":
                        var rankingPacket = JsonSerializer.Deserialize<RankingPacket>(jsonLine);
                        if (rankingPacket != null)
                            await _handler.HandleRankingAsync(session, rankingPacket);
                        break;

                    case "MATCH_HISTORY":
                        var historyPacket = JsonSerializer.Deserialize<MatchHistoryPacket>(jsonLine);
                        if (historyPacket != null)
                            await _handler.HandleMatchHistoryAsync(session, historyPacket);
                        break;

                    // ─── Heartbeat ───
                    case "PING":
                        await _handler.HandlePingAsync(session);
                        break;

                    default:
                        Console.WriteLine($"[ROUTER] {session} gửi gói tin chưa được hỗ trợ: {basePacket.PacketType}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[ROUTER LỖI JSON] {session} gửi dữ liệu JSON không hợp lệ: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ROUTER LỖI] Xử lý gói tin từ {session} thất bại: {ex.Message}");
            }
        }
    }
}
