using Server.Database;
using System;
using System.Security.Cryptography;

namespace Server.Services
{
    public class AuthService
    {
        private const int PasswordIterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        private readonly UserRepository _users;

        public AuthService(DatabaseContext databaseContext)
        {
            databaseContext.Initialize();
            _users = new UserRepository(databaseContext);
        }

        public bool Register(string username, string password, out string message)
        {
            username = username.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                message = "Username không được để trống.";
                return false;
            }

            if (username.Length < 3)
            {
                message = "Username phải có ít nhất 3 ký tự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                message = "Password không được để trống.";
                return false;
            }

            if (password.Length < 6)
            {
                message = "Password phải có ít nhất 6 ký tự.";
                return false;
            }

            string passwordHash = HashPassword(password);
            bool created = _users.CreateUser(username, passwordHash);
            if (!created)
            {
                message = "Username này đã tồn tại.";
                return false;
            }

            message = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return true;
        }

        public bool Login(string username, string password, out string message)
        {
            username = username.Trim();

            var user = _users.FindByUsername(username);
            if (user == null)
            {
                message = "Tài khoản không tồn tại.";
                return false;
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                message = "Sai mật khẩu.";
                return false;
            }

            message = $"Đăng nhập thành công! Chào mừng {username}.";
            return true;
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                PasswordIterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"PBKDF2${PasswordIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            string[] parts = storedHash.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2")
                return false;

            if (!int.TryParse(parts[1], out int iterations))
                return false;

            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
