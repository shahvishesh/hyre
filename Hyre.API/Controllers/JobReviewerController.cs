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

    }
}
