using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Vereis JWT voor alle endpoints in deze controller
    public class KeysController : ControllerBase
    {
        // In een echte applicatie zouden deze in een database staan.
        // Voor dit project zijn de hardgecodeerde shares conform de opdracht.
        private static readonly Dictionary<string, string> KeyShares = new()
        {
            // Room ID -> Keyshare
            { "room_secret", "Share_For_Players_123" },
            { "room_admin", "Share_For_Admins_999" }
        };

        [HttpGet("keyshare/{roomId}")]
        public IActionResult GetKeyShare(string roomId)
        {
            if (!KeyShares.TryGetValue(roomId, out var share))
            {
                return NotFound(new { message = "Keyshare niet gevonden voor deze kamer." });
            }

            // Autorisatiecheck: Alleen Admins mogen de -keyshare van de Admin Room opvragen
            if (roomId == "room_admin")
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (role != "Admin")
                {
                    return StatusCode(403, new { message = "Toegang geweigerd: U heeft de 'Admin' rol nodig om deze keyshare te verkrijgen." });
                }
            }

            return Ok(new { keyshare = share });
        }
    }
}