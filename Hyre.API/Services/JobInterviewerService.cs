using Hyre.API.Dtos.InterviewerJob;
using Hyre.API.Interfaces;
using Hyre.API.Interfaces.InterviewerJob;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Hyre.API.Services
{
    public class JobInterviewerService : IJobInterviewerService
    {
        private readonly IJobInterviewerRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobService _jobService;


        public JobInterviewerService(
            IJobInterviewerRepository repo,
            UserManager<ApplicationUser> userManager,
            IJobService jobService)
        {
            _repo = repo;
            _userManager = userManager;
            _jobService = jobService;
        }

        public async Task AssignInterviewersAsync(AssignInterviewersDto dto, string recruiterId)
        {

            var job = await _jobService.GetJobByIdAsync(dto.JobID);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            foreach (var interviewerId in dto.InterviewerIDs)
            {
                if (await _repo.ExistsAsync(dto.JobID, interviewerId))
                    continue;

                var user = await _userManager.FindByIdAsync(interviewerId);
                if (user == null)
                {
                    throw new Exception($"Interviewer with ID {interviewerId} not found");
                }

                var entity = new JobInterviewer
                {
                    JobID = dto.JobID,
                    InterviewerID = interviewerId,
                    Role = dto.Role,
                    SkillArea = dto.SkillArea,
                    AssignedBy = recruiterId
                };

                await _repo.AddAsync(entity);
            }
        }

        public async Task RemoveInterviewerAsync(int jobId, string interviewerId)
        {
            var user = await _userManager.FindByIdAsync(interviewerId);
            if (user == null)
            {
                throw new Exception($"Interviewer with ID {interviewerId} not found");
            }

            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            await _repo.RemoveAsync(jobId, interviewerId);
        }

        public async Task<List<JobInterviewerDto>> GetAssignedInterviewersAsync(int jobId)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            var list = await _repo.GetAssignedAsync(jobId);

            return list.Select(x => new JobInterviewerDto(
                x.InterviewerID,
                x.Interviewer.FirstName,
                x.Interviewer.LastName,
                x.Interviewer.Email,
                x.Role,
                x.SkillArea,
                x.AssignedAt
            )).ToList();
        }

        public async Task<List<JobInterviewerDto>> GetInterviewersByRoleAsync(int jobId, string role)
        {
            var job = await _jobService.GetJobByIdAsync(jobId);
            if (job == null)
            {
                throw new Exception("Job not found");
            }

            var list = await _repo.GetAssignedByRoleAsync(jobId, role);

            return list.Select(x => new JobInterviewerDto(
                x.InterviewerID,
                x.Interviewer.FirstName,
                x.Interviewer.LastName,
                x.Interviewer.Email,
                x.Role,
                x.SkillArea,
                x.AssignedAt
            )).ToList();
        }

        public async Task<List<InterviewerJobResponseDto>> GetJobsByInterviewerStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status) ||
                (!status.Equals("pending", StringComparison.OrdinalIgnoreCase) &&
                 !status.Equals("completed", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Status must be either 'pending' or 'completed'");
            }

            var jobs = await _repo.GetJobsByInterviewerStatusAsync(status);

            return jobs.Select(job => new InterviewerJobResponseDto(
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
                job.JobSkills?.Select(js => new JobSkillDetailDto(
                    js.SkillID,
                    js.Skill?.SkillName ?? string.Empty,
                    js.SkillType
                )).ToList() ?? new List<JobSkillDetailDto>()
            )).ToList();
        }

        public async Task<List<EmployeeDetailDto>> GetEmployeesBySystemRoleAsync(string role)
        {
            var employees =
                await _repo.GetEmployeesByRoleAsync(role);

            var result = new List<EmployeeDetailDto>();

            foreach (var e in employees)
            {
                var roles = await _userManager
                    .GetRolesAsync(e.User!);

                result.Add(new EmployeeDetailDto(
                    e.EmployeeID,
                    e.UserID!,  
                    $"{e.FirstName} {e.LastName}".Trim(),
                    e.User!.Email!,
                    e.Designation,
                    roles.First() 
                ));
            }

            return result;
        }

    }
}
