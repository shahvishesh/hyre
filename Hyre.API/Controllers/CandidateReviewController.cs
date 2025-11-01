using Hyre.API.Interfaces.CandidateReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Hyre.API.Dtos.CandidateReview.ReviewDtos;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateReviewController : ControllerBase
    {
        private readonly ICandidateReviewService _service;

        public CandidateReviewController(ICandidateReviewService service)
        {
            _service = service;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost("create")]
        [Authorize(Roles = "Reviewer,Recruiter")]
        public async Task<IActionResult> CreateReview(CreateReviewDto dto)
        {
            var result = await _service.CreateReviewAsync(dto, GetUserId());
            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> UpdateReview(UpdateReviewDto dto)
        {
            var result = await _service.UpdateReviewAsync(dto, GetUserId());
            return Ok(result);
        }

        [HttpPost("comment")]
        [Authorize(Roles = "Reviewer,Recruiter")]
        public async Task<IActionResult> AddComment(AddCommentDto dto)
        {
            await _service.AddCommentAsync(dto, GetUserId());
            return Ok(new { Message = "Comment added successfully" });
        }

        [HttpPut("recruiter-decision")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> ApplyRecruiterDecision(RecruiterDecisionDto dto)
        {
            await _service.ApplyRecruiterDecisionAsync(dto, GetUserId());
            return Ok(new { Message = "Recruiter decision applied successfully" });
        }

        [HttpGet("job/{jobId}")]
        [Authorize(Roles = "Recruiter,Reviewer")]
        public async Task<IActionResult> GetReviewsByJob(int jobId)
        {
            var reviews = await _service.GetReviewsByJobAsync(jobId);
            return Ok(reviews);
        }
    }
}
