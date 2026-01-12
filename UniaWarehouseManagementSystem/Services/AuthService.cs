using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UniaWarehouseManagementSystem.Data;
using UniaWarehouseManagementSystem.Models;

namespace UniaWarehouseManagementSystem.Services
{
    public static class AuthService
    {
        // Tu przechowujemy zalogowanego użytkownika (sesja)
        public static User? CurrentUser { get; private set; }

        public static bool Login(string username, string password)
        {
            using (var context = new UniaDbContext())
            {
                var hash = HashPassword(password);

                // Szukamy użytkownika z takim loginem i hasłem
                var user = context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);

                if (user != null)
                {
                    CurrentUser = user;
                    return true;
                }
                return false;
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        // Prosta metoda szyfrująca (SHA256)
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}