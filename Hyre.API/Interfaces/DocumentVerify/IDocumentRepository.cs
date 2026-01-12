using Hyre.API.Models;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Interfaces.DocumentVerify
{
    public interface IDocumentRepository
    {
        Task<CandidateDocumentVerification> GetVerificationAsync(string userId, int jobId);
        
        Task<List<DocumentType>> GetActiveDocumentTypesAsync();
    }
}
