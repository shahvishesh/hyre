using DocumentFormat.OpenXml.Spreadsheet;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.ReviewerJob;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;
using static Hyre.API.Dtos.ReviewerJob.ReviewerJobDtos;

namespace Hyre.API.Services
{
    public class JobReviewerService : IJobReviewerService
    {
        private readonly IJobReviewerRepository _repo;
        private readonly IJobService _jobService;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobReviewerService(IJobReviewerRepository repo, IJobService jobService, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _jobService = jobService;
            _userManager = userManager;
        }

        private async Task checkUsersExist(List<string> userIds)
        {
           // var users = new List<ApplicationUser>();
            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("One or more users not found");
                }
               // users.Add(user);
            }
            //return users;
        }

        public async Task AssignReviewersAsync(AssignReviewerDto dto, string recruiterId)
        {
            await checkUsersExist(dto.ReviewerIds);
            var job = await _jobService.GetJobByIdAsync(dto.JobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }
            await _repo.AssignReviewersAsync(dto.JobId, dto.ReviewerIds, recruiterId);
        }

        public async Task<List<JobReviewerDto>> GetJobReviewersAsync(int jobId)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }
            var reviewers = await _repo.GetReviewersByJobIdAsync(jobId);
            return reviewers.Select(r => new JobReviewerDto(
                r.JobReviewerId,
                r.JobId,
                r.ReviewerId,
                $"{r.Reviewer.FirstName} {r.Reviewer.LastName}",
                r.AssignedAt
            )).ToList();
        }

        public async Task RemoveReviewerAsync(int jobId, string reviewerId)
        {
            var user = await _userManager.FindByIdAsync(reviewerId);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }
            await _repo.RemoveReviewerAsync(jobId, reviewerId);
        }

        public async Task<List<JobResponseDto>> GetJobsByReviewerStatusAsync(string status)
        {
            var jobs = await _repo.GetJobsByReviewerStatusAsync(status);
            
            return jobs.Select(job => new JobResponseDto(
                job.JobID,
                job.Title,
                job.Description,
                job.MinExperience,
                job.MaxExperience,
                job.CompanyName,
                job.Location,
                job.JobType,
                job.WorkplaceType,
                job.Status,
                job.CreatedAt,
                job.JobSkills.Select(js => new JobSkillDetailDto(
                    js.SkillID,
                    js.Skill?.SkillName ?? "Unknown",
                    js.SkillType
                )).ToList()
            )).ToList();
        }
    }
}
