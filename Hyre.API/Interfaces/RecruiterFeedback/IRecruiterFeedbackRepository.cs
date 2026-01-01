using Hyre.API.Models;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterFeedbackRepository
    {
        Task<CandidateInterviewRound?> GetRoundWithFeedbackAsync(int roundId);
    }
}
