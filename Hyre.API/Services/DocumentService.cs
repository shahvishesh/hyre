using Hyre.API.Interfaces.DocumentVerify;
using static Hyre.API.Dtos.DocumentVerification.DocumentVerificationDtos;

namespace Hyre.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;

        public DocumentService(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RequiredDocumentDto>> GetRequiredDocumentsAsync(string userId, int jobId)
        {
            

            var verification = await _repository.GetVerificationAsync(userId, jobId);

            var documentTypes = await _repository.GetActiveDocumentTypesAsync();

            var result = new List<RequiredDocumentDto>();

            foreach (var docType in documentTypes)
            {
                var candidateDoc = verification.Documents
                    .FirstOrDefault(d =>
                        d.DocumentTypeId == docType.DocumentTypeId);

                result.Add(new RequiredDocumentDto(
                    docType.DocumentTypeId,
                    docType.Name,
                    docType.IsMandatory,
                    docType.AllowedFormats,
                    docType.DisplayOrder,
                    candidateDoc?.Status ?? "NotUploaded",
                    candidateDoc?.FilePath
                ));
            }

            return result;
        }

    }
}
