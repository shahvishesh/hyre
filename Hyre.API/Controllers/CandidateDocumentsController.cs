using Hyre.API.Interfaces.DocumentVerify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateDocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public CandidateDocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        private string GetUserId()=> User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

        // GET api/candidate/documents/required?jobId=5
        [Authorize(Roles = "Candidate")]
        [HttpGet("required")]
        public async Task<IActionResult> GetRequiredDocuments([FromQuery] int jobId)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("UserId not found in token");

                var result = await _documentService.GetRequiredDocumentsAsync(userId, jobId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
