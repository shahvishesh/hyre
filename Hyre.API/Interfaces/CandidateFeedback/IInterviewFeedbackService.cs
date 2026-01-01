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
    }
}
