public static class PacketHandler
{
    public static string BuildLogin(string username)
    {
        return $"LOGIN|{username}";
    }

    public static string BuildMessage(string receiver,
                                      string content)
    {
        return $"SEND|{receiver}|{content}";
    }
}