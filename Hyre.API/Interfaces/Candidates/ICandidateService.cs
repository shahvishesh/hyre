using Hyre.API.Dtos.Candidate;

namespace Hyre.API.Interfaces.Candidates
{
    public interface ICandidateService
    {
        Task<CandidateDto> CreateCandidateAsync(CreateCandidateDto dto, String createdByUserId, IFormFile? resumeFile = null);
        Task ImportFromWorkbookAsync(IFormFile file, string createdByUserId);
    }
}