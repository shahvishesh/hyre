using Hyre.API.Models;

namespace Hyre.API.Interfaces.CandidateFeedback
{
    public interface IInterviewFeedbackRepository
    {
        Task<bool> HasAccessToRoundAsync(int roundId, string interviewerId);
        Task<bool> FeedbackExistsAsync(int roundId, string interviewerId);
        Task AddFeedbackAsync(CandidateInterviewFeedback feedback);
        Task<List<CandidateInterviewFeedback>> GetFeedbacksByRoundAsync(int roundId);

        Task<List<CandidateInterviewRound>> GetCompletedRoundsForInterviewerAsync(string interviewerId);
        Task<List<CandidateInterviewFeedback>> GetFeedbacksByInterviewerAsync(string interviewerId);
        Task SaveAsync();
    }
}
