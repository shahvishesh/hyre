using Hyre.API.Exceptions;
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

        [HttpGet("{candidateId}/resume/{jobId}")]
        [Authorize(Roles = "Reviewer,Recruiter,Admin,HR")]
        public async Task<IActionResult> GetResume(int candidateId, int jobId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var userRoles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var fileBytes = await _service
                    .GetCandidateResumeAsync(candidateId, userId, jobId, userRoles);

                return File(fileBytes, "application/pdf");
            }
            catch (ForbiddenAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("reviewer/jobs")]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> GetJobsAssignedToReviewer()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }

                var jobs = await _service.GetJobsAssignedToReviewerAsync(userId);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
