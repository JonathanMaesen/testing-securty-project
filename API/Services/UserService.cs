using API.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly ConcurrentDictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);
        private const int MaxFailedAttempts = 3;

        public bool RegisterUser(string username, string password, string role)
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            // Simplified role validation: Default to "Player" unless "Admin" is specified (case-insensitive).
            var assignedRole = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Player";

            // Generate a salt
            var salt = GenerateSalt();
            // Hash het wachtwoord met SHA-256
            var passwordHash = HashPassword(password, salt);

            // Maak nieuwe user
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                Role = assignedRole,
                FailedLoginAttempts = 0,
                IsLockedOut = false
            };
            
            // If the key (username) already exists, TryAdd returns false.
            return _users.TryAdd(username, user);
        }

        public User? GetUser(string username)
        {
            _users.TryGetValue(username, out var user);
            return user;
        }

        public bool ValidatePassword(string username, string password)
        {
            var user = GetUser(username);
            if (user == null) return false;

            var hashedInput = HashPassword(password, user.PasswordSalt);
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
        
        private string GenerateSalt()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToHexString(bytes);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash);
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _users.Values;
        }
    }
}
