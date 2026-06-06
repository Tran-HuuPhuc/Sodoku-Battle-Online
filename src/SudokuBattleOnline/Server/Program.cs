using System;
using System.Text;
using System.Threading.Tasks;
using SudokuBattle.Server.Network;

namespace SudokuBattle.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Đồng bộ định dạng hiển thị ký tự UTF-8 trên Console
            Console.OutputEncoding = Encoding.UTF8;

            TcpServer server = new TcpServer();
            // Kích hoạt Server chạy bất đồng bộ
            await server.StartAsync();
        }
    }
}