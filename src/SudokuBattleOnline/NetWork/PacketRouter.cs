using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
public static class PacketRouter
{
    public static void Route(
        Dictionary<string, TcpClient> clients,
        string sender,
        string receiver,
        string content)
    {
        if (!clients.ContainsKey(receiver))
            return;

        string msg =
            $"[{sender}] {content}";

        byte[] data =
            Encoding.UTF8.GetBytes(msg);

        clients[receiver]
            .GetStream()
            .Write(data, 0, data.Length);
    }
}