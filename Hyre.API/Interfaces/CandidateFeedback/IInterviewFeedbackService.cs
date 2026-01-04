using Hyre.API.Dtos.Feedback;

namespace Hyre.API.Interfaces.CandidateFeedback
{
    public interface IInterviewFeedbackService
    {
        Task SubmitFeedbackAsync(SubmitFeedbackDto dto, string interviewerId);
        Task<List<FeedbackResponseDto>> GetMyFeedbacksAsync(string interviewerId);
        Task<List<FeedbackResponseDto>> GetFeedbacksForRoundAsync(int roundId);
        Task<List<PendingFeedbackDto>> GetPendingFeedbackAsync(string interviewerId);
        Task<List<FeedbackResponseDto>> GetCompletedFeedbackAsync(string interviewerId);
        Task<List<InterviewerJobDto>> GetInterviewerJobsAsync(string interviewerId);
        Task<List<InterviewedCandidateDto>> GetInterviewedCandidatesForJobAsync(int jobId, string interviewerId);
        Task<List<PendingFeedbackDto>> GetPendingFeedbackForCandidateJobAsync(int candidateId, int jobId, string interviewerId);
        Task<List<CompletedFeedbackDto>> GetCompletedFeedbackForCandidateJobAsync(int candidateId, int jobId, string interviewerId);
        Task<RoundDetailDto> GetRoundDetailAsync(int candidateRoundId, string interviewerId);
    }
}
