using Hyre.API.Dtos.Candidate;

namespace Hyre.API.Interfaces.Candidates
{
    public interface ICandidateService
    {
        Task<CandidateDto> CreateCandidateAsync(CreateCandidateDto dto, String createdByUserId, IFormFile? resumeFile = null);
        Task ImportFromWorkbookAsync(IFormFile file, string createdByUserId);
        Task<bool> CandidateExistsAsync(int candidateId);
        Task<List<CandidateDto>> GetAllCandidatesAsync();
        Task<CandidateDto?> GetCandidateByIdAsync(int candidateId);
        Task<(byte[] fileBytes, string fileName, string contentType)?> GetCandidateResumeAsync(int candidateId);
    }
}