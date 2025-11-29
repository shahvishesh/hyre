using Hyre.API.Dtos.Scheduling;
using Hyre.API.Interfaces.Scheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateRoundsController : ControllerBase
    {
        private readonly ICandidateRoundService _service;
        public CandidateRoundsController(ICandidateRoundService service) => _service = service;

        [HttpGet("{candidateId}/job/{jobId}")]
        public async Task<IActionResult> GetRounds(int candidateId, int jobId)
        {
            var rounds = await _service.GetCandidateRoundsAsync(candidateId, jobId);
            return Ok(rounds);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Upsert([FromBody] CandidateRoundsUpdateDto dto)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            var updated = await _service.UpsertCandidateRoundsAsync(dto, recruiterId);
            return Ok(updated);
        }
    }
}
