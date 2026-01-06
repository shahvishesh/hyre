using Hyre.API.Dtos.RecruiterRoundDecesion;
using Hyre.API.Interfaces.RecruiterFeedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecruiterDecisionController : ControllerBase
    {
        private readonly IRecruiterDecisionService _service;

        public RecruiterDecisionController(
            IRecruiterDecisionService service)
        {
            _service = service;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Decide(
            [FromBody] RecruiterRoundDecisionDto dto)
        {
            await _service.ApplyDecisionAsync(dto, GetUserId());
            return Ok(new { message = "Decision applied successfully." });
        }

        [HttpGet("next-round")]
        public async Task<IActionResult> GetNextRoundDetail(
            [FromQuery] int candidateId, 
            [FromQuery] int jobId, 
            [FromQuery] int currentSequenceNo)
        {
            var result = await _service.GetNextRoundDetailAsync(candidateId, jobId, currentSequenceNo);
            
            if (result == null)
                return NotFound(new { message = "No next round found." });
                
            return Ok(result);
        }

        [HttpGet("{roundId}/recruiter-decision")]
        public async Task<IActionResult> GetRecruiterDecision(int roundId)
        {
            var result = await _service.GetRecruiterDecisionAsync(roundId);
            
            if (result == null)
                return NotFound(new { message = "Round not found." });
                
            return Ok(result);
        }
    }
}
