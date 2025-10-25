using Hyre.API.Dtos.CandidateMatching;
using Hyre.API.Interfaces.CandidateMatching;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing user token." });

            var result = await _candidateJobService.LinkCandidateAsync(jobId, dto, userId);

            return Ok(new { message = "Candidate linked successfully.", result });
        }
    }
}
