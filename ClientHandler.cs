using System.Net.Sockets;

public class ClientHandler
{
    public string Username { get; set; }

    public System.Net.Sockets.TcpClient Client { get; set; }
}