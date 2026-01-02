using Hyre.API.Dtos.CandidateMatching;
using Hyre.API.Interfaces.CandidateMatching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateJobController : ControllerBase
    {
        private readonly ICandidateJobService _candidateJobService;

        public CandidateJobController(ICandidateJobService candidateJobService)
        {
            _candidateJobService = candidateJobService;
        }

        [HttpPost("{jobId}/link")]
        public async Task<IActionResult> LinkCandidateToJob(int jobId, [FromBody] CreateCandidateLinkDto dto)
        {
            try
            {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing user token." });

            var result = await _candidateJobService.LinkCandidateAsync(jobId, dto, userId);

            return Ok(new { message = "Candidate linked successfully.", result });
            }
            catch (Exception ex) { 
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Recruiter,Admin,HR,Interviewer,Reviewer")]
        [HttpGet("{jobId}/candidates")]
        public async Task<IActionResult> GetLinkedCandidates(int jobId)
        {
            try
            {
                var result = await _candidateJobService.GetLinkedCandidatesAsync(jobId);
                return Ok(new
                {
                    message = "Linked candidates retrieved successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
