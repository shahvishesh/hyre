using Hyre.API.Interfaces.DocumentVerify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
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
    }
}
