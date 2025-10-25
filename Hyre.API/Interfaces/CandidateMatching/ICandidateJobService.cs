using Hyre.API.Dtos.CandidateMatching;

namespace Hyre.API.Interfaces.CandidateMatching
{
    public interface ICandidateJobService
    {
        Task<CandidateJobResponseDto> LinkCandidateAsync(int jobId, CreateCandidateLinkDto dto, string createdByUserId);
    }
}
