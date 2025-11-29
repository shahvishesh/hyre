using Hyre.API.Dtos.InterviewerJob;
using Hyre.API.Interfaces.InterviewerJob;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobInterviewerController : ControllerBase
    {
        private readonly IJobInterviewerService _service;

        public JobInterviewerController(IJobInterviewerService service)
        {
            _service = service;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignInterviewers([FromBody] AssignInterviewersDto dto)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _service.AssignInterviewersAsync(dto, recruiterId);

            return Ok(new { message = "Interviewers assigned successfully." });
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetAssignedInterviewers(int jobId)
        {
            var list = await _service.GetAssignedInterviewersAsync(jobId);
            return Ok(list);
        }

        [HttpGet("{jobId}/interviewers")]
        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> GetInterviewers(int jobId, [FromQuery] string role)
        {
            if (string.IsNullOrEmpty(role))
                return BadRequest("Role must be provided. Example: ?role=Technical");

            var result = await _service.GetInterviewersByRoleAsync(jobId, role);
            return Ok(result);
        }


        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveInterviewer([FromQuery] int jobId, [FromQuery] string interviewerId)
        {
            await _service.RemoveInterviewerAsync(jobId, interviewerId);
            return Ok(new { message = "Interviewer removed successfully." });
        }
    }
}
