using Hyre.API.Interfaces.ReviewerJob;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Hyre.API.Dtos.ReviewerJob.ReviewerJobDtos;

namespace Hyre.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobReviewerController : ControllerBase
    {
        private readonly IJobReviewerService _service;

        public JobReviewerController(IJobReviewerService service)
        {
            _service = service;
        }

        // Assign reviewers to a job
        [HttpPost("assign")]
        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> AssignReviewers([FromBody] AssignReviewerDto dto)
        {
            try
            {

                var recruiterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (recruiterId == null)
                    return Unauthorized();

                await _service.AssignReviewersAsync(dto, recruiterId);
                return Ok(new { message = "Reviewers assigned successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Get assigned reviewers for a job
        [HttpGet("{jobId}")]
        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> GetReviewersByJob(int jobId)
        {
            try
            {
                var reviewers = await _service.GetJobReviewersAsync(jobId);
                return Ok(reviewers);

            }catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Remove reviewer
        [HttpDelete("{jobId:int}/{reviewerId}")]
        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> RemoveReviewer(int jobId, string reviewerId)
        {
            try
            {
                await _service.RemoveReviewerAsync(jobId, reviewerId);
                return Ok(new { message = "Reviewer removed successfully" });

            }catch( Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Get jobs by reviewer assignment status
        [HttpGet("jobs")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetJobsByReviewerStatus([FromQuery] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    return BadRequest(new { message = "Status query parameter is required. Use 'pending' or 'completed'." });

                var jobs = await _service.GetJobsByReviewerStatusAsync(status);
                return Ok(jobs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
