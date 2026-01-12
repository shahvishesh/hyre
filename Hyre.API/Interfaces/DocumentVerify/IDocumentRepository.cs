using Hyre.API.Models;

namespace Hyre.API.Interfaces.DocumentVerify
{
    public interface IDocumentRepository
    {
        Task<CandidateDocumentVerification> GetVerificationAsync(string userId, int jobId);
        Task<List<DocumentType>> GetActiveDocumentTypesAsync();
        Task<CandidateDocument?> GetCandidateDocumentAsync(int verificationId, int documentTypeId);
        Task AddAsync(CandidateDocument entity);
        Task UpdateAsync(CandidateDocument entity);
        Task UpdateVerificationAsync(CandidateDocumentVerification entity);
        Task<List<DocumentType>> GetMandatoryDocumentTypesAsync();
        Task<List<Job>> GetJobsAsync();
    }
}
