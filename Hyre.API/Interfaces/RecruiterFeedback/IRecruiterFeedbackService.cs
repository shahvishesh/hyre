using Hyre.API.Dtos.RecruiterRoundDecesion;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterFeedbackService
    {
        Task<RoundAggregatedFeedbackDto> GetAggregatedFeedbackAsync(int roundId);
        Task<IEnumerable<PendingRecruiterDecisionDto>> GetRoundsByDecisionStateAsync(int candidateId, int jobId, RecruiterDecisionState decisionState);
    }
}
