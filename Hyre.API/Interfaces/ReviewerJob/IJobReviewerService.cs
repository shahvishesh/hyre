using static Hyre.API.Dtos.ReviewerJob.ReviewerJobDtos;

namespace Hyre.API.Interfaces.ReviewerJob
{
    public interface IJobReviewerService
    {
        Task AssignReviewersAsync(AssignReviewerDto dto, string recruiterId);
        Task<List<JobReviewerDto>> GetJobReviewersAsync(int jobId);
        Task RemoveReviewerAsync(int jobId, string reviewerId);
    }
}
