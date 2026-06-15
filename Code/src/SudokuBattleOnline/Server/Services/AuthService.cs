using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    internal class AuthService
    {
        public bool Login(string username, string password)
        {
            return username == "admin"
                && password == "123456";
        }
    }
}
