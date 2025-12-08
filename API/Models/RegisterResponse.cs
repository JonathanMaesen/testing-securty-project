namespace API.Models;

public class RegisterResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
}
