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
    }
}
