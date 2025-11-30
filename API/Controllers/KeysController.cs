using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KeysController : ControllerBase
    {
        private readonly IKeyService _keyService;

        public KeysController(IKeyService keyService)
        {
            _keyService = keyService;
        }

        [HttpGet("keyshare/{roomId}")]
        public IActionResult GetKeyShare(string roomId)
        {
            // Haal rol uit token
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Ongeldige token" });
            }

            // Validatie roomId
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return BadRequest(new { message = "RoomId is verplicht" });
            }

            // Probeer keyshare op te halen
            var keyShare = _keyService.GetKeyShare(roomId, userRole);

            if (keyShare == null)
            {
                return NotFound(new { message = "Room niet gevonden of geen toegang" });
            }

            return Ok(keyShare);
        }
    }
}
