using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Protect this entire controller. Only authorized users can access these endpoints.
    [Authorize]
    public class UsersController(IUserService userService) : ControllerBase
    {
        // GET: api/users
        [HttpGet]
        // Further restrict this endpoint to only users in the "Admin" role.
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            var users = userService.GetAllUsers();

            // Map the internal User model to the public UserDto
            var userDtos = users.Select(u => new UserDto
            {
                Username = u.Username,
                Role = u.Role
            }).ToList();

            return Ok(userDtos);
        }
    }
}