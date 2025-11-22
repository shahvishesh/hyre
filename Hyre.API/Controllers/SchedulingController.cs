using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Scheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulingController : ControllerBase
    {
        private readonly ICandidateInterviewService _service;

        public SchedulingController(ICandidateInterviewService service)
        {
            _service = service;
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> Schedule([FromBody] CreateCandidateInterviewDto dto)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User id missing");
            try
            {
                var results = await _service.ScheduleRoundsAsync(dto, recruiterId);
                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
