using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username en password zijn verplicht" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password moet minimaal 6 karakters zijn" });
            }

            // Probeer te registreren
            var success = _userService.RegisterUser(request.Username, request.Password, request.Role);

            if (!success)
            {
                return BadRequest(new { message = "Username bestaat al" });
            }

            return Ok(new { message = "Registratie succesvol", username = request.Username });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username en password zijn verplicht" });
            }

            // Check lockout
            if (_userService.IsLockedOut(request.Username))
            {
                return Unauthorized(new { message = "Account is vergrendeld na 3 mislukte pogingen" });
            }

            // Valideer wachtwoord
            if (!_userService.ValidatePassword(request.Username, request.Password))
            {
                _userService.IncrementFailedAttempts(request.Username);
                var user = _userService.GetUser(request.Username);

                if (user != null && user.IsLockedOut)
                {
                    return Unauthorized(new { message = "Account is nu vergrendeld na 3 mislukte pogingen" });
                }

                return Unauthorized(new { message = "Ongeldige inloggegevens" });
            }

            // Reset failed attempts
            _userService.ResetFailedAttempts(request.Username);

            // Haal user op
            var loggedInUser = _userService.GetUser(request.Username);
            if (loggedInUser == null)
            {
                return Unauthorized(new { message = "Gebruiker niet gevonden" });
            }

            // Genereer JWT token
            var token = GenerateJwtToken(loggedInUser);

            return Ok(new LoginResponse
            {
                Token = token,
                Username = loggedInUser.Username,
                Role = loggedInUser.Role
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Geen geldige token" });
            }

            return Ok(new
            {
                username = username,
                role = role
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? "DitIsEenGeheimeSleutelVoorJWT2024MinimaalDertigKarakters";
            var jwtIssuer = "TextAdventureAPI";
            var jwtAudience = "TextAdventureClient";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
