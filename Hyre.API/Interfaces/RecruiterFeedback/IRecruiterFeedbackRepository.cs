using Hyre.API.Dtos.RecruiterRoundDecesion;
using Hyre.API.Models;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterFeedbackRepository
    {
        Task<CandidateInterviewRound?> GetRoundWithFeedbackAsync(int roundId);
        Task<IEnumerable<CandidateInterviewRound>> GetRoundsByDecisionStateAsync(int candidateId, int jobId, RecruiterDecisionState decisionState);

        Task<List<Candidate>> GetInterviewedCandidatesForJobAsync(int jobId);

        Task<CandidateInterviewRound?> GetRoundByIdAsync(int candidateRoundId);
    }
}
