using System.Net;
using System.Net.Sockets;
public class ServerListener
{
    private TcpListener listener;

    public void Start()
    {
        listener =
            new TcpListener(IPAddress.Any, 5000);

        listener.Start();
    }
}