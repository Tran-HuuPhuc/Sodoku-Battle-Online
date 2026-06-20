using Client.Network;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SudokuBattleOnline.Client
{
    /// <summary>
    /// Lưu phiên làm việc chung của Client: kết nối Server và user đang đăng nhập.
    /// </summary>
    public static class AppSession
    {
        public static string ServerIp { get; set; } = "127.0.0.1";
        public static int ServerPort { get; set; } = 8888;
        public static string CurrentUsername { get; set; } = string.Empty;

        public static ClientConnection Connection { get; } = new ClientConnection();

        public static bool IsConnected => Connection.IsConnected;
        public static bool IsLoggedIn => !string.IsNullOrWhiteSpace(CurrentUsername);

        public static event Action<BasePacket, string>? PacketReceived;

        static AppSession()
        {
            Connection.OnMessageReceived += HandleRawMessage;
            Connection.OnDisconnected += () =>
            {
                CurrentUsername = string.Empty;
            };
        }

        public static async Task EnsureConnectedAsync()
        {
            if (!Connection.IsConnected)
                await Connection.ConnectAsync(ServerIp, ServerPort);
        }

        public static async Task SendPacketAsync(BasePacket packet)
        {
            await EnsureConnectedAsync();
            await Connection.SendPacketAsync(packet);
        }

        public static async Task<T?> SendAndWaitAsync<T>(BasePacket packet, string expectedPacketType, int timeoutMs = 8000)
            where T : BasePacket
        {
            await EnsureConnectedAsync();

            var tcs = new TaskCompletionSource<T?>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Handler(BasePacket basePacket, string rawJson)
            {
                if (!string.Equals(basePacket.PacketType, expectedPacketType, StringComparison.OrdinalIgnoreCase))
                    return;

                try
                {
                    T? result = JsonSerializer.Deserialize<T>(rawJson);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            PacketReceived += Handler;
            try
            {
                await Connection.SendPacketAsync(packet);

                Task completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
                if (completed != tcs.Task)
                    throw new TimeoutException("Server không phản hồi đúng thời gian cho phép.");

                return await tcs.Task;
            }
            finally
            {
                PacketReceived -= Handler;
            }
        }

        private static void HandleRawMessage(string rawJson)
        {
            try
            {
                BasePacket? basePacket = JsonSerializer.Deserialize<BasePacket>(rawJson);
                if (basePacket == null || string.IsNullOrWhiteSpace(basePacket.PacketType))
                    return;

                PacketReceived?.Invoke(basePacket, rawJson);
            }
            catch
            {
                // Bỏ qua gói tin không hợp lệ.
            }
        }
    }
}
