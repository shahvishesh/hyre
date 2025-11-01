using Hyre.API.Models;

namespace Hyre.API.Interfaces.ReviewerJob
{
    public interface IJobReviewerRepository
    {
        Task AssignReviewersAsync(int jobId, List<string> reviewerIds, string assignedBy);
        Task<List<JobReviewer>> GetReviewersByJobIdAsync(int jobId);
        Task RemoveReviewerAsync(int jobId, string reviewerId);
        Task<bool> IsReviewerAssignedToJobAsync(string reviewerId, int jobId);
    }
}
