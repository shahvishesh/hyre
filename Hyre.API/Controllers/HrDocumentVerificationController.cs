using Hyre.API.Interfaces.DocumentVerify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Controllers
{
    [Route("api/hr/verifications")]
    [Authorize(Roles = "HR")]
    [ApiController]
    public class HrDocumentVerificationController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public HrDocumentVerificationController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet("jobs")]
        public async Task<ActionResult<List<DocumentJobDto>>> GetJobsWithPendingVerifications()
        {
            try
            {
                var jobs = await _documentService.GetJobsWithPendingVerificationsAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("jobs/{jobId}/candidates")]
        public async Task<ActionResult<List<CandidateDetailDto>>> GetCandidatesByVerificationStatus(
            int jobId,
            [FromQuery] string status)
        {
            try
            {
                var validStatuses = new[] { "ReuploadRequired", "UnderVerification", "Completed" };
                if (string.IsNullOrEmpty(status) || !validStatuses.Contains(status))
                {
                    return BadRequest(new { message = "Invalid status. Must be one of: ReuploadRequired, UnderVerification, Completed" });
                }

                var candidates = await _documentService.GetCandidatesByVerificationStatusAsync(jobId, status);
                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{verificationId}")]
        public async Task<IActionResult> GetVerificationDetail(int verificationId)
        {
            try
            {

                var result = await _documentService
                    .GetVerificationForHrAsync(verificationId);

                return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("action")]
        public async Task<IActionResult> ProcessAction(
        [FromBody] HrVerificationActionDto dto)
        {
            try
            {
                string hrUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
                if (string.IsNullOrEmpty(hrUserId))
                    return Unauthorized("hrUserId not found in token");

                await _documentService
                    .ProcessHrActionAsync(hrUserId, dto);

                return Ok(new ApiResponse(
                    true,
                    "Action processed successfully"
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(
                    false,
                    ex.Message
                ));
            }
        }
    }
}
