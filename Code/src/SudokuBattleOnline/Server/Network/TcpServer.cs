
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SudokuBattle.Server.Network
{
    public class TcpServer
    {
        private TcpListener _listener;
        private readonly int _port = 8888;
        private bool _isRunning;
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();

        public async Task StartAsync()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;

                Console.WriteLine($"[SERVER] Sudoku TCP Server đã mở thành công tại cổng {_port}...");
                Console.WriteLine("[SERVER] Đang sẵn sàng đón nhận các client kết nối vào...\n---");

                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    lock (_connectedClients)
                    {
                        _connectedClients.Add(client);
                    }
                    Console.WriteLine($"[KẾT NỐI] Có người chơi mới tham gia! Tổng số trực tuyến: {_connectedClients.Count}");

                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI CRITICAL] Hệ thống Server gặp sự cố: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                try
                {
                    string jsonLine;
                    while ((jsonLine = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(jsonLine)) continue;

                        Console.WriteLine($"[SERVER NHẬN JSON]: {jsonLine}");

                        var basePacket = JsonSerializer.Deserialize<BasePacket>(jsonLine);

                        switch (basePacket?.PacketType)
                        {
                            case "LOGIN":
                                var loginData = JsonSerializer.Deserialize<LoginPacket>(jsonLine);
                                Console.WriteLine($"[LOGIC] Người chơi '{loginData.Username}' yêu cầu Đăng Nhập!");
                                break;

                            default:
                                Console.WriteLine("[CẢNH BÁO] Gói tin không xác định.");
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    // Xử lý ngắt kết nối
                }
                finally
                {
                    lock (_connectedClients)
                    {
                        _connectedClients.Remove(client);
                    }
                    client.Close();
                    Console.WriteLine($"[NGẮT KẾT NỐI] Một client đã rời đi. Còn lại: {_connectedClients.Count}");
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            Console.WriteLine("[SERVER] Đã dừng lắng nghe hệ thống.");
        }
    }
}