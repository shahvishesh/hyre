using Hyre.API.Dtos.Feedback;
using Hyre.API.Dtos.RecruiterRoundDecesion;

namespace Hyre.API.Interfaces.RecruiterFeedback
{
    public interface IRecruiterDecisionService
    {
        Task ApplyDecisionAsync(
            RecruiterRoundDecisionDto dto, string recruiterId);
        Task<RoundDetailDto?> GetNextRoundDetailAsync(int candidateId, int jobId, int currentSequenceNo);
        Task<RecruiterDecisionResultDto?> GetRecruiterDecisionAsync(int roundId);
    }
}
