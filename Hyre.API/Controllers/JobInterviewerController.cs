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
            try
            {

                var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _service.AssignInterviewersAsync(dto, recruiterId);

                return Ok(new { message = "Interviewers assigned successfully." });
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetAssignedInterviewers(int jobId)
        {
            try
            {
                var list = await _service.GetAssignedInterviewersAsync(jobId);
                return Ok(list);

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{jobId}/interviewers")]
        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> GetInterviewers(int jobId, [FromQuery] string role)
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                    return BadRequest("Role must be provided. Example: ?role=Technical");

                var result = await _service.GetInterviewersByRoleAsync(jobId, role);
                return Ok(result);

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveInterviewer([FromQuery] int jobId, [FromQuery] string interviewerId)
        {
            try
            {
                await _service.RemoveInterviewerAsync(jobId, interviewerId);
                return Ok(new { message = "Interviewer removed successfully." });

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
