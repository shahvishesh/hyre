using Hyre.API.Dtos.CandidateMatching;

namespace Hyre.API.Interfaces.CandidateMatching
{
    public interface ICandidateMatchingService
    {
        Task<MatchResultDto> GetMatchingCandidatesAsync(int jobId);
    }
}
