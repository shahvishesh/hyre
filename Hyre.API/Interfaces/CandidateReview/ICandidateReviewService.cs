using static Hyre.API.Dtos.CandidateReview.ReviewDtos;

namespace Hyre.API.Interfaces.CandidateReview
{
    public interface ICandidateReviewService
    {
        Task<ReviewResponseDto> CreateReviewAsync(CreateReviewDto dto, string reviewerId);
        Task<ReviewResponseDto> UpdateReviewAsync(UpdateReviewDto dto, string reviewerId);
        Task AddCommentAsync(AddCommentDto dto, string commenterId);
        Task ApplyRecruiterDecisionAsync(RecruiterDecisionDto dto, string recruiterId);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByJobAsync(int jobId);
        Task<byte[]> GetCandidateResumeAsync(int candidateId, string requesterId, int jobId, IEnumerable<string> userRoles);
        Task<List<ReviewerJobDto>> GetJobsAssignedToReviewerAsync(string reviewerId);
        Task<List<ReviewerJobDto>> GetOpenJobsWithPendingReviewsAsync();
    }
}
