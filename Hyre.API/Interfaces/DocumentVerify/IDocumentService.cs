using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Interfaces.DocumentVerify
{
    public interface IDocumentService
    {
        Task<List<RequiredDocumentDto>> GetRequiredDocumentsAsync(string candidateId,int jobId);
        Task UploadDocumentAsync(string userId, UploadDocumentDto dto);
        Task SubmitForVerificationAsync(string userId, SubmitForVerificationDto dto);
        Task<List<DocumentJobDto>> GetJobsWithPendingVerificationsAsync();
        Task<List<CandidateDetailDto>> GetCandidatesByVerificationStatusAsync(int jobId, string status);

        Task<HrVerificationDetailDto> GetVerificationForHrAsync(int verificationId);

    }
}
