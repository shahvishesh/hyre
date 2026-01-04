using Hyre.API.Dtos.Feedback;
using Hyre.API.Interfaces.CandidateFeedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewFeedbackController : ControllerBase
    {
        private readonly IInterviewFeedbackService _service;

        public InterviewFeedbackController(IInterviewFeedbackService service)
        {
            _service = service;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(
            [FromBody] SubmitFeedbackDto dto)
        {
            await _service.SubmitFeedbackAsync(dto, GetUserId());
            return Ok(new { message = "Feedback submitted successfully." });
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            return Ok(await _service.GetMyFeedbacksAsync(GetUserId()));
        }

        [HttpGet("round/{roundId}")]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> GetFeedbacksForRound(int roundId)
        {
            return Ok(await _service.GetFeedbacksForRoundAsync(roundId));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingFeedbackAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompleted()
        {
            var result = await _service.GetCompletedFeedbackAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetInterviewerJobs()
        {
            var result = await _service.GetInterviewerJobsAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("job/{jobId}/candidates")]
        public async Task<IActionResult> GetInterviewedCandidatesForJob(int jobId)
        {
            var result = await _service.GetInterviewedCandidatesForJobAsync(jobId, GetUserId());
            return Ok(result);
        }

        [HttpGet("job/{jobId}/candidate/{candidateId}/pending")]
        public async Task<IActionResult> GetPendingFeedbackForCandidateJob(int jobId, int candidateId)
        {
            var result = await _service.GetPendingFeedbackForCandidateJobAsync(candidateId, jobId, GetUserId());
            return Ok(result);
        }

        [HttpGet("job/{jobId}/candidate/{candidateId}/completed")]
        public async Task<IActionResult> GetCompletedFeedbackForCandidateJob(int jobId, int candidateId)
        {
            var result = await _service.GetCompletedFeedbackForCandidateJobAsync(candidateId, jobId, GetUserId());
            return Ok(result);
        }

        [HttpGet("round-detail/{candidateRoundId}")]
        public async Task<IActionResult> GetRoundDetail(int candidateRoundId)
        {
            var result = await _service.GetRoundDetailAsync(candidateRoundId, GetUserId());
            return Ok(result);
        }

        [HttpGet("round/{candidateRoundId}/my-feedback")]
        public async Task<IActionResult> GetMyFeedbackForRound(int candidateRoundId)
        {
            var result = await _service.GetMyFeedbackForRoundAsync(candidateRoundId, GetUserId());
            return Ok(result);
        }
    }
}
