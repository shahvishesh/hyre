using Hyre.API.Models;

namespace Hyre.API.Interfaces.Scheduling
{
    public interface ICandidateRoundRepository
    {
        Task<List<CandidateInterviewRound>> GetByCandidateAndJobAsync(int candidateId, int jobId);
        Task RemoveRoundsByIdsAsync(IEnumerable<int> ids);
        Task AddRoundsAsync(IEnumerable<CandidateInterviewRound> rounds);
        Task SaveChangesAsync();
    }
}
