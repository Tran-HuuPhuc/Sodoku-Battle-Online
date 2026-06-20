using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SudokuBattleOnline.Shared.Packets;

namespace Client.Network
{
    /// <summary>
    /// Quản lý kết nối TCP từ Client tới Server.
    /// Client gửi/nhận packet JSON, mỗi packet kết thúc bằng một dòng mới.
    /// </summary>
    public class ClientConnection
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public event Action<string>? OnMessageReceived;
        public event Action? OnDisconnected;

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected && _stream != null;

        public async Task ConnectAsync(string ip, int port)
        {
            if (IsConnected)
                return;

            Disconnect(false);

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(ip, port);

            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream, new UTF8Encoding(false));
            _writer = new StreamWriter(_stream, new UTF8Encoding(false))
            {
                AutoFlush = true
            };

            _ = Task.Run(ReceiveDataAsync);
        }

        public void Disconnect()
        {
            Disconnect(true);
        }

        private void Disconnect(bool notify)
        {
            try { _reader?.Dispose(); } catch { }
            try { _writer?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _tcpClient?.Close(); } catch { }

            _reader = null;
            _writer = null;
            _stream = null;
            _tcpClient = null;

            if (notify)
                OnDisconnected?.Invoke();
        }

        /// <summary>
        /// Gửi packet sang Server. Phải serialize theo kiểu thật của object
        /// để không mất các field con như Username, Password, Rankings...
        /// </summary>
        public async Task SendPacketAsync(BasePacket packet)
        {
            if (!IsConnected || _writer == null)
                throw new InvalidOperationException("Client chưa kết nối tới Server.");

            await _writeLock.WaitAsync();
            try
            {
                string jsonString = JsonSerializer.Serialize(packet, packet.GetType());
                await _writer.WriteLineAsync(jsonString);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task ReceiveDataAsync()
        {
            try
            {
                while (IsConnected && _reader != null)
                {
                    string? line = await _reader.ReadLineAsync();
                    if (line == null)
                        break;

                    if (!string.IsNullOrWhiteSpace(line))
                        OnMessageReceived?.Invoke(line);
                }
            }
            catch
            {
                // Kết nối bị ngắt hoặc stream đóng.
            }
            finally
            {
                Disconnect(true);
            }
        }
    }
}
