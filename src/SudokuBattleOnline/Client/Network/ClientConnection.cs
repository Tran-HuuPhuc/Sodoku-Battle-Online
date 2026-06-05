using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    /// <summary>
    /// quản lý kết nối tcp client
    /// </summary>
    public class ClientConnection
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public event Action<string> OnMessageReceived;
        public event Action OnDisconnected;

        public async Task ConnectAsync(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                _stream = _tcpClient.GetStream();

                // start loop chờ nhận tin nhắn
                _ = ReceiveDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _tcpClient?.Close();
            OnDisconnected?.Invoke();
        }

        public async Task SendMessageAsync(string message)
        {
            if (_stream != null && _stream.CanWrite)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
            }
        }

        private async Task ReceiveDataAsync()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (_tcpClient != null && _tcpClient.Connected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived?.Invoke(message);
                    }
                    else
                    {
                        Disconnect();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }
}
