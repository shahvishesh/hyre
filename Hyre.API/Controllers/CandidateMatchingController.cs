using Hyre.API.Interfaces.CandidateMatching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateMatchingController : ControllerBase
    {
        private readonly ICandidateMatchingService _matchingService;

        public CandidateMatchingController(ICandidateMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        [HttpGet("{jobId}/candidates/match")]
        public async Task<IActionResult> GetMatchingCandidates(int jobId)
        {
            try
            {
                var result = await _matchingService.GetMatchingCandidatesAsync(jobId);
                return Ok(result);

            }catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
