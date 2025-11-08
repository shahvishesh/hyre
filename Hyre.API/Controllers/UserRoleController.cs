using Hyre.API.Interfaces;
using Hyre.API.Interfaces.ReviewerJob;
using Hyre.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobReviewerService _jobReviewerService;
        private readonly IJobService _jobService;

        public UserRoleController(
            UserManager<ApplicationUser> userManager,
            IJobReviewerService jobReviewerService,
            IJobService jobService)
        {
            _userManager = userManager;
            _jobReviewerService = jobReviewerService;
            _jobService = jobService;
        }


        [HttpGet("available-reviewers/{jobId}")]
        [Authorize(Roles = "Recruiter,Admin,HR")]
        public async Task<IActionResult> GetAvailableReviewersForJob(int jobId)
        {

            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                return NotFound(new { message = "Job not found" });
            }
            var allReviewers = await _userManager.GetUsersInRoleAsync("Reviewer");

            var assignedReviewers = await _jobReviewerService.GetJobReviewersAsync(jobId);

            var availableReviewers = allReviewers
                .Where(r => !assignedReviewers.Any(a => a.ReviewerId == r.Id))
                .Select(r => new
                {
                    r.Id,
                    Name = $"{r.FirstName} {r.LastName}",
                    r.Email
                })
                .OrderBy(r => r.Name)
                .ToList();

            return Ok(availableReviewers);
        }
    }
}
