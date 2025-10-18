using Hyre.API.Dtos.Candidate;
using Hyre.API.Interfaces.Candidates;
using Hyre.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;
        private readonly UserManager<ApplicationUser> _userManager;
        public CandidateController(ICandidateService candidateService, UserManager<ApplicationUser> userManager)
        {
            _candidateService = candidateService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Recruiter,Admin,HR")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCandidate([FromForm] CreateCandidateDto dto, IFormFile? resume)
        {
            try
            {
                var createdByUserId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(createdByUserId)) return Unauthorized();
                var result = await _candidateService.CreateCandidateAsync(dto, createdByUserId, resume);

                return Ok(new
                {
                    message = result.ResumePath != null
                        ? "Candidate created successfully with resume."
                        : "Candidate created successfully (no resume uploaded).",
                    candidate = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected server error.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Recruiter,Admin,HR")]
        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Excel file missing.");

            var createdByUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(createdByUserId)) return Unauthorized();
            await _candidateService.ImportFromWorkbookAsync(file, createdByUserId);

            return Ok(new
            {
                message = $"Candidates imported successfully."
            });
        }
    }
}
