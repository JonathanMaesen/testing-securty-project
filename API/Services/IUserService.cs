using API.Models;

namespace API.Services
{
    public interface IUserService
    {
        bool RegisterUser(string username, string password, string role);
        User? GetUser(string username);
        bool ValidatePassword(string username, string password);
        void IncrementFailedAttempts(string username);
        void ResetFailedAttempts(string username);
        bool IsLockedOut(string username);
        IEnumerable<User> GetAllUsers();
    }
}
