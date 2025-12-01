using API.Models;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users = new List<User>();
        private const int MaxFailedAttempts = 3;

        public bool RegisterUser(string username, string password, string role)
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            // Check of username al bestaat
            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Valideer rol
            if (role != "Player" && role != "Admin")
                role = "Player";

            // Hash het wachtwoord met SHA-256
            var passwordHash = HashPassword(password);

            // Maak nieuwe user
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = role,
                FailedLoginAttempts = 0,
                IsLockedOut = false
            };

            _users.Add(user);
            return true;
        }

        public User? GetUser(string username)
        {
            return _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public bool ValidatePassword(string username, string password)
        {
            var user = GetUser(username);
            if (user == null) return false;

            var hashedInput = HashPassword(password);
            return user.PasswordHash == hashedInput;
        }

        public void IncrementFailedAttempts(string username)
        {
            var user = GetUser(username);
            if (user == null) return;

            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.IsLockedOut = true;
            }
        }

        public void ResetFailedAttempts(string username)
        {
            var user = GetUser(username);
            if (user == null) return;

            user.FailedLoginAttempts = 0;
            user.IsLockedOut = false;
        }

        public bool IsLockedOut(string username)
        {
            var user = GetUser(username);
            return user?.IsLockedOut ?? false;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash);
            }
        }
    }
}
