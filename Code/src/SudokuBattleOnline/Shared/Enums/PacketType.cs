namespace Shared.Enums
{
    public enum PacketType
    {
        Login,
        LoginResult,

        Register,
        RegisterResult,

        CreateRoom,
        JoinRoom,
        LeaveRoom,

        FindMatch,
        MatchFound,

        CellUpdate,

        Chat,

        StartGame,
        EndGame,

        Ping,
        Pong
    }
}