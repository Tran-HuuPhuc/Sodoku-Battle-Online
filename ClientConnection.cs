using System.Net.Sockets;
public class ClientConnection
{
    public TcpClient Client { get; private set; }

    public void Connect(string ip, int port)
    {
        Client = new TcpClient();
        Client.Connect(ip, port);
    }
}