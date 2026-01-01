using Hyre.API.Models;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterDecisionRepository
    {
        Task<CandidateInterviewRound?> GetRoundAsync(int roundId);
        Task<CandidateInterviewRound?> GetNextRoundAsync(
            int candidateId, int jobId, int currentSequenceNo);

        Task<List<CandidateInterviewRound>> GetFutureRoundsAsync(
            int candidateId, int jobId, int fromSequenceNo);

        Task SaveAsync();
    }
}
