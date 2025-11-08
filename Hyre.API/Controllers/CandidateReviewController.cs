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

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost("create")]
        [Authorize(Roles = "Reviewer,Recruiter")]
        public async Task<IActionResult> CreateReview(CreateReviewDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }
                var result = await _service.CreateReviewAsync(dto, userId);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> UpdateReview(UpdateReviewDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }
                var result = await _service.UpdateReviewAsync(dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("comment")]
        [Authorize(Roles = "Reviewer,Recruiter")]
        public async Task<IActionResult> AddComment(AddCommentDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }
                await _service.AddCommentAsync(dto, userId);
                return Ok(new { Message = "Comment added successfully" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("recruiter-decision")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> ApplyRecruiterDecision(RecruiterDecisionDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }
                await _service.ApplyRecruiterDecisionAsync(dto, userId);
                return Ok(new { Message = "Recruiter decision applied successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("job/{jobId}")]
        [Authorize(Roles = "Recruiter,Reviewer")]
        public async Task<IActionResult> GetReviewsByJob(int jobId)
        {
            try
            {

                var reviews = await _service.GetReviewsByJobAsync(jobId);
                return Ok(reviews);
            }catch(Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
