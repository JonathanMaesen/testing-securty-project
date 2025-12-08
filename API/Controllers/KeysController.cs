using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require JWT for all endpoints here
    public class KeysController : ControllerBase
    {
        // In a real app, these would be in a database.
        // For this project, hardcoded shares match the assignment structure.
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
                return NotFound(new { message = "Keyshare not found for this room." });
            }

            // Authorization Check: Only Admins can access the Admin Room keyshare
            if (roomId == "room_admin")
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (role != "Admin")
                {
                    return StatusCode(403, new { message = "Access Denied: You need the 'Admin' role to access this keyshare." });
                }
            }

            return Ok(new { keyshare = share });
        }
    }
}