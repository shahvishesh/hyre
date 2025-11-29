using Hyre.API.Dtos.InterviewerJob;
using Hyre.API.Interfaces.InterviewerJob;
using Hyre.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Hyre.API.Services
{
    public class JobInterviewerService : IJobInterviewerService
    {
        private readonly IJobInterviewerRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobInterviewerService(
            IJobInterviewerRepository repo,
            UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        public async Task AssignInterviewersAsync(AssignInterviewersDto dto, string recruiterId)
        {
            foreach (var interviewerId in dto.InterviewerIDs)
            {
                if (await _repo.ExistsAsync(dto.JobID, interviewerId))
                    continue;

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
            await _repo.RemoveAsync(jobId, interviewerId);
        }

        public async Task<List<JobInterviewerDto>> GetAssignedInterviewersAsync(int jobId)
        {
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

    }
}
