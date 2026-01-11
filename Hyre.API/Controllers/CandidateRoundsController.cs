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
            try
            {
                var rounds = await _service.GetCandidateRoundsAsync(candidateId, jobId);
                return Ok(rounds);

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Upsert([FromBody] CandidateRoundsUpdateDto dto)
        {
            try
            {
                var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

                var updated = await _service.UpsertCandidateRoundsAsync(dto, recruiterId);
                return Ok(updated);

            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobsWithSchedulingState()
        {
            try
            {
                var jobs = await _service.GetJobsWithSchedulingStateAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("job/{jobId}/candidates")]
        public async Task<IActionResult> GetCandidatesBySchedulingStatus(int jobId, [FromQuery] string status)
        {
            try
            {
                var candidates = await _service.GetCandidatesBySchedulingStatusAsync(jobId, status);
                return Ok(candidates);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /*---------------------------------------------------------------------------------------------*/
        [HttpPost("single")]
        public async Task<IActionResult> UpsertSingleRound([FromBody] SingleCandidateRoundDto dto)
        {
            try
            {
                var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

                var result = await _service.UpsertSingleRoundAsync(dto, recruiterId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{roundId}")]
        public async Task<IActionResult> DeleteRound(int roundId)
        {
            try
            {
                var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

                var deleted = await _service.DeleteRoundAsync(roundId, recruiterId);
                return Ok(deleted);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("validate-save/{candidateId}/job/{jobId}")]
        public async Task<IActionResult> ValidateForSave(int candidateId, int jobId)
        {
            try
            {
                var validation = await _service.ValidateRoundsForSaveAsync(candidateId, jobId);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
