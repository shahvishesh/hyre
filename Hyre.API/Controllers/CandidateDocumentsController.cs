using Hyre.API.Interfaces.DocumentVerify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

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

        [Authorize(Roles = "Candidate")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto)
        {
            try
            {
                string userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                await _documentService.UploadDocumentAsync(userId, dto);

                return Ok(new UploadResponseDto(
                    true,
                    "Document uploaded successfully",
                    dto.DocumentTypeId,
                    "Uploaded"
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new UploadResponseDto(false, "Document upload failed", dto.DocumentTypeId, "Failed"));
            }
        }

        [Authorize(Roles = "Candidate")]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitForVerificationDto dto)
        {
            try
            {
                string userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                await _documentService.SubmitForVerificationAsync(userId, dto);

                return Ok(new ApiResponse(
                    true,
                    "Documents submitted for verification"
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
        }

        [Authorize(Roles = "Candidate")]
        [HttpGet("existing")]
        public async Task<IActionResult> GetExistingDocuments([FromQuery] int jobId)
        {

            try
            {

                string userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                var result = await _documentService.GetCandidateVerificationDetailAsync(userId, jobId);

                return Ok(result);
            }catch(Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
        }

    }
}
