using Shared.Enums;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SudokuBattle.Shared.Models;

namespace SudokuBattleOnline.Shared.Packets
{
    public class RoomListPacket : BasePacket
    {
        public List<RoomInfo> Rooms { get; set; } = new();

        public RoomListPacket()
        {
            PacketType = "GET_ROOM_LIST";
        }
    }

}
