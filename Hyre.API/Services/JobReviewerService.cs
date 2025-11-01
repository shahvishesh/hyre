using Hyre.API.Interfaces;
using Hyre.API.Interfaces.ReviewerJob;
using static Hyre.API.Dtos.ReviewerJob.ReviewerJobDtos;

namespace Hyre.API.Services
{
    public class JobReviewerService : IJobReviewerService
    {
        private readonly IJobReviewerRepository _repo;

        public JobReviewerService(IJobReviewerRepository repo)
        {
            _repo = repo;
        }

        public async Task AssignReviewersAsync(AssignReviewerDto dto, string recruiterId)
        {
            await _repo.AssignReviewersAsync(dto.JobId, dto.ReviewerIds, recruiterId);
        }

        public async Task<List<JobReviewerDto>> GetJobReviewersAsync(int jobId)
        {
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
            await _repo.RemoveReviewerAsync(jobId, reviewerId);
        }
    }
}
