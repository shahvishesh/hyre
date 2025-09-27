using Hyre.API.Dtos;
using Hyre.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hyre.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        /// <summary>
        /// Create a new job
        /// </summary>
        [HttpPost]
        //[Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdByUserId = int.Parse(User.FindFirst("UserID").Value);

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
    }
}
