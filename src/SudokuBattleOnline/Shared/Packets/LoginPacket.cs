using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuBattleOnline.Shared.Packets
{
    public class LoginPacket : BasePacket
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public LoginPacket()
        {
            PacketType = "LOGIN";
        }
    }
}
