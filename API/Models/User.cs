namespace API.Models;

public class User
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public string? Role { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool IsLockedOut { get; set; }
}
