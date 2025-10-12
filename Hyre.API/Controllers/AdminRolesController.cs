using Hyre.API.Dtos.Role;
using Hyre.API.Models;
using Hyre.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminRolesController : ControllerBase
    {
        private readonly AdminRolesService _adminService;

        public AdminRolesController(AdminRolesService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var user = await _adminService.GetUserRolesAsync(email);
            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [HttpPost("assign-roles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
        {
            try
            {
                var message = await _adminService.AssignMultipleRolesAsync(dto);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("remove-roles")]
        public async Task<IActionResult> RemoveRoles([FromBody] RemoveRolesDto dto)
        {
            try
            {
                var message = await _adminService.RemoveMultipleRolesAsync(dto);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}