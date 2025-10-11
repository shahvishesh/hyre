using Hyre.API.Dtos;
using Hyre.API.Interfaces;
using Hyre.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(IJobService jobService, UserManager<ApplicationUser> userManager)
        {
            _jobService = jobService;
            _userManager = userManager;
        }

        /// <summary>
        /// Create a new job
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Recruiter,Admin")]
        [Authorize]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdByUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(createdByUserId)) return Unauthorized();

            var job = await _jobService.CreateJobAsync(dto, createdByUserId);
            return CreatedAtAction(nameof(GetJobById), new { jobId = job.JobID }, job);
        }

        /// <summary>
        /// Get all jobs
        /// </summary>
        [HttpGet]
        //[AllowAnonymous] 
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        /// <summary>
        /// Get job details by ID
        /// </summary>
        [HttpGet("{jobId}")]
        //[AllowAnonymous]
        public async Task<IActionResult> GetJobById(int jobId)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
                return NotFound(new { Message = "Job not found" });

            return Ok(job);
        }

        /// <summary>
        /// Update a job
        /// </summary>
        [HttpPut("{jobId}")]
        //[Authorize(Roles = "Recruiter,Admin")]
        [Authorize]
        public async Task<IActionResult> UpdateJob(int jobId, [FromBody] UpdateJobDto dto)
        {
            //var updatedByUserId = int.Parse(User.FindFirst("UserID").Value);
            try
            {
            var job = await _jobService.UpdateJobAsync(jobId, dto);
            if (job == null)
                return NotFound(new { Message = "Job not found" });

            return Ok(job);

            }catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message});
            }
        }

        /// <summary>
        /// Delete a job
        /// </summary>
        [HttpDelete("{jobId}")]
        [Authorize]
        //[Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            var success = await _jobService.DeleteJobAsync(jobId);
            if (!success)
                return NotFound(new { Message = "Job not found" });

            return NoContent();
        }
    }
}
